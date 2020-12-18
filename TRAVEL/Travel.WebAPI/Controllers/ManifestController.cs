using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class ManifestController : BaseController
    {
        private readonly IManifestService _service;

        public ManifestController(IManifestService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetByVehicleTripId/{id}")]
        public async Task<IServiceResponse<ManifestDetailDTO>> GetManifestManagemtById(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                var manifestManagement = await _service.GetManifestById(id);

                return new ServiceResponse<ManifestDetailDTO>
                {
                    Object = manifestManagement
                };
            });
        }
        [HttpPut]
        [Route("OpenManifest")]
        public async Task<IServiceResponse<bool>> OpenManifest(ManifestDTO query)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.UpdateOpenManifest(query.VehicleTripRegistrationId);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("getvehicletrip/{busTripRegistrationId}")]
        public async Task<IServiceResponse<VehicleTripRegistrationDTO>> GetVehicleTripRegistrationById(Guid busTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var busTripRegistration = await _service.GetVehicleTripRegistrationDTO(busTripRegistrationId);

                return new ServiceResponse<VehicleTripRegistrationDTO>
                {
                    Object = busTripRegistration
                };
            });
        }
        [HttpGet]
        [Route("GetManifestTripFare/{subrouteId}/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<decimal?>> GetManifestTripFare(int subrouteId, Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _service.GetTripFare(subrouteId, vehicleTripRegistrationId);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetManifestPassengerFare/{passengerInfo}")]
        public async Task<IServiceResponse<decimal?>> GetManifestPassengerFare(string passengerInfo)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _service.GetPassengerFare(passengerInfo);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetManifestMainTripFare/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<decimal?>> GetManifestMainTripFare(Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _service.GetMainTripFare(vehicleTripRegistrationId);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("manifest/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<ManifestExt>> GetManifestById(Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestManagement = await _service.GetManifestByVehicleTripIdAsync(vehicleTripRegistrationId);

                return new ServiceResponse<ManifestExt>
                {
                    Object = manifestManagement
                };
            });
        }
        [HttpPut]
        [Route("UpdateDispatch")]
        public async Task<IServiceResponse<bool>> UpdateDispatchManifestManagement(ManifestExt manifestManagement)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateDispatchManifestManagement(manifestManagement);

                return new ServiceResponse<bool>();
            });
        }
        [HttpPut]
        [Route("UpdateBusType")]
        public async Task<IServiceResponse<bool>> UpdateBusType(VehicleTripRegistrationDTO manifestManagement)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateBusType(manifestManagement);

                return new ServiceResponse<bool>();
            });
        }

        [HttpGet]
        [Route("manifestPrint/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<ManifestDetailDTO>> PrintManifestManagemtById(Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestManagement = await _service.PrintManifestViewModelById(vehicleTripRegistrationId);

                return new ServiceResponse<ManifestDetailDTO>
                {
                    Object = manifestManagement
                };
            });
        }
        //Here we are
        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddManifestManagement(ManifestDTO manifestManagement)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.AddManifest(manifestManagement);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("UpdateFare/{Amount}")]
        public async Task<IServiceResponse<decimal?>> UpdateFare(NewTripIdDTO newTrip, decimal Amount)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.UpdateRouteFare(newTrip.vehicleTripReg, Amount);
                return new ServiceResponse<decimal?>(Amount);
            });
        }
    }
}