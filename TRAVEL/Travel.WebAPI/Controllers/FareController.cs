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
    public class FareController : BaseController
    {
        private readonly IFareService _fareService;

        public FareController(IFareService fareService)
        {
            _fareService = fareService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<FareDTO>>> GetFares(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var fares = await _fareService.GetFares(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<FareDTO>>
                {
                    Object = fares
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<FareDTO>> GetFareById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var fare = await _fareService.GetFareById(id);

                return new ServiceResponse<FareDTO>
                {
                    Object = fare
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddFare(FareDTO fare)
        {
            return await HandleApiOperationAsync(async () => {
                await _fareService.AddFare(fare);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateFare(int id, FareDTO fare)
        {
            return await HandleApiOperationAsync(async () => {
                await _fareService.UpdateFare(id, fare);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteFare(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _fareService.DeleteFare(id);

                return new ServiceResponse<bool>(true);
            });
        }


        [HttpGet]
        [Route("GetFareByRouteId/{id}")]
        public async Task<IServiceResponse<decimal>> GetFareByRouteId(int id, int modelId)
        {
            return await HandleApiOperationAsync(async () => {
                var fare =  _fareService.GetFareByRouteId(id, modelId);

                return new ServiceResponse<decimal>
                {
                    Object = fare
                };
            });
        }
    }
}