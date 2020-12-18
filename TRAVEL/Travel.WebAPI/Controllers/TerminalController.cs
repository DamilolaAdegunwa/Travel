using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using IPagedList;
using System.Collections.Generic;
using System.Threading.Tasks;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class TerminalController : BaseController
    {
        private readonly ITerminalService _terminalSvc;
        private readonly IRouteService _routeSvc;
        private readonly IUserService _userManagerSvc;
        private readonly IServiceHelper _serviceHelper;
        public TerminalController(ITerminalService terminalSvc,
            IRouteService routeSvc, IUserService userManagerSvc, IServiceHelper serviceHelper)
        {
            _terminalSvc = terminalSvc;
            _routeSvc = routeSvc;
            _userManagerSvc = userManagerSvc;
            _serviceHelper = serviceHelper;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddTerminal(TerminalDTO terminal)
        {
            return await HandleApiOperationAsync(async () => {
                await _terminalSvc.AddTerminal(terminal);

                return new ServiceResponse<bool>(true);
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<TerminalDTO>>> GetTerminals(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var terminals = await _terminalSvc.GetTerminals(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<TerminalDTO>>
                {
                    Object = terminals
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<TerminalDTO>> GetTerminalById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var terminal = await _terminalSvc.GetTerminalById(id);

                return new ServiceResponse<TerminalDTO>
                {
                    Object = terminal
                };
            });
        }


        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateTerminal(int id, TerminalDTO terminal)
        {
            return await HandleApiOperationAsync(async () => {
                await _terminalSvc.UpdateTerminal(id, terminal);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteTerminal(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _terminalSvc.RemoveTerminal(id);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetEmployeeTerminal")]
        public async Task<IServiceResponse<List<TerminalDTO>>> GetEmployeeTerminal()
        {
            var username = User.Identity.Name;
            return await HandleApiOperationAsync(async () => {
                var loginEmployeeTerminals = await _terminalSvc.GetEmployeeTerminal(CurrentUser?.Id ?? 0);

                return new ServiceResponse<List<TerminalDTO>>
                {
                    Object = loginEmployeeTerminals
                };
            });
        }

        [HttpGet]
        [Route("GetLoginEmployeeTerminal")]
        public async Task<IServiceResponse<TerminalDTO>> GetLoginEmployeeTerminal()
        {
            var username = CurrentUser.UserName;
            return await HandleApiOperationAsync(async () => {
                var loginEmployeeTerminals = await _terminalSvc.GetLoginEmployeeTerminal(username);

                return new ServiceResponse<TerminalDTO>
                {
                    Object = loginEmployeeTerminals
                };
            });
        }

        [HttpGet]
        [Route("GetTerminalTicketers/{id}")]
        public async Task<IServiceResponse<List<EmployeeDTO>>> GetTerminalTicketers(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var loginEmployeeTerminals = await _terminalSvc.GetTerminalTicketers(id);

                return new ServiceResponse<List<EmployeeDTO>>
                {
                    Object = loginEmployeeTerminals
                };
            });
        }

        [HttpGet]
        [Route("GetRoutes/{id}")]
        public async Task<IServiceResponse<List<RouteDTO>>> GetTerminalRoutes(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var routes = await _terminalSvc.GetTerminalRoutes(id);

                return new ServiceResponse<List<RouteDTO>>
                {
                    Object = routes
                };
            });
        }


        [HttpGet]
        [Route("GetTerminalAccountants/{id}")]
        public async Task<IServiceResponse<List<EmployeeDTO>>> GetTerminalAccountants(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var loginEmployeeTerminals = await _terminalSvc.GetTerminalAccountants(id);

                return new ServiceResponse<List<EmployeeDTO>>
                {
                    Object = loginEmployeeTerminals
                };
            });
        }

        [HttpGet]
        [Route("GetStaffTerminalRoutes")]
        public async Task<IServiceResponse<List<RouteDTO>>> GetStaffTerminalRoutes()
        {
            var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            return await HandleApiOperationAsync(async () => {
                var ticketerRoutes = await _routeSvc.GetStaffTerminalRoutes(email.Email);
                return new ServiceResponse<List<RouteDTO>>
                {
                    Object = ticketerRoutes
                };
            });
        }
    }
}