using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class FareCalendarController : BaseController
    {
        private readonly IFareCalendarService _FareCalendarService;

        public FareCalendarController(IFareCalendarService FareCalendarService)
        {
            _FareCalendarService = FareCalendarService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<FareCalendarDTO>>> GetFareCalendars(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var FareCalendars = await _FareCalendarService.GetFareCalendarsAsync(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<FareCalendarDTO>>
                {
                    Object = FareCalendars
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<FareCalendarDTO>> GetFareCalendarById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var FareCalendar = await _FareCalendarService.GetFareCalendarByIdAsync(id);

                return new ServiceResponse<FareCalendarDTO>
                {
                    Object = FareCalendar
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddFareCalendar(FareCalendarDTO FareCalendar)
        {
            return await HandleApiOperationAsync(async () => {
                await _FareCalendarService.AddFareCalendar(FareCalendar);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateFareCalendar(int id, FareCalendarDTO FareCalendar)
        {
            return await HandleApiOperationAsync(async () => {
                await _FareCalendarService.UpdateFareCalendar(id, FareCalendar);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteFareCalendar(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _FareCalendarService.DeleteFareCalendar(id);

                return new ServiceResponse<bool>(true);
            });
        }

    

    }
}