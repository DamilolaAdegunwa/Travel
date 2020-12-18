using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IPassportTypeService
    {
        IQueryable<PassportType> GetAll();
        Task<IPagedList<PassportTypeDTO>> GetPassportTypes(int page, int size, string query = null);
        Task<PassportTypeDTO> GetPassportTypeById(int passportTypeId);
        Task AddPassportType(PassportTypeDTO passportType);
        Task UpdatePassportType(int PassportTypeId, PassportTypeDTO passportType);
        Task RemovePassportType(int passportTypeId);
        Task<List<PassportTypeDTO>> GetPassportTypeAsync();
        Task<PassportTypeDTO> GetPassportTypeByRouteAndId(int id, int routeid);
        Task<PassportTypeDTO> GetPassportTypeByPassportTypeName(string Name);
        
    }

    public class PassportTypeService : IPassportTypeService
    {
        private readonly IRepository<PassportType> _passportTypeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public PassportTypeService(
            IRepository<PassportType> passportTypeRepo,
            IUnitOfWork unitOfWork,
            IServiceHelper serviceHelper)
        {
            _passportTypeRepo = passportTypeRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public IQueryable<PassportType> GetAll()
        {
            return _passportTypeRepo.GetAllIncluding(x => x.Id, y => y.Name, w => w.AddOnFare);
        }

        public Task<List<PassportTypeDTO>> GetPassportTypeAsync()
        {
            var passport =
                from passportType in _passportTypeRepo.GetAll()
                select new PassportTypeDTO
                {
                    Id = passportType.Id,
                    Name = passportType.Name,
                    Description = passportType.Description,
                    RouteId = passportType.RouteId,
                    AddOnFare = passportType.AddOnFare

                };

            return passport.AsNoTracking().ToListAsync();
        }

        //private async Task<bool> IsValidRegion(int passportTypeId)
        //{
        //    return regionId > 0 &&
        //         await _passportTypeRepo.ExistAsync(m => m.Id == regionId);
        //}

        public async Task AddPassportType(PassportTypeDTO passportType)
        {
            //if (!await IsValidRegion(passportType.Id)) {
            //    throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_NOT_EXIST);
            //}

            passportType.Name = passportType.Name.Trim();

            if (await _passportTypeRepo.ExistAsync(v => v.Name == passportType.Name)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_EXIST);
            }

            _passportTypeRepo.Insert(new PassportType
            {
                Id = passportType.Id,
                Name = passportType.Name,
                Description = passportType.Description,
                RouteId = passportType.RouteId,
                AddOnFare = passportType.AddOnFare
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PassportTypeDTO> GetPassportTypeById(int passportTypeId)
        {
            var passportType = await _passportTypeRepo.GetAsync(passportTypeId);

            if (passportType == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            return new PassportTypeDTO
            {
                Id = passportType.Id,
                Name = passportType.Name,
                Description = passportType.Description,
                RouteId = passportType.RouteId,
                AddOnFare = passportType.AddOnFare
            };
        }

        public Task<IPagedList<PassportTypeDTO>> GetPassportTypes(int page, int size, string query = null)
        {
            var passportTypes =
                from passportType in _passportTypeRepo.GetAll()
                where string.IsNullOrWhiteSpace(query) || passportType.Name.Contains(query)
                select new PassportTypeDTO
                {
                    Id = passportType.Id,
                    Name = passportType.Name,
                    Description = passportType.Description,
                    RouteId = passportType.RouteId,
                    AddOnFare = passportType.AddOnFare
                };

            return passportTypes.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task RemovePassportType(int PassportTypeId)
        {
            var passportType = await _passportTypeRepo.GetAsync(PassportTypeId);

            if (passportType == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            _passportTypeRepo.Delete(passportType);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdatePassportType(int passportTypeId, PassportTypeDTO passportType)
        {
            var passportTypes = await _passportTypeRepo.GetAsync(passportTypeId);

            if (passportTypes == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            passportTypes.Name = passportType.Name.Trim();

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<PassportTypeDTO> GetPassportTypeByRouteAndId(int id, int routeid)
        {
            var passportType = from passport in _passportTypeRepo.GetAll()
                               where passport.Id == id 
                               //&& passport.RouteId == routeid
                               select new PassportTypeDTO
                                {
                                    Id = passport.Id,
                                    Name = passport.Name,
                                    Description = passport.Description,
                                    RouteId = passport.RouteId,
                                    AddOnFare = passport.AddOnFare
                                };

            return passportType.SingleAsync();
        }

        public Task<PassportTypeDTO> GetPassportTypeName(int id)
        {
            var passportType = from passport in _passportTypeRepo.GetAll()
                               where passport.Id == id 
                               select new PassportTypeDTO
                               {
                                   Id = passport.Id,
                                   Name = passport.Name,
                                   Description = passport.Description,
                                   RouteId = passport.RouteId,
                                   AddOnFare = passport.AddOnFare
                               };

            return passportType.SingleAsync();
        }


        public Task<PassportTypeDTO> GetPassportTypeByPassportTypeName(string Name)
        {
            var passportType = from passport in _passportTypeRepo.GetAll()
                               where passport.Name == Name
                               select new PassportTypeDTO
                               {
                                   Id = passport.Id,
                                   Name = passport.Name,
                                   Description = passport.Description,
                                   RouteId = passport.RouteId,
                                   AddOnFare = passport.AddOnFare
                               };

            return passportType.SingleAsync();
        }


    }
}