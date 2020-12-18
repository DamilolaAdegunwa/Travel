using Travel.Core.Domain.Entities;
using Travel.Data.Repository;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IErrorCodeService
    {
        Task<ErrorCode> GetErrorByCodeAsync(string errorCode);
    }

    public class ErrorCodeService : IErrorCodeService
    {
        readonly IRepository<ErrorCode> _repository;

        public ErrorCodeService(IRepository<ErrorCode> repository)
        {
            _repository = repository;
        }

        public Task<ErrorCode> GetErrorByCodeAsync(string errorCode)
        {
            return _repository.FirstOrDefaultAsync(e => e.Code.ToLower() == errorCode.ToLower());
        }
    }
}