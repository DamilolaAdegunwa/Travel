using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Core.Timing;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Business.Services
{
    public interface IRouteService
    {
        Task<IPagedList<RouteDTO>> GetRoutes(int pageNumber, int pageSize, string query = null);
        Task<RouteDTO> GetRouteById(int routeId);
        Task<List<TerminalDTO>> GetDestinationTerminals(int departureTerminalId);
        Task<List<RouteDTO>> GetTerminalRoutes(int terminalId);
        Task AddRoute(RouteDTO route);
        Task<List<AvailableTripDetailDTO>> GetRouteVirtualBuses(int routeId);
        Task<List<AvailableTripDetailDTO>> GetRouteVirtualBusesWithFareCalendar(int routeId);
        Task<List<AvailableTripDetailDTO>> GetRoutePhysicalBuses(int routeId);
        Task<List<AvailableTripDetailDTO>> GetRoutePhysicalBusesWithFareCalendar(int routeId);
        Task<List<EmployeeRouteDTO>> GetEmployeeRoutes();
        Task AddEmployeeRoute(EmployeeRouteDTO employeeroute);
        Task UpdateRoute(int routeId, RouteDTO route);
        Task RemoveRoute(int routeId);
        Task<EmployeeRouteDTO> GetEmployeeRoutebyId(int employeerouteId);
        IQueryable<Route> GetAll();
        Task<Route> SingleOrDefaultAsync(Expression<Func<Route, bool>> predicate);
        Task<string> GetParentRouteNameAsync(int? parentRoutId);
        Task<List<RouteDTO>> GetStaffTerminalRoutes(string username);
        Task<string> GetDepartureterminalFromRoute(int? RouteId);
        Task<int> GetDepartureterminalIdFromRoute(int? RouteId);
        Task<string> GetDestinationterminalFromRoute(int? RouteId);
        Task<string> GetDestinationTerminalCodeFromRoute(int? RouteId);
        Task<string> GetDepartureCodeFromRoute(int? RouteId);
        Task<List<RouteDTO>> GetLoginEmployeeRoutes();
        Task<EmployeeRouteDTO> GetEmployeeRouteById(int EmployeeId, int RouteId);
        Task<List<TerminalDTO>> GetRouteIdByDestinationAndDepartureId(int departureTerminalId, int destinationTerminalId);
    }

    public class RouteService : IRouteService
    {
        private readonly IRepository<SeatManagement, long> _seatMgtRepo;
        private readonly IRepository<VehicleTripRegistration, Guid> _vehicleTripRegRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<FareCalendar> _fareCalendarRepo;
        private readonly IRepository<Fare> _fareRepo;
        private readonly IRepository<Trip, Guid> _tripRepo;
        private readonly IRepository<ExcludedSeat> _excludedSeatRepo;
        private readonly IRepository<Route> _repo;
        private readonly IRepository<State> _stateRepo;
        private readonly IRepository<Driver> _driverRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<EmployeeRoute, long> _employeeRouteRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeService _employeeSvc;
        private readonly IFareCalendarService _fareCalendarSvc;
        private readonly IUserService _userSvc;
        public RouteService(IRepository<SeatManagement, long> seatMgtRepo, IUserService userSvc,
            IRepository<VehicleTripRegistration, Guid> vehicleTripRegRepo,
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<FareCalendar> fareCalendarRepo, IRepository<Fare> fareRepo,
            IRepository<Trip, Guid> tripRepo, IRepository<ExcludedSeat> excludedSeatRepo,
            IRepository<Route> routeRepo, IRepository<State> stateRepo, IRepository<Driver> driverRepo,
            IRepository<Terminal> terminalRepo, IRepository<EmployeeRoute, long> employeeRouteRepo,
            IRepository<Employee> employeeRepo, IFareCalendarService fareCalendarSvc,
            IServiceHelper serviceHelper, IUnitOfWork unitOfWork, IEmployeeService employeeSvc)
        {
            _seatMgtRepo = seatMgtRepo;
            _userSvc = userSvc;
            _employeeSvc = employeeSvc;
            _fareCalendarSvc = fareCalendarSvc;
            _vehicleTripRegRepo = vehicleTripRegRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _fareCalendarRepo = fareCalendarRepo;
            _fareRepo = fareRepo;
            _tripRepo = tripRepo;
            _excludedSeatRepo = excludedSeatRepo;
            _repo = routeRepo;
            _stateRepo = stateRepo;
            _driverRepo = driverRepo;
            _terminalRepo = terminalRepo;
            _employeeRouteRepo = employeeRouteRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
            _employeeRepo = employeeRepo;
        }

        public async Task AddEmployeeRoute(EmployeeRouteDTO employeeroute)
        {
            var employeerouteResult = await GetEmployeeRouteById(employeeroute.EmployeeId.GetValueOrDefault(), employeeroute.RouteId.GetValueOrDefault());

            if (employeerouteResult == null) {
                _employeeRouteRepo.Insert(new EmployeeRoute()
                {
                    TerminalId = employeeroute.TerminalId,
                    EmployeeId = employeeroute.EmployeeId,
                    RouteId = employeeroute.RouteId
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<EmployeeRouteDTO> GetEmployeeRoute(EmployeeRouteDTO employeeroute)
        {
            var employeeroutes =
                from employeerouteresult in _employeeRouteRepo.GetAll()
                where employeerouteresult.EmployeeId == employeeroute.EmployeeId && employeerouteresult.RouteId == employeeroute.RouteId

                select new EmployeeRouteDTO
                {
                    Id = employeeroute.Id,
                };

            return employeeroutes.FirstOrDefaultAsync();
        }

        public Task<EmployeeRouteDTO> GetEmployeeRouteById(int EmployeeId, int RouteId)
        {
            var employeeroutes =
                from employeerouteresult in _employeeRouteRepo.GetAll()
                where employeerouteresult.EmployeeId == EmployeeId && employeerouteresult.RouteId == RouteId

                select new EmployeeRouteDTO
                {
                    Id = employeerouteresult.Id,
                    EmployeeId = employeerouteresult.EmployeeId
                };

            return employeeroutes.FirstOrDefaultAsync();
        }

        public Task<List<TerminalDTO>> GetDestinationTerminals(int departureTerminalId)
        {
            var routes =
                from route in GetAll()
                join destination in _terminalRepo.GetAll() on route.DestinationTerminalId equals destination.Id
                join state in _stateRepo.GetAll() on destination.StateId equals state.Id
                where route.DepartureTerminalId == departureTerminalId && route.AvailableOnline

                select new TerminalDTO
                {
                    Id = destination.Id,
                    Name = state.Name + "-" + destination.Name
                };

            return routes.AsNoTracking().ToListAsync();
        }

        public Task<List<EmployeeRouteDTO>> GetEmployeeRoutes()
        {
            var employeeroutes =
                 from employeeroute in _employeeRouteRepo.GetAllIncluding(x => x.Employee, x => x.Employee.User)

                 select new EmployeeRouteDTO
                 {
                     Id = employeeroute.EmployeeRouteId,
                     TerminalId = employeeroute.TerminalId,
                     TerminalName = employeeroute.Terminal.Name,
                     EmployeeId = employeeroute.EmployeeId,
                     EmployeeName = employeeroute.Employee.User.FirstName + " " + employeeroute.Employee.User.LastName,
                     RouteId = employeeroute.RouteId,
                     RouteName = employeeroute.Route.Name
                 };

            return employeeroutes.AsNoTracking().ToListAsync();
        }

        public async Task<RouteDTO> GetRouteById(int routeId)
        {
            var route = await _repo.GetAsync(routeId);

            if (route is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            return new RouteDTO
            {
                Id = route.Id,
                Name = route.Name,
                RouteType = route.Type,
                DriverFee = route.DriverFee,
                DispatchFee = route.DispatchFee,
                LoaderFee = route.LoaderFee,
                AvailableOnline = route.AvailableOnline,
                AvailableAtTerminal = route.AvailableAtTerminal,
                ParentRouteId = route.ParentRouteId,
                DestinationTerminalId = route.DestinationTerminalId,
                DepartureTerminalId = route.DepartureTerminalId,
                ParentRouteName = route.ParentRoute
            };
        }

        public Task<List<AvailableTripDetailDTO>> GetRoutePhysicalBuses(int routeId)
        {
            var departureDate = DateTime.Now.Date;

            var excludedSeats = _excludedSeatRepo.GetAll().Select(s => s.SeatNumber);

            //var fareCalendar =
            //    _fareCalendarRepo.FirstOrDefault(c => departureDate >= c.StartDate.Date && c.EndDate.Date >= departureDate && routeId == c.RouteId);

            //var fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault();

            var routes =
                from route in GetAll()
                join trip in _tripRepo.GetAll() on route.Id equals trip.RouteId
                join vehicletripregistration in _vehicleTripRegRepo.GetAll() on trip.Id equals vehicletripregistration.TripId
                join vehicleModel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehicleModel.Id
                join fare in _fareRepo.GetAll() on new { RouteId = route.Id, VehicleModelId = vehicleModel.Id } equals new { fare.RouteId, fare.VehicleModelId }

                let bookedSeats = _seatMgtRepo.GetAll().Where(s => s.VehicleTripRegistrationId == vehicletripregistration.Id)
                let bookedSeat = _seatMgtRepo.GetAll()
                        .Where(s => s.VehicleTripRegistrationId == vehicletripregistration.Id)
                        .Select(s => (int) s.SeatNumber)
                let availableNumberOfSeats = vehicleModel.NumberOfSeats

                let driver = _driverRepo.GetAll().Where(s => s.Code == vehicletripregistration.DriverCode)
                let originaldriver = _driverRepo.GetAll().Where(s => s.Code == vehicletripregistration.OriginalDriverCode)
                where route.Id == routeId && vehicletripregistration.DepartureDate == departureDate && vehicletripregistration.IsVirtualBus == false && vehicletripregistration.ManifestPrinted == false

                select new AvailableTripDetailDTO
                {
                    VehicleTripRegistrationId = vehicletripregistration.Id,
                    RouteName = route.Name,
                    RouteId = routeId,
                    VehicleModelId = vehicletripregistration.VehicleModel.Id,
                    VehicleName = vehicletripregistration.VehicleModel.VehicleMake.Name + " (" + vehicletripregistration.VehicleModel.Name + ")",
                    DriverCode = driver.Any() && driver.FirstOrDefault().DriverType == DriverType.Virtual ? vehicletripregistration.OriginalDriverCode : vehicletripregistration.DriverCode,
                    PhysicalBus = vehicletripregistration.PhysicalBusRegistrationNumber,
                    DepartureTime = trip.DepartureTime,
                    FarePrice = fare.Amount,
                    AvailableNumberOfSeats = availableNumberOfSeats - bookedSeats.Count(),
                    BookedSeat = bookedSeats.Count(),
                    BookedSeats = bookedSeats.Select(s => (int) s.SeatNumber),
                    DepartureDate = vehicletripregistration.DepartureDate,
                    TotalNumberOfSeats = vehicletripregistration.VehicleModel.NumberOfSeats,
                    ExcludedSeats = excludedSeats.Union(bookedSeat)
                };

            return routes.AsNoTracking().ToListAsync();
        }
        public async Task<List<AvailableTripDetailDTO>> GetRoutePhysicalBusesWithFareCalendar(int routeId)
        {
            var availabletrips = await GetRoutePhysicalBuses(routeId);
            foreach (var trip in availabletrips) {
                var fareCalendar = new FareCalendarDTO();
                fareCalendar = null;
                var farecalendarList = new List<FareCalendarDTO>();

                farecalendarList = await _fareCalendarSvc.GetFareCalendaListByRoutesAsync(trip.RouteId, trip.DepartureDate);
                foreach (var calendar in farecalendarList) {
                    if (calendar.VehicleModelId != null) {
                        if (trip.VehicleModelId == calendar.VehicleModelId) {
                            fareCalendar = calendar;

                        }
                        break;
                    }

                    fareCalendar = calendar;
                }
                if (fareCalendar == null) {
                    var DepartureterminalId = await GetDepartureterminalIdFromRoute(trip.RouteId);
                    farecalendarList = await _fareCalendarSvc.GetFareCalendaListByTerminalsAsync(DepartureterminalId, trip.DepartureDate);
                    foreach (var calendar in farecalendarList) {
                        if (calendar.VehicleModelId != null) {
                            if (trip.VehicleModelId == calendar.VehicleModelId) {
                                fareCalendar = calendar;

                            }
                            break;
                        }

                        fareCalendar = calendar;
                    }
                }


                decimal fareDifference = 0;
                if (fareCalendar != null) {


                    if (fareCalendar.FareAdjustmentType == FareAdjustmentType.Percentage) {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.FarePrice / 100);

                    }
                    else {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                    }
                }

                trip.FarePrice += fareDifference;

            }
            return availabletrips;
        }
        public Task<IPagedList<RouteDTO>> GetRoutes(int pageNumber, int pageSize, string query)
        {
            var routes = from r in GetAll()
                         join depature in _terminalRepo.GetAll() on r.DepartureTerminalId equals depature.Id
                         join destination in _terminalRepo.GetAll() on r.DestinationTerminalId equals destination.Id
                         where string.IsNullOrWhiteSpace(query) || r.Name.Contains(query)
                         orderby r.Name

                         select new RouteDTO
                         {
                             Id = r.Id,
                             Name = r.Name,
                             RouteType = r.Type,
                             DepartureTerminalId = depature.Id,
                             DepartureTerminalName = depature.Name,
                             DestinationTerminalId = destination.Id,
                             DestinationTerminalName = destination.Name,
                             DriverFee = r.DriverFee,
                             DispatchFee = r.DispatchFee,
                             LoaderFee = r.LoaderFee,
                             AvailableOnline = r.AvailableOnline,
                             AvailableAtTerminal = r.AvailableAtTerminal,
                             ParentRouteName = r.ParentRoute
                         };

            return routes.ToPagedListAsync(pageNumber, pageSize);
        }

        public Task<List<AvailableTripDetailDTO>> GetRouteVirtualBuses(int routeId)
        {
            var departureDate = Clock.Now.Date;

            var excludedSeats = _excludedSeatRepo.GetAll().Select(s => s.SeatNumber);
            //var fareCalendar =
            //   _fareCalendarRepo.FirstOrDefault(c => departureDate >= c.StartDate.Date && c.EndDate.Date >= departureDate && routeId == c.RouteId);

            //var fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault();

            var routes =
                from route in GetAll()
                join trip in _tripRepo.GetAll() on route.Id equals trip.RouteId
                join vehicletripregistration in _vehicleTripRegRepo.GetAll() on trip.Id equals vehicletripregistration.TripId
                join vehicleModel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehicleModel.Id
                join fare in _fareRepo.GetAll() on new { RouteId = route.Id, VehicleModelId = vehicleModel.Id } equals new { fare.RouteId, fare.VehicleModelId }

                let bookedSeats = _seatMgtRepo.GetAll().Where(st => st.VehicleTripRegistrationId == vehicletripregistration.Id)
                let bookedSeat = _seatMgtRepo.GetAll()
                         .Where(s => s.VehicleTripRegistrationId == vehicletripregistration.Id)
                         .Select(s => (int) s.SeatNumber)

                let availableNumberOfSeats = vehicleModel.NumberOfSeats
                where route.Id == routeId && vehicletripregistration.DepartureDate == departureDate && vehicletripregistration.IsVirtualBus == true && vehicletripregistration.ManifestPrinted == false

                select new AvailableTripDetailDTO
                {
                    VehicleTripRegistrationId = vehicletripregistration.Id,
                    RouteName = route.Name,
                    VehicleName = vehicletripregistration.VehicleModel.VehicleMake.Name + " (" + vehicletripregistration.VehicleModel.Name + ")",
                    DriverCode = vehicletripregistration.DriverCode,
                    PhysicalBus = vehicletripregistration.PhysicalBusRegistrationNumber,
                    DepartureTime = trip.DepartureTime,
                    FarePrice = fare.Amount,
                    AvailableNumberOfSeats = availableNumberOfSeats - bookedSeats.Count(),
                    BookedSeats = bookedSeats.Select(s => (int) s.SeatNumber),
                    DepartureDate = vehicletripregistration.DepartureDate,
                    TotalNumberOfSeats = vehicletripregistration.VehicleModel.NumberOfSeats,
                    ExcludedSeats = excludedSeats.Union(bookedSeat)
                };

            return routes.AsNoTracking().ToListAsync();
        }
        public async Task<List<AvailableTripDetailDTO>> GetRouteVirtualBusesWithFareCalendar(int routeId)
        {
            var availabletrips = await GetRouteVirtualBuses(routeId);
            foreach (var trip in availabletrips) {
                var fareCalendar = new FareCalendarDTO();
                fareCalendar = null;
                var farecalendarList = new List<FareCalendarDTO>();

                farecalendarList = await _fareCalendarSvc.GetFareCalendaListByRoutesAsync(trip.RouteId, trip.DepartureDate);
                foreach (var calendar in farecalendarList) {
                    if (calendar.VehicleModelId != null) {
                        if (trip.VehicleModelId == calendar.VehicleModelId) {
                            fareCalendar = calendar;

                        }
                        break;
                    }

                    fareCalendar = calendar;
                }
                if (fareCalendar == null) {
                    var DepartureterminalId = await GetDepartureterminalIdFromRoute(trip.RouteId);
                    farecalendarList = await _fareCalendarSvc.GetFareCalendaListByTerminalsAsync(DepartureterminalId, trip.DepartureDate);
                    foreach (var calendar in farecalendarList) {
                        if (calendar.VehicleModelId != null) {
                            if (trip.VehicleModelId == calendar.VehicleModelId) {
                                fareCalendar = calendar;

                            }
                            break;
                        }

                        fareCalendar = calendar;
                    }
                }


                decimal fareDifference = 0;
                if (fareCalendar != null) {


                    if (fareCalendar.FareAdjustmentType == FareAdjustmentType.Percentage) {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.FarePrice / 100);

                    }
                    else {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                    }
                }


                trip.FarePrice += fareDifference;

            }
            return availabletrips;
        }
        public Task<List<RouteDTO>> GetTerminalRoutes(int terminalId)
        {
            var routes =
                from route in GetAll()
                where route.DepartureTerminalId == terminalId && route.AvailableAtTerminal

                select new RouteDTO
                {
                    Id = route.Id,
                    Name = route.Name,
                    RouteType = route.Type,
                    AvailableOnline = route.AvailableOnline,
                    AvailableAtTerminal = route.AvailableAtTerminal
                };

            return routes.AsNoTracking().ToListAsync();
        }

        public async Task UpdateRoute(int routeId, RouteDTO dto)
        {
            var route = await _repo.GetAsync(routeId);

            if (route is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            if (route.DepartureTerminalId != dto.DepartureTerminalId || route.DestinationTerminalId != dto.DestinationTerminalId) {
                if (await _repo.ExistAsync(v => v.DepartureTerminalId == dto.DepartureTerminalId && v.DestinationTerminalId == dto.DestinationTerminalId)) {
                    throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_EXIST);
                }
            }

            route.Name = await GetRouteNameAsync(dto.DepartureTerminalId, dto.DestinationTerminalId);
            route.Type = dto.RouteType;
            route.DepartureTerminalId = dto.DepartureTerminalId;
            route.DestinationTerminalId = dto.DestinationTerminalId;
            route.DriverFee = dto.DriverFee;
            route.DispatchFee = dto.DispatchFee;
            route.LoaderFee = dto.LoaderFee;
            route.ParentRouteId = dto.ParentRouteId;
            route.AvailableAtTerminal = dto.AvailableAtTerminal;
            route.AvailableOnline = dto.AvailableOnline;

            if (dto.ParentRouteId != null) {
                route.ParentRoute = await GetParentRouteNameAsync(dto.ParentRouteId);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<string> GetRouteNameAsync(int departureTerminalId, int destinationTerminalId)
        {
            var routes =
                from departure in _terminalRepo.GetAll()
                join destination in _terminalRepo.GetAll() on destinationTerminalId equals destination.Id
                join fromState in _stateRepo.GetAll() on departure.StateId equals fromState.Id
                join toState in _stateRepo.GetAll() on destination.StateId equals toState.Id
                where departure.Id == departureTerminalId
                select
                departure.Name + " ==> " + destination.Name;

            return routes.SingleOrDefaultAsync();
        }

        public Task<string> GetParentRouteNameAsync(int? ParentRoutId)
        {
            var routes =
                from route in GetAll()
                where route.Id == ParentRoutId

                select route.Name;

            return routes.SingleOrDefaultAsync();
        }
        public Task<string> GetDepartureCodeFromRoute(int? RouteId)
        {
            var depterminal =
                    from route in GetAll()
                    join terminal in _terminalRepo.GetAll()
                    on route.DepartureTerminalId equals terminal.Id
                    where route.Id == RouteId

                    select terminal.Code;

            return depterminal.SingleOrDefaultAsync() ?? Task.FromResult(string.Empty);
        }
        public Task<string> GetDestinationTerminalCodeFromRoute(int? RouteId)
        {
            var depterminal =
                    from route in GetAll()
                    join terminal in _terminalRepo.GetAll()
                    on route.DestinationTerminalId equals terminal.Id
                    where route.Id == RouteId

                    select terminal.Code;

            return depterminal.SingleOrDefaultAsync() ?? Task.FromResult(string.Empty);
        }
        public Task<string> GetDepartureterminalFromRoute(int? RouteId)
        {
            var depterminal =
                    from route in GetAll()
                    join terminal in _terminalRepo.GetAll()
                    on route.DepartureTerminalId equals terminal.Id
                    where route.Id == RouteId

                    select terminal.Name;

            return depterminal.SingleOrDefaultAsync() ?? Task.FromResult(string.Empty);
        }

        public Task<int> GetDepartureterminalIdFromRoute(int? RouteId)
        {
            var depterminal =
                    from route in GetAll()
                    join terminal in _terminalRepo.GetAll()
                    on route.DepartureTerminalId equals terminal.Id
                    where route.Id == RouteId

                    select terminal.Id;

            return depterminal.SingleOrDefaultAsync() ?? Task.FromResult(0);
        }
        public Task<string> GetDestinationterminalFromRoute(int? RouteId)
        {
            var desterminal =
                    from route in GetAll()
                    join terminal in _terminalRepo.GetAll()
                    on route.DestinationTerminalId equals terminal.Id
                    where route.Id == RouteId

                    select terminal.Name;

            return desterminal.SingleOrDefaultAsync() ?? Task.FromResult(string.Empty);
        }
        public async Task RemoveRoute(int routeId)
        {
            var route = await _repo.GetAsync(routeId);

            if (route == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            _repo.Delete(route);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<EmployeeRouteDTO> GetEmployeeRoutebyId(int employeerouteId)
        {
            var route = await _employeeRouteRepo.GetAsync(employeerouteId);

            if (route == null) {
                throw await _serviceHelper.GetExceptionAsync("Record for employee route does not exit");
            }

            return new EmployeeRouteDTO
            {
                TerminalId = route.TerminalId,
                EmployeeId = route.EmployeeId,
                Id = route.Id,
                DateCreated = route.CreationTime
            };
        }

        public async Task AddRoute(RouteDTO routeDto)
        {
            if (await _repo.ExistAsync(v => v.DepartureTerminalId == routeDto.DepartureTerminalId && v.DestinationTerminalId == routeDto.DestinationTerminalId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_EXIST);
            }

            routeDto.Name = await GetRouteNameAsync(routeDto.DepartureTerminalId, routeDto.DestinationTerminalId);

            if (routeDto.ParentRouteId != null) {
                routeDto.ParentRouteName = await GetParentRouteNameAsync(routeDto.ParentRouteId);
            }

            _repo.Insert(new Route
            {
                Name = routeDto.Name,
                ParentRoute = routeDto.ParentRouteName,
                ParentRouteId = routeDto.ParentRouteId,
                Type = routeDto.RouteType,
                DepartureTerminalId = routeDto.DepartureTerminalId,
                DestinationTerminalId = routeDto.DestinationTerminalId,
                DriverFee = routeDto.DriverFee,
                DispatchFee = routeDto.DispatchFee,
                LoaderFee = routeDto.LoaderFee,
                AvailableAtTerminal = routeDto.AvailableAtTerminal,
                AvailableOnline = routeDto.AvailableOnline
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public IQueryable<Route> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<Route> SingleOrDefaultAsync(Expression<Func<Route, bool>> predicate)
        {
            return GetAll().SingleOrDefaultAsync(predicate);
        }

        public async Task<List<RouteDTO>> GetLoginEmployeeRoutes()
        {
            var email = await _userSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            var employeeterminalId = await _employeeSvc.GetAssignedTerminal(email.Email);
            //Context.Employees.FirstOrDefault(x => x.Email == username);

            var employeeroutes =
                from route in GetAll()

                where route.DepartureTerminalId == employeeterminalId

                select new RouteDTO
                {
                    Name = route.Name,
                    Id = route.Id,
                    RouteType = route.Type,
                    AvailableOnline = route.AvailableOnline,
                    AvailableAtTerminal = route.AvailableAtTerminal

                };

            return employeeroutes.AsNoTracking().ToList();
        }


        public Task<List<RouteDTO>> GetStaffTerminalRoutes(string username)
        {
            var routes =
                from employee in _employeeRepo.GetAllIncluding(x => x.User)
                join route in _repo.GetAll() on employee.TerminalId equals route.DepartureTerminalId
                where employee.User.Email == username

                select new RouteDTO
                {
                    Id = route.Id,
                    Name = route.Name,
                    RouteType = route.Type,
                    AvailableOnline = route.AvailableOnline,
                    AvailableAtTerminal = route.AvailableAtTerminal
                };

            return routes.AsNoTracking().ToListAsync();
        }

        public Task<List<TerminalDTO>> GetRouteIdByDestinationAndDepartureId(int departureTerminalId, int destinationTerminalId)
        {
            var routes =
               from route in GetAll()
               where route.DepartureTerminalId == departureTerminalId && route.DestinationTerminalId == destinationTerminalId

               select new TerminalDTO
               {
                   Id = route.DepartureTerminalId,
                   Name = route.Name,
                   RouteId = route.Id
               };

            return routes.AsNoTracking().ToListAsync();
        }
    }
}