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
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleSvc;

        public RoleController(IRoleService roleSvc)
        {
            _roleSvc = roleSvc;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> Create(RoleDTO role)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _roleSvc.CreateAsync(role);

                return new ServiceResponse<bool>(result);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<ServiceResponse<IPagedList<RoleDTO>>> GetRoutes(
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize,
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {

                var roles = await _roleSvc.Get(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<RoleDTO>>
                {
                    Object = roles
                };
            });
        }
    }
}