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
    public class StateController : BaseController
    {
        private readonly IStateService _stateService;

        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<StateDTO>>> GetStates(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var states = await _stateService.GetStates(pageNumber, pageSize);

                return new ServiceResponse<IPagedList<StateDTO>>
                {
                    Object = states
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<StateDTO>> GetStateById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var state = await _stateService.GetStateById(id);

                return new ServiceResponse<StateDTO>
                {
                    Object = state
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddState(StateDTO state)
        {
            return await HandleApiOperationAsync(async () => {
                await _stateService.AddState(state);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateState(int id, StateDTO state)
        {
            return await HandleApiOperationAsync(async () => {
                await _stateService.UpdateState(id, state);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteState(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _stateService.RemoveState(id);

                return new ServiceResponse<bool>(true);
            });
        }
    }
}