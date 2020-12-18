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
    public interface IVehicleModelService
    {

        Task<IPagedList<VehicleModelDTO>> GetVehicleModels(int page, int size, string query = null);
        Task<VehicleModelDTO> GetVehicleModelById(int id);
        Task AddVehicleModel(VehicleModelDTO vehicleModel);
        Task UpdateVehicleModel(int id, VehicleModelDTO vehicleModel);
        Task RemoveVehicleModel(int id);
    }

    public class VehicleModelService : IVehicleModelService
    {

        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<VehicleMake> _vehicleMakeRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleModelService(
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<VehicleMake> vehicleMakeRepo,
            IServiceHelper serviceHelper,
            IUnitOfWork unitOfWork
            )
        {
            _vehicleModelRepo = vehicleModelRepo;
            _vehicleMakeRepo = vehicleMakeRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task AddVehicleModel(VehicleModelDTO vehicleModelDto)
        {
            if (!await IsValidVehicleMake(vehicleModelDto.VehicleMakeId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MAKE_NOT_EXIST);
            }

            vehicleModelDto.Name = vehicleModelDto.Name.Trim();

            var vehicleModelName = vehicleModelDto.Name.ToLower();

            if (await _vehicleModelRepo.ExistAsync(v => v.Name.ToLower() == vehicleModelName)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_EXIST);
            }

            _vehicleModelRepo.Insert(new VehicleModel
            {
                Name = vehicleModelDto.Name,
                NumberOfSeats = vehicleModelDto.NumberOfSeats,
                VehicleMakeId = vehicleModelDto.VehicleMakeId
            });

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> IsValidVehicleMake(int vehicleMakeId)
        {
            return vehicleMakeId > 0 &&
                 await _vehicleMakeRepo.ExistAsync(m => m.Id == vehicleMakeId);
        }

        public async Task<VehicleModelDTO> GetVehicleModelById(int id)
        {
            var vehiclemodel = await _vehicleModelRepo.GetAsync(id);

            if (vehiclemodel == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            return new VehicleModelDTO
            {
                Id = vehiclemodel.Id,
                Name = vehiclemodel.Name,
                NumberOfSeats = vehiclemodel.NumberOfSeats,
                VehicleMakeId = vehiclemodel.VehicleMakeId
            };
        }

        public Task<IPagedList<VehicleModelDTO>> GetVehicleModels(int page, int size, string query = null)
        {
            var vehiclemodels =
                from vehiclemodel in _vehicleModelRepo.GetAll()
                join vehiclemake in _vehicleMakeRepo.GetAll() on vehiclemodel.VehicleMakeId equals vehiclemake.Id
                where query == null || (vehiclemodel.Name.Contains(query) || vehiclemake.Name.Contains(query))

                orderby vehiclemodel.CreationTime descending

                select new VehicleModelDTO
                {
                    Id = vehiclemodel.Id,
                    Name = vehiclemodel.Name,
                    NumberOfSeats = vehiclemodel.NumberOfSeats,
                    VehicleMakeId = vehiclemake.Id,
                    VehicleMakeName = vehiclemake.Name,
                    DateCreated=vehiclemodel.CreationTime
                };

            return vehiclemodels.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task RemoveVehicleModel(int id)
        {
            var model = await _vehicleModelRepo.GetAsync(id);

            if (model == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            _vehicleModelRepo.Delete(model);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateVehicleModel(int id, VehicleModelDTO vehicleModel)
        {
            var model = await _vehicleModelRepo.GetAsync(id);

            if (model == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            model.Name = vehicleModel.Name;
            model.NumberOfSeats = vehicleModel.NumberOfSeats;
            model.VehicleMakeId = vehicleModel.VehicleMakeId;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}