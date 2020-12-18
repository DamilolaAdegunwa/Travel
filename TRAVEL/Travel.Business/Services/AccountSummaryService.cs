using Travel.Data.Repository;
using System;
using Travel.Core.Domain.Entities;
using Travel.Core.Utils;

namespace Travel.Business.Services
{
    public interface IAccountSummaryService
    {
        AccountSummary CreateOrUpdateEntry(AccountSummary accountSummary);
    }

    public class AccountSummaryService : IAccountSummaryService
    {
        readonly IRepository<AccountSummary, Guid> _repo;
        private readonly IServiceHelper _serviceHelper;

        public AccountSummaryService(
            IRepository<AccountSummary, Guid> repo,
            IServiceHelper serviceHelper)
        {
            _repo = repo;
            _serviceHelper = serviceHelper;
        }

        public AccountSummary CreateOrUpdateEntry(AccountSummary accountSummary)
        {

            if (accountSummary is null) {
                throw new Exception("Null entry rejected");
            }

            var existing = _repo.FirstOrDefault(a => a.AccountName == accountSummary.AccountName);


            if (existing != null) {

                existing.Balance += accountSummary.Balance;
            }

            else {

                existing = new AccountSummary()
                {
                    Id = SequentialGuidGenerator.Instance.Create(),
                    IsDeleted = false,
                    CreatorUserId = _serviceHelper.GetCurrentUserId(),
                    AccountName = accountSummary.AccountName,
                    Balance = accountSummary.Balance,
                };

                _repo.Insert(existing);
            }
            return existing;
        }
    }
}