using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IVehicleMakeService
    {
        Task<IPagedList<VehicleMakeDTO>> GetVehicleMakes(int page, int size, string query = null);
        Task<VehicleMakeDTO> GetVehicleMakeById(int id);
        Task AddVehicleMake(VehicleMakeDTO vehicleMake);
        Task UpdateVehicleMake(int id, VehicleMakeDTO vehicleMakeDto);
        Task RemoveVehicleMake(int id);
    }

    public class VehicleMakeService : IVehicleMakeService
    {

        private readonly IRepository<VehicleMake> _vehicleMakeRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleMakeService(
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<VehicleMake> vehicleMakeRepo,
            IServiceHelper serviceHelper,
            IUnitOfWork unitOfWork
            )
        {
            _vehicleMakeRepo = vehicleMakeRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task AddVehicleMake(VehicleMakeDTO vehicleMakeDto)
        {
            vehicleMakeDto.Name = vehicleMakeDto.Name.Trim();

            if (await _vehicleMakeRepo.ExistAsync(v => v.Name == vehicleMakeDto.Name)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MAKE_EXIST);
            }

            _vehicleMakeRepo.Insert(new VehicleMake
            {
                Name = vehicleMakeDto.Name,
                CreatorUserId = _serviceHelper.GetCurrentUserId()
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<VehicleMakeDTO> GetVehicleMakeById(int id)
        {
            var vehiclemodel = await _vehicleMakeRepo.GetAsync(id);

            if (vehiclemodel == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MAKE_NOT_EXIST);
            }

            return new VehicleMakeDTO
            {
                Id = vehiclemodel.Id,
                Name = vehiclemodel.Name
            };
        }

        public Task<IPagedList<VehicleMakeDTO>> GetVehicleMakes(int page, int size, string query = null)
        {
            var vehiclemodels =
                from vehiclemodel in _vehicleMakeRepo.GetAll()
                where query == null || vehiclemodel.Name.Contains(query)
                orderby vehiclemodel.CreationTime descending

                select new VehicleMakeDTO
                {
                    Id = vehiclemodel.Id,
                    Name = vehiclemodel.Name,
                    DateCreated = vehiclemodel.CreationTime
                };

            return vehiclemodels.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task RemoveVehicleMake(int id)
        {
            var model = await _vehicleMakeRepo.GetAsync(id);

            if (model == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MAKE_NOT_EXIST);
            }

            _vehicleMakeRepo.Delete(model);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateVehicleMake(int id, VehicleMakeDTO vehicleMake)
        {
            var model = await _vehicleMakeRepo.GetAsync(id);

            if (model == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MAKE_NOT_EXIST);
            }

            model.Name = vehicleMake.Name;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}