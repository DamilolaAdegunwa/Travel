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
    public class VehicleMakeController : BaseController
    {

        private readonly IVehicleMakeService _vehicleMakeService;
        public VehicleMakeController(IVehicleMakeService vehicleMakeService)
        {
            _vehicleMakeService = vehicleMakeService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<VehicleMakeDTO>>> GetVehicleMakes(
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null
            )
        {
            return await HandleApiOperationAsync(async () => {
                var makes = await _vehicleMakeService.GetVehicleMakes(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<VehicleMakeDTO>>
                {
                    Object = makes
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<VehicleMakeDTO>> GetVehicleMakeById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var make = await _vehicleMakeService.GetVehicleMakeById(id);

                return new ServiceResponse<VehicleMakeDTO>
                {
                    Object = make
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddVehicleMake(VehicleMakeDTO vehicleMake)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleMakeService.AddVehicleMake(vehicleMake);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateVehicleMake(int id, VehicleMakeDTO vehicleMake)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleMakeService.UpdateVehicleMake(id, vehicleMake);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteVehicleMake(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleMakeService.RemoveVehicleMake(id);

                return new ServiceResponse<bool>(true);
            });
        }

    }
}