using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface IWalletService
    {
        Task<Wallet> GetAsync(int id);
        IQueryable<Wallet> GetAll();
        Task<WalletNumber> GenerateNextValidWalletNumber();
        void Add(Wallet wallet);
    }

    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _repo;
        private readonly IWalletNumberService _walletNumberSvc;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public WalletService(IRepository<Wallet> repo, IWalletNumberService walletNumberSvc,
            IUnitOfWork unitOfWork, IServiceHelper serviceHelper)
        {
            _repo = repo;
            _walletNumberSvc = walletNumberSvc;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public Wallet FirstOrDefault(Expression<Func<Wallet, bool>> predicate)
        {
            return _repo.FirstOrDefault(predicate);
        }

        public async Task<WalletNumber> GenerateNextValidWalletNumber()
        {
            var walletNumber = await _walletNumberSvc.GetLastValidWalletNumber();

            var walletPan = walletNumber?.WalletPan ?? "0";

            var number = long.Parse(walletPan) + 1;
            var numberStr = number.ToString("0000000000");

            return new WalletNumber
            {
                WalletPan = numberStr,
                IsActive = true
            };
        }

        public IQueryable<Wallet> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<Wallet> GetAsync(int id)
        {
            return _repo.GetAsync(id);
        }

        public void Add(Wallet wallet)
        {
            _repo.Insert(wallet);
        }
    }
}