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
    public class PassportTypeController : BaseController
    {
        private readonly IPassportTypeService _passportTypeService;

        public PassportTypeController(IPassportTypeService passportTypeService)
        {
            _passportTypeService = passportTypeService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<PassportTypeDTO>>> GetPassportTypes(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var passportTypes = await _passportTypeService.GetPassportTypes(pageNumber, pageSize);

                return new ServiceResponse<IPagedList<PassportTypeDTO>>
                {
                    Object = passportTypes
                };
            });
        }

        [HttpGet]
        [Route("GetPassportTypeAsync")]
        public async Task<IServiceResponse<List<PassportTypeDTO>>> GetPassportTypeAsync()
        {
            return await HandleApiOperationAsync(async () => {
                var passportTypes = await _passportTypeService.GetPassportTypeAsync();

                return new ServiceResponse<List<PassportTypeDTO>>
                {
                    Object = passportTypes
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<PassportTypeDTO>> GetPassportTypeById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var passportType = await _passportTypeService.GetPassportTypeById(id);

                return new ServiceResponse<PassportTypeDTO>
                {
                    Object = passportType
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddPassportType(PassportTypeDTO passportType)
        {
            return await HandleApiOperationAsync(async () => {
                await _passportTypeService.AddPassportType(passportType);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdatePassportType(int id, PassportTypeDTO passportType)
        {
            return await HandleApiOperationAsync(async () => {
                await _passportTypeService.UpdatePassportType(id, passportType);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeletePassportType(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _passportTypeService.RemovePassportType(id);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetByRouteAndId/{routeid}/{id}")]
        public async Task<IServiceResponse<PassportTypeDTO>> GetPassportTypeByRouteAndId(int id, int routeid)
        {
            return await HandleApiOperationAsync(async () => {
                var passportType = await _passportTypeService.GetPassportTypeByRouteAndId(id, routeid);

                return new ServiceResponse<PassportTypeDTO>
                {
                    Object = passportType
                };
            });
        }
    }
}