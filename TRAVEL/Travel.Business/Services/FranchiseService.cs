using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface IFranchiseService
    {
        Task<IPagedList<FranchiseDTO>> GetFranchises(int pageNumber, int pageSize, string searchTerm);
        Task<FranchiseDTO> GetFranchiseById(int id);
        Task AddFranchise(FranchiseDTO franchise);
        Task UpdateFranchise(int id, FranchiseDTO franchise);
        Task RemoveFranchise(int id);
    }

    public class FranchiseService : IFranchiseService
    {
        private readonly IRepository<Franchise> _franchise;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public FranchiseService(IRepository<Franchise> franchise, IUnitOfWork unitOfWork, IServiceHelper serviceHelper)
        {
            _franchise = franchise;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public async Task AddFranchise(FranchiseDTO franchisedto)
        {
            if (await IsdefinedFranchise(franchisedto.Id))
            {
                throw new LMEGenericException($"Fare already exist!");
            }

            var franchises = new Franchise
            {
                Id = franchisedto.Id,
                Name = franchisedto.Name,
                FirstName = franchisedto.FirstName,
                LastName = franchisedto.LastName,
                Code = franchisedto.Code,
                PhoneNumber = franchisedto.PhoneNumber
            };

            _franchise.Insert(franchises);
            await _unitOfWork.SaveChangesAsync();

        }

        private async Task <bool> IsdefinedFranchise(int id )
        {
            return await _franchise.ExistAsync(f => f.Id == id);
        }

        public async Task<FranchiseDTO> GetFranchiseById(int id)
        {
            var franchise = await _franchise.GetAsync(id);
            if (franchise == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FRANCHISE_NOT_EXIST);
            }

            return new FranchiseDTO
            {
                Id = franchise.Id,
                Name = franchise.Name,
                FirstName = franchise.FirstName,
                LastName = franchise.LastName,
                Code = franchise.Code,
                PhoneNumber = franchise.PhoneNumber
            };
        }

        public Task<IPagedList<FranchiseDTO>> GetFranchises(int pageNumber, int pageSize, string searchTerm)
        {
            var franchise = from franchises in _franchise.GetAll()
                            where (string.IsNullOrWhiteSpace(searchTerm) || franchises.Name.Contains(searchTerm))
                            orderby franchises.CreationTime descending

                            select new FranchiseDTO
                            {
                                Id = franchises.Id,
                                Name = franchises.Name,
                                FirstName = franchises.FirstName,
                                LastName = franchises.LastName,
                                Code = franchises.Code,
                                PhoneNumber = franchises.PhoneNumber
                            };

            return franchise.AsNoTracking().ToPagedListAsync( pageNumber , pageSize);
        }

        public async Task RemoveFranchise(int id)
        {
            var Franchise = await _franchise.GetAsync(id);

            if (Franchise == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FRANCHISE_NOT_EXIST);
            }

            _franchise.Delete(Franchise);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task UpdateFranchise(int id, FranchiseDTO franchise)
        {
            var Franchise = await _franchise.GetAsync(id);
            if (Franchise == null)
            {
                throw new LMEGenericException($"Transaction Not Exist");
            }
            Franchise.Id = franchise.Id;
            Franchise.Name = franchise.Name;
            Franchise.FirstName = franchise.FirstName;
            Franchise.LastName = franchise.LastName;
            Franchise.Code = franchise.Code;
            Franchise.PhoneNumber = franchise.PhoneNumber;

        }
    }

}