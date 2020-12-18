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
    public class RouteController : BaseController
    {
        private readonly IRouteService _routeService;
        private readonly IUserService _userManagerSvc;
        private readonly IServiceHelper _serviceHelper;
        public RouteController(IRouteService routeService, IUserService userManagerSvc, IServiceHelper serviceHelper)
        {
            _routeService = routeService;
            _userManagerSvc = userManagerSvc;
            _serviceHelper = serviceHelper;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddRoute(RouteDTO route)
        {
            return await HandleApiOperationAsync(async () => {
                await _routeService.AddRoute(route);
                return new ServiceResponse<bool>(true);
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<ServiceResponse<IPagedList<RouteDTO>>> GetRoutes(
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize,
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {

                var routes = await _routeService.GetRoutes(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<RouteDTO>>
                {
                    Object = routes
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<RouteDTO>> GetRouteById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var route = await _routeService.GetRouteById(id);

                return new ServiceResponse<RouteDTO>
                {
                    Object = route
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDestinationTerminals/{departureTerminalId}")]
        public async Task<ServiceResponse<List<TerminalDTO>>> GetDestinationTerminals(int departureTerminalId)
        {
            return await HandleApiOperationAsync(async () => {
                var destinationTerminals = await _routeService.GetDestinationTerminals(departureTerminalId);

                return new ServiceResponse<List<TerminalDTO>>
                {
                    Object = destinationTerminals
                };
            });
        }

        [HttpGet]
        [Route("GetRouteIdByDestinationAndDepartureId/{departureTerminalId}/{destinationTerminalId}")]
        public async Task<ServiceResponse<List<TerminalDTO>>> GetRouteIdByDestinationAndDepartureId(int departureTerminalId, int destinationTerminalId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var terminalDetails = await _routeService.GetRouteIdByDestinationAndDepartureId(departureTerminalId, destinationTerminalId);
                return new ServiceResponse<List<TerminalDTO>>
                {
                    Object = terminalDetails
                };
            });
        }

        [HttpGet]
        [Route("Terminals/managerroutes")]
        public async Task<IServiceResponse<List<RouteDTO>>> GetTerminalManagerRoutes()
        {

            var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            {

                return await HandleApiOperationAsync(async () =>
                {
                    var ticketerRoutes = await _routeService.GetStaffTerminalRoutes(email.Email);
                    return new ServiceResponse<List<RouteDTO>>
                    {
                        Object = ticketerRoutes
                    };
                });

            }
        }

     
        [HttpGet]
        [Route("Terminals/Routes/{terminalId}")]
        public async Task<ServiceResponse<List<RouteDTO>>> GetTerminalRoutes(int terminalId)
        {
            return await HandleApiOperationAsync(async () => {
                var destinationTerminals = await _routeService.GetTerminalRoutes(terminalId);

                return new ServiceResponse<List<RouteDTO>>
                {
                    Object = destinationTerminals
                };
            });
        }

        [HttpGet]
        [Route("Route/VirtualBuses/{routeId}")]
        public async Task<ServiceResponse<List<AvailableTripDetailDTO>>> GetRouteVirtualBuses(int routeId)
        {
            return await HandleApiOperationAsync(async () => {
                var routebuses = await _routeService.GetRouteVirtualBusesWithFareCalendar(routeId);

                return new ServiceResponse<List<AvailableTripDetailDTO>>
                {
                    Object = routebuses
                };
            });
        }

        [HttpGet]
        [Route("Route/PhysicalBuses/{routeId}")]
        public async Task<ServiceResponse<List<AvailableTripDetailDTO>>> GetRoutePhysicalBuses(int routeId)
        {
            return await HandleApiOperationAsync(async () => {
                var routebuses = await _routeService.GetRoutePhysicalBusesWithFareCalendar(routeId);

                return new ServiceResponse<List<AvailableTripDetailDTO>>
                {
                    Object = routebuses
                };
            });
        }

        [HttpGet]
        [Route("GetEmployeeRoutes")]
        public async Task<ServiceResponse<List<EmployeeRouteDTO>>> GetEmployeeRoutes()
        {
            return await HandleApiOperationAsync(async () => {
                var employeeroutes = await _routeService.GetEmployeeRoutes();

                return new ServiceResponse<List<EmployeeRouteDTO>>
                {
                    Object = employeeroutes
                };
            });
        }

        [HttpPost]
        [Route("AddEmployeeRoute")]
        public async Task<ServiceResponse<bool>> AddEmployeeRoute(EmployeeRouteDTO employeeroute)
        {
            return await HandleApiOperationAsync(async () => {
                await _routeService.AddEmployeeRoute(employeeroute);
                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<ServiceResponse<bool>> UpateRoute(int id, RouteDTO route)
        {
            return await HandleApiOperationAsync(async () => {
                await _routeService.UpdateRoute(id, route);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<ServiceResponse<bool>> DeleteRoute(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _routeService.RemoveRoute(id);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetEmployeeRoutebyId")]
        public async Task<ServiceResponse<EmployeeRouteDTO>> GetEmployeeRoutebyId(int employeerouteId)
        {
            return await HandleApiOperationAsync(async () => {
                var employeeroute = await _routeService.GetEmployeeRoutebyId(employeerouteId);

                return new ServiceResponse<EmployeeRouteDTO>
                {
                    Object = employeeroute
                };
            });
        }
    }
}