using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class VehicleModelController : BaseController
    {
        private readonly IVehicleModelService _vehicleModelService;

        public VehicleModelController(IVehicleModelService vehicleModelService)
        {
            _vehicleModelService = vehicleModelService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<VehicleModelDTO>>> GetVehicleModels(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var models = await _vehicleModelService.GetVehicleModels(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<VehicleModelDTO>>
                {
                    Object = models
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<VehicleModelDTO>> GetVehicleModelById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var model = await _vehicleModelService.GetVehicleModelById(id);

                return new ServiceResponse<VehicleModelDTO>
                {
                    Object = model
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddVehicleModel(VehicleModelDTO vehicleModel)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleModelService.AddVehicleModel(vehicleModel);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateVehicleMake(int id, VehicleModelDTO vehicleModel)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleModelService.UpdateVehicleModel(id, vehicleModel);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteVehicleMake(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleModelService.RemoveVehicleModel(id);

                return new ServiceResponse<bool>(true);
            });
        }

    }
}