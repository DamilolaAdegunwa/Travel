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
    public class FranchiseController : BaseController
    {
        private readonly IFranchiseService _franchiseService;

        public FranchiseController(IFranchiseService franchiseService)
        {
            _franchiseService = franchiseService;
         
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ServiceResponse<bool>> AddFranchise(FranchiseDTO franchise)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchiseService.AddFranchise(franchise);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<ServiceResponse<IPagedList<FranchiseDTO>>> GetFranchises(
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize,
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                IPagedList<FranchiseDTO> franchise;

                franchise = await _franchiseService.GetFranchises(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<FranchiseDTO>>
                {
                    Object = franchise
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<FranchiseDTO>> GetFranchiseById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var franchise = await _franchiseService.GetFranchiseById(id);

                return new ServiceResponse<FranchiseDTO>
                {
                    Object = franchise
                };
            });
        }

        [HttpDelete]
        [Route("Delete/{franchiseId}")]
        public async Task<IServiceResponse<bool>> DeleteFranchise(int franchiseId)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchiseService.RemoveFranchise(franchiseId);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateFranchise(int id, FranchiseDTO franchise)
        {
            return await HandleApiOperationAsync(async () => {
                await _franchiseService.UpdateFranchise(id, franchise);

                return new ServiceResponse<bool>(true);
            });
        }

      
   


    }
}