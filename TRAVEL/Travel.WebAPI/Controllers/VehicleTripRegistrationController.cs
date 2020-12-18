using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class VehicleTripRegistrationController : BaseController
    {
        private readonly IVehicleTripRegistrationService _vtrSvc;

        public VehicleTripRegistrationController(IVehicleTripRegistrationService vtrSvc)
        {
            _vtrSvc = vtrSvc;
        }

        [HttpPost]
        [Route("CreateBus")]
        public async Task<IServiceResponse<bool>> CreatePhysicalBuses(TerminalBookingDTO physicalbus)
        {
            return await HandleApiOperationAsync(async () => {
                await _vtrSvc.CreatePhysicalBus(physicalbus);

                return new ServiceResponse<bool>(true);
            });
        }
        [HttpGet]
        [Route("GetByVehicleTripId/{id}")]
        public async Task<IServiceResponse<List<PassengerDto>>> GetAllPassengersInTrip(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                var passengers = await _vtrSvc.GetAllPassengersInTrip(id);
                return new ServiceResponse<List<PassengerDto>>(passengers);
            });
        }

        [HttpGet]
        [Route("GetByVehicleTripId/{id}/{bookingType}")]
        public async Task<IServiceResponse<List<PassengerDto>>> GetAllPassengersInTrip(Guid id, int bookingType)
        {
            return await HandleApiOperationAsync(async () => {
                var passengers = await _vtrSvc.GetAllPassengersInTrip(id, bookingType);
                return new ServiceResponse<List<PassengerDto>>(passengers);
            });
        }



        [HttpGet]
        [Route("{busTripRegistrationId}")]
        public async Task<IServiceResponse<VehicleTripRegistrationDTO>> GetVehicleTripRegistrationById(Guid busTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var busTripRegistration = await _vtrSvc.GetVehicleTripRegistrationDTO(busTripRegistrationId);

                return new ServiceResponse<VehicleTripRegistrationDTO>
                {
                    Object = busTripRegistration
                };
            });
        }
        [HttpPost]
        [Route("vehicletripbydrivercode")]
        public async Task<IServiceResponse<List<VehicleTripRegistrationDTO>>> VehicleTripsByDriverCode(SalaryReportQuery dto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var busTripRegistration = await _vtrSvc.GetVehicleTripByDriverCode(dto);

                return new ServiceResponse<List<VehicleTripRegistrationDTO>>
                {
                    Object = busTripRegistration
                };
            });
        }

        [HttpPut]
        [Route("")]
        public async Task<IServiceResponse<bool>> UpateVehicleTrip(VehicleTripRegistrationDTO trip)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _vtrSvc.UpdateVehcileTrip(trip);

                return new ServiceResponse<bool>(true);
            });
        }

        [Route("UpdateVehicleTripId")]
        [HttpPut]
        public async Task<IServiceResponse<bool>> UpdateVehicleTripId(NewTripIdDTO newTrip)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _vtrSvc.UpdateVehicleTripId(newTrip.vehicleTripReg, newTrip.newTripId);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetTodayVehicleTripRegistrationsForterminal")]
        public async Task<IServiceResponse<List<VehicleTripRegistrationDTO>>> GetvehicleTripByTerminalId()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var busTripRegistrations = await _vtrSvc.GetvehicleTripByTerminalId();

                return new ServiceResponse<List<VehicleTripRegistrationDTO>>
                {
                    Object = busTripRegistrations
                };
            });
        }

        [HttpPost]
        [Route("CreateBlow")]
        public async Task<IServiceResponse<bool>> CreateBlownBuses(VehicleTripRegistrationDTO blowVehicleDetails)
        {
            return await HandleApiOperationAsync(async () => {
                await _vtrSvc.BlowVehicle(blowVehicleDetails);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPost]
        [Route("GetBlown")]
        [Route("GetBlown/{pageNumber}/{pageSize}")]
        [Route("GetBlown/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<VehicleTripRegistrationDTO>>> GetBlown(DateModel search, int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var BlownVehicle = await _vtrSvc.GetBlownVehicleAsync(search, pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<VehicleTripRegistrationDTO>>
                {
                    Object = BlownVehicle
                };
            });
        }

        [HttpPost]
        [Route("GetFleetHistory")]
        [Route("GetFleetHistory/{pageNumber}/{pageSize}")]
        [Route("GetFleetHistory/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<FleetHistoryDTO>>> GetFleetHistory(SearchDTO search,
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize,
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                IPagedList<FleetHistoryDTO> fleet;

                fleet = await _vtrSvc.SearchFleetHistory(search, pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<FleetHistoryDTO>>
                {
                    Object = fleet
                };
            });
        }
    }
}