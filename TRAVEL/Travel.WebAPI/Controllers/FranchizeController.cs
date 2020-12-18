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
    public class FranchizeController : BaseController
    {
        private readonly IFranchizeService _franchize;
        private readonly IServiceHelper _serviceHelper;

        public FranchizeController(IFranchizeService franchize, IServiceHelper serviceHelper, IUserService userManagerSvc)
        {
            _franchize = franchize;
            _serviceHelper = serviceHelper;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddFranchiseUser(FranchiseUserDTO FranchiseUser)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchize.AddFranchiseUser(FranchiseUser);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<FranchiseUserDTO>> GetFranchiseUser(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var franchiseUser = await _franchize.GetFranchiseUser(id);

                return new ServiceResponse<FranchiseUserDTO>(franchiseUser);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateFranchiseUser(int id, FranchiseUserDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchize.UpdateFranchiseUser(id, model);

                return new ServiceResponse<bool>(true);
            });
        }

     


        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{search}")]
        public async Task<IServiceResponse<IPagedList<FranchiseUserDTO>>> GetFranchiseUsers(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string search = null)
        {
            return await HandleApiOperationAsync(async () => {
                var FranchiseUsers = await _franchize.GetFranchiseUsers(pageNumber, pageSize, search);

                return new ServiceResponse<IPagedList<FranchiseUserDTO>>
                {
                    Object = FranchiseUsers
                };
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteFranchise(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchize.RemoveFranchise(id);

                return new ServiceResponse<bool>(true);
            });
        }



    }
}