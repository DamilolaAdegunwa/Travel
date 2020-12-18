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
    public class TripController : BaseController
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<ServiceResponse<IPagedList<TripDTO>>> GetTrips(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize,string query=null)
        {
            return await HandleApiOperationAsync(async () => {
                var trips = await _tripService.GetTrips(pageNumber, pageSize,query);

                return new ServiceResponse<IPagedList<TripDTO>>
                {
                    Object = trips
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<TripDTO>> GetTripById(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                var accountT = await _tripService.GetTripById(id);

                return new ServiceResponse<TripDTO>
                {
                    Object = accountT
                };
            });
        }

        [HttpGet]
        [Route("GetByRouteId/{id}")]
        public async Task<ServiceResponse<List<TripDTO>>> GetTripByRouteId(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var tripsbyroute = await _tripService.GetTripByRouteId(id);

                return new ServiceResponse<List<TripDTO>>
                {
                    Object = tripsbyroute
                };
            });
        }

        [HttpGet]
        [Route("GetByVehicleId/{id}")]
        public async Task<ServiceResponse<List<TripDTO>>> GetTripsByVehicle(string id)
        {
            return await HandleApiOperationAsync(async () => {
                var trips = await _tripService.GetTripsByVehicleId(id);

                return new ServiceResponse<List<TripDTO>>
                {
                    Object = trips
                };
            });
        }

        [HttpPut]
        [Route("ToggleOnlineStatus/{uId}")]
        public async Task<ServiceResponse<bool>> ToggleOnlineStatus(Guid uId)
        {
            return await HandleApiOperationAsync(async () => {
                await _tripService.ToggleOnlineStatus(uId);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ServiceResponse<bool>> AddTrip(TripDTO trip)
        {
            return await HandleApiOperationAsync(async () => {
                await _tripService.AddTrip(trip);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<ServiceResponse<bool>> UpdateTrip(Guid id, TripDTO trip)
        {
            return await HandleApiOperationAsync(async () => {
                await _tripService.UpdateTrip(id, trip);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<ServiceResponse<bool>> DeleteTrip(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                await _tripService.RemoveTrip(id);

                return new ServiceResponse<bool>(true);
            });
        }
    }
}