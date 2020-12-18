using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class SubRouteController : BaseController
    {
        private readonly ISubRouteService _service;

        public SubRouteController(ISubRouteService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetByRouteId/{id}")]
        public async Task<IServiceResponse<List<SubRouteDTO>>> GetRouteById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var routes = await _service.GetRouteById(id);

                return new ServiceResponse<List<SubRouteDTO>>
                {
                    Object = routes
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddSubRoute(SubRouteDTO subroute)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.AddSubRoute(subroute);
                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<SubRouteDTO>> Get(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var routes = await _service.GetSubRouteById(id);

                return new ServiceResponse<SubRouteDTO>
                {
                    Object = routes
                };
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateSubRoute(int id, SubRouteDTO subroute)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.UpdateSubRoute(id, subroute);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetByRouteViewId/{Id}")]
        public async Task<IServiceResponse<List<SubRouteViewModel>>> GetByRouteViewId(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var routes = await _service.GetByRouteViewId(id);

                return new ServiceResponse<List<SubRouteViewModel>>
                {
                    Object = routes
                };
            });
        }
        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{search}")]
        public async Task<IServiceResponse<IPagedList<SubRouteDTO>>> GetSubRoutes(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string search = null)
        {
            return await HandleApiOperationAsync(async () => {

                var routes = await _service.GetSubRoutes(pageNumber, pageSize, search);

                return new ServiceResponse<IPagedList<SubRouteDTO>>
                {
                    Object = routes
                };
            });
        }
    }
}