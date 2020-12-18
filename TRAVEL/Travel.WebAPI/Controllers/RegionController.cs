using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class RegionController : BaseController
    {
        private readonly IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<RegionDTO>>> GetRegions(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var regions = await _regionService.GetRegions(pageNumber, pageSize);

                return new ServiceResponse<IPagedList<RegionDTO>>
                {
                    Object = regions
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<RegionDTO>> GetRegionById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var region = await _regionService.GetRegionById(id);

                return new ServiceResponse<RegionDTO>
                {
                    Object = region
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddRegion(RegionDTO region)
        {
            return await HandleApiOperationAsync(async () => {
                await _regionService.AddRegion(region);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateRegion(int id, RegionDTO region)
        {
            return await HandleApiOperationAsync(async () => {
                await _regionService.UpdateRegion(id, region);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteRegion(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _regionService.RemoveRegion(id);

                return new ServiceResponse<bool>(true);
            });
        }
    }
}