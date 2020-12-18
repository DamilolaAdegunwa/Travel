using Travel.Core.Domain.Entities;
using Travel.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IWalletNumberService
    {
        Task<WalletNumber> GetLastValidWalletNumber();
    }

    public class WalletNumberService : IWalletNumberService
    {
        private readonly IRepository<WalletNumber> _repo;

        public WalletNumberService(IRepository<WalletNumber> repo)
        {
            _repo = repo;
        }

        public Task<WalletNumber> GetLastValidWalletNumber()
        {
            var wallets =
                from walletNumber in _repo.GetAll()
                orderby walletNumber.WalletPan descending
                select walletNumber;

            return wallets.FirstOrDefaultAsync();
        }
    }
}
