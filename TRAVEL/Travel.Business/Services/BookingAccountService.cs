using Travel.Core.Timing;

using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using System;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Domain.Entities;
using Travel.Core.Utils;

namespace Travel.Business.Services
{
    public interface IAccountTransactionService
    {
        void UpdateBookingAccount(BookingTypes bookingType, PaymentMethod paymentMethod, string rfcode, Guid vehicletripRegId, decimal amount, TransactionType transType = TransactionType.Credit);
    }

    public class AccountTransactionService : IAccountTransactionService
    {
        IRepository<AccountTransaction, Guid> _repo;
        IAccountSummaryService _AccountSummarySvc;
        IServiceHelper _serviceHelper;
        IUnitOfWork _unitOfWork;

        public AccountTransactionService(
            IRepository<AccountTransaction, Guid> repo,
            IAccountSummaryService accountSummarySvc,
            IServiceHelper serviceHelper, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _AccountSummarySvc = accountSummarySvc;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
        }

        public void UpdateBookingAccount(BookingTypes bookingType, PaymentMethod paymentMethod, string refCode, Guid vehicletripRegId, decimal amount, TransactionType transType = TransactionType.Credit)
        {
            if (
                 (bookingType == BookingTypes.Advanced || bookingType == BookingTypes.Terminal)
                 && (paymentMethod == PaymentMethod.Cash || paymentMethod == PaymentMethod.CashAndPos)
                  ) {

                AddEntry(vehicletripRegId, refCode, amount, transType);
            }
        }

        private void AddEntry(Guid vehicleTripRegistrationId, string refcode, decimal amount, TransactionType transType = TransactionType.Credit)
        {
            var transEntry = new AccountTransaction
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                AccountType = AccountType.BookingAccount,
                LastModificationTime = Clock.Now,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                TransactionDate = Clock.Now,
                TransactionSourceId = vehicleTripRegistrationId,
                Narration = refcode
            };

            var summary = new AccountSummary
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                AccountName = _serviceHelper.GetCurrentUserEmail()
            };

            switch (transType) {
                case TransactionType.Credit:

                    transEntry.TransactionType = transType;
                    transEntry.Amount = Convert.ToDouble(amount);
                    summary.Balance = Convert.ToDouble(amount);

                    break;
                case TransactionType.Debit:

                    transEntry.TransactionType = transType;
                    transEntry.Amount = -Convert.ToDouble(amount);
                    summary.Balance = -Convert.ToDouble(amount);
                    break;
            }

            _repo.Insert(transEntry);
            _AccountSummarySvc.CreateOrUpdateEntry(summary);
        }
    }
}