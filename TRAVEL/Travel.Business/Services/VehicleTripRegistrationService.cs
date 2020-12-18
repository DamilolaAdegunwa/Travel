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
    public interface IVehicleTripRegistrationService
    {
        Task<bool> ExistAsync(Expression<Func<VehicleTripRegistration, bool>> p);
        Task UpdateVehcileTrip(VehicleTripRegistrationDTO vehicleTripRegistration);

        Task UpdateVehicleTripId(Guid vehicleTripReg, Guid newTripId);
        void Add(VehicleTripRegistration entity);
        IQueryable<VehicleTripRegistration> GetAll();
        Task<VehicleTripRegistration> GetAsync(Guid id);
        Task<VehicleTripRegistrationDTO> GetVehicleTripRegistrationDTO(Guid id);
        VehicleTripRegistration FirstOrDefault(Expression<Func<VehicleTripRegistration, bool>> predicate);
        Task CreatePhysicalBus(TerminalBookingDTO physicalbus);
        Task<List<PassengerDto>> GetAllPassengersInTrip(Guid VehicleTripId);
        Task<List<PassengerDto>> GetAllPassengersInTrip(Guid VehicleTripId, int bookingtype);
        Task<List<VehicleTripRegistrationDTO>> GetVehicleTripByDriverCode(SalaryReportQuery dto);
        Task<List<VehicleTripRegistrationDTO>> GetvehicleTripByTerminalId(int terminalId);
        Task<List<VehicleTripRegistrationDTO>> GetvehicleTripByTerminalId();
        Task BlowVehicle(VehicleTripRegistrationDTO blowVehicleDetails);
        Task<IPagedList<VehicleTripRegistrationDTO>> GetBlownVehicleAsync(DateModel date, int pageNumber, int pageSize, string query);
        Task<IPagedList<FleetHistoryDTO>> SearchFleetHistory(SearchDTO date, int pageNumber, int pageSize, string query);
    }


    public class VehicleTripRegistrationService : IVehicleTripRegistrationService
    {
        private readonly IRepository<VehicleTripRegistration, Guid> _repo;
        private readonly IRepository<Driver> _driver;
        private readonly IRepository<Route> _routerepo;
        private readonly IUserService _userManagerSvc;
        private readonly ITripService _tripSvc;
        private readonly IVehicleService _vehicleSvc;
        private readonly IServiceHelper _serviceHelper;
        private readonly IEmployeeService _employeeSvc;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDriverService _driverSvc;
        private readonly ISeatManagementService _seatManagementService;
        private readonly IRepository<JourneyManagement, Guid> _journeyManagementRepo;
        private readonly IRepository<Vehicle> _vehRepo;
        private readonly IRepository<Manifest, Guid> _manRepo;

        public VehicleTripRegistrationService(IUserService userManagerSvc,
            IRepository<VehicleTripRegistration, Guid> repo,
            ITripService tripSvc,
            IVehicleService vehicleSvc,
            IServiceHelper serviceHelper,
            IEmployeeService employeeSvc, IRepository<Route> routerepo,
            IUnitOfWork unitOfWork,
            IDriverService driverSvc, ISeatManagementService seatManagementService,
            IRepository<JourneyManagement, Guid> journeyManagementRepo, IRepository<Vehicle> vehRepo,
            IRepository<Manifest, Guid> manRepo)
        {
            _userManagerSvc = userManagerSvc;
            _repo = repo;
            _tripSvc = tripSvc;
            _vehicleSvc = vehicleSvc;
            _serviceHelper = serviceHelper;
            _employeeSvc = employeeSvc;
            _driverSvc = driverSvc;
            _unitOfWork = unitOfWork;
            _seatManagementService = seatManagementService;
            _journeyManagementRepo = journeyManagementRepo;
            _routerepo = routerepo;
            _vehRepo = vehRepo;
            _manRepo = manRepo;
        }

        public void Add(VehicleTripRegistration entity)
        {
            _repo.Insert(entity);
        }

        public async Task CreatePhysicalBus(TerminalBookingDTO physicalbus)
        {
            var trip = await _tripSvc.GetTripById(physicalbus.TripId);
            var vehicle = await _vehicleSvc.GetVehicleById(physicalbus.VehicleId);

            DateTime departuredate = Clock.Now.Date;

            var asignedVehicleDto = await _vehicleSvc.GetVehiclesByRegNum(vehicle.RegistrationNumber);
            if (asignedVehicleDto == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_NOT_EXIST);

            }
            var assignedVehicle = await _vehicleSvc.GetVehicleById(physicalbus.VehicleId);
            var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
            var employee = await _employeeSvc.GetEmployeesByemailAsync(email.Email);
            if (employee != null && employee.TerminalId != null && assignedVehicle.VehicleStatus != VehicleStatus.InWorkshop) {
                assignedVehicle.LocationId = employee.TerminalId;
            }
            //assignedVehicle.Location = null;
            if (assignedVehicle != null && assignedVehicle.VehicleStatus != VehicleStatus.InWorkshop) {
                assignedVehicle.VehicleStatus = VehicleStatus.TerminalUse;

            }
            if (assignedVehicle != null && assignedVehicle.VehicleStatus == VehicleStatus.InWorkshop) {

                assignedVehicle.VehicleStatus = VehicleStatus.InWorkshopAndAssigned;
            }
            //Get assigned Captain and Update
            var assignedCaptainDto = await _driverSvc.GetActiveDriverByCodeAsync(physicalbus.DriverCode);
            if (assignedCaptainDto == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }

            var assignedDrv = await _driverSvc.GetAsync(assignedCaptainDto.Id);

            var phyBus = new VehicleTripRegistration
            {
                PhysicalBusRegistrationNumber = vehicle.RegistrationNumber,
                DepartureDate = departuredate,
                TripId = physicalbus.TripId,
                DriverCode = physicalbus.DriverCode,
                VehicleModelId = trip.VehicleModelId,
                IsVirtualBus = false,
                JourneyType = JourneyType.Loaded
            };


            if (assignedDrv.DriverType == DriverType.Virtual) {
                if (string.IsNullOrEmpty(physicalbus.OriginalDriverCode))
                    throw new LMEGenericException("Could not create vehicle because you did not choose the original captain that will take the trip");

                phyBus.OriginalDriverCode = physicalbus.OriginalDriverCode;
            }
            else {
                phyBus.OriginalDriverCode = "";
            }

            _repo.Insert(phyBus);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<VehicleTripRegistration, bool>> predicate)
        {
            return await _repo.ExistAsync(predicate);
        }

        public VehicleTripRegistration FirstOrDefault(Expression<Func<VehicleTripRegistration, bool>> predicate)
        {
            return _repo.FirstOrDefault(predicate);
        }

        public IQueryable<VehicleTripRegistration> GetAll()
        {
            return _repo.GetAllIncluding(x => x.VehicleModel, y => y.Trip);
        }

        public async Task<VehicleTripRegistration> GetAsync(Guid id)
        {
            return await Task.FromResult(_repo.GetAllIncluding(x => x.VehicleModel, y => y.Trip).FirstOrDefault(x => x.Id == id));
        }

        public Task<VehicleTripRegistrationDTO> GetVehicleTripRegistrationDTO(Guid id)
        {
            var query = from vehicleTripRegistration in GetAll()

                        where vehicleTripRegistration.Id == id
                        select new VehicleTripRegistrationDTO
                        {
                            Id = vehicleTripRegistration.Id,
                            PhysicalBusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                            DepartureDate = vehicleTripRegistration.DepartureDate,
                            IsVirtualBus = vehicleTripRegistration.IsVirtualBus,
                            IsBusFull = vehicleTripRegistration.IsBusFull,
                            RouteId = vehicleTripRegistration.Trip.RouteId,
                            IsBlownBus = vehicleTripRegistration.IsBlownBus,
                            DateCreated = vehicleTripRegistration.CreationTime,
                            DateModified = vehicleTripRegistration.LastModificationTime,
                            DriverCode = vehicleTripRegistration.DriverCode,
                            BookingTypeId = vehicleTripRegistration.BookingTypeId,
                            TripId = vehicleTripRegistration.TripId,
                            VehicleModelId = vehicleTripRegistration.VehicleModelId,
                            VehicleModel = vehicleTripRegistration.VehicleModel.Name,
                            JourneyType = vehicleTripRegistration.JourneyType
                        };

            return query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateVehcileTrip(VehicleTripRegistrationDTO vehicleTripRegistration)
        {
            //var vehicletrips = await _uow.VehicleTripRegistrations.GetAsyncByvId(vehicleTripRegistration.VehicleTripRegistrationId);
            var vehicletrips = await _repo.GetAsync(vehicleTripRegistration.VehicleTripRegistrationId.GetValueOrDefault());

            //_uow.VehicleTripRegistrations.Find(
            //        r => r.VehicleTripRegistrationId == vehicleTripRegistration.VehicleTripRegistrationId)
            //    .FirstOrDefault();

            if (vehicletrips == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }


            vehicletrips.PhysicalBusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber;
            vehicletrips.IsVirtualBus = false;
            vehicletrips.DriverCode = vehicleTripRegistration.DriverCode;
            var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            var employee = await _employeeSvc.GetEmployeesByemailAsync(email.Email);

            //Get Assigned Vehicle and Update
            var asignedVehicleDto = await _vehicleSvc.GetVehiclesByRegNum(vehicleTripRegistration.PhysicalBusRegistrationNumber);
            if (asignedVehicleDto == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_NOT_EXIST);

            }
            var assignedVehicle = await _vehicleSvc.GetVehicleById(asignedVehicleDto.Id);
            //assignedVehicle.Location = null;
            if (employee != null && employee.TerminalId != null && assignedVehicle.VehicleStatus != VehicleStatus.InWorkshop)
            {
                assignedVehicle.LocationId = employee.TerminalId;
            }
            if (assignedVehicle != null && assignedVehicle.VehicleStatus != VehicleStatus.InWorkshop)
            {


                assignedVehicle.VehicleStatus = VehicleStatus.TerminalUse;
            }
            if (assignedVehicle != null && assignedVehicle.VehicleStatus == VehicleStatus.InWorkshop)
            {


                assignedVehicle.VehicleStatus = VehicleStatus.InWorkshopAndAssigned;
            }
            //Get assigned Captain and Update



            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task<List<VehicleTripRegistrationDTO>> GetVehicleTripByDriverCode(SalaryReportQuery dto)
        {
            var query = from vehicleTripRegistration in GetAll()
                        join journeymanagement in _journeyManagementRepo.GetAll() on vehicleTripRegistration.Id equals journeymanagement.VehicleTripRegistrationId
                        join trip in _tripSvc.GetAll() on vehicleTripRegistration.TripId equals trip.Id
                        join route in _routerepo.GetAll() on trip.RouteId equals route.Id

                        where (vehicleTripRegistration.DriverCode == dto.DriverCode)
                        &&
                        (vehicleTripRegistration.DepartureDate >= dto.StartDate && vehicleTripRegistration.DepartureDate <= dto.EndDate)
                        // && (journeymanagement.JourneyStatus == JourneyStatus.Received)
                        select new VehicleTripRegistrationDTO
                        {
                            Id = vehicleTripRegistration.Id,
                            PhysicalBusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                            DepartureDate = vehicleTripRegistration.DepartureDate,
                            IsVirtualBus = vehicleTripRegistration.IsVirtualBus,
                            IsBusFull = vehicleTripRegistration.IsBusFull,
                            RouteId = vehicleTripRegistration.Trip.RouteId,
                            IsBlownBus = vehicleTripRegistration.IsBlownBus,
                            DateCreated = vehicleTripRegistration.CreationTime,
                            DateModified = vehicleTripRegistration.LastModificationTime,
                            DriverCode = vehicleTripRegistration.DriverCode,
                            BookingTypeId = vehicleTripRegistration.BookingTypeId,
                            TripId = vehicleTripRegistration.TripId,
                            VehicleModelId = vehicleTripRegistration.VehicleModelId,
                            VehicleModel = vehicleTripRegistration.VehicleModel.Name,
                            JourneyType = vehicleTripRegistration.JourneyType,
                            RouteName = route.Name,
                            DriverFee = route.DriverFee,
                            JourneyStatus = journeymanagement.JourneyStatus
                        };
            return await query.AsNoTracking().ToListAsync();
        }
        public Task<List<PassengerDto>> GetAllPassengersInTrip(Guid VehicleTripId)
        {
            var passengers = from pass in _seatManagementService.GetAll()
                             join route in _routerepo.GetAll()
                             on pass.RouteId equals route.Id
                             where pass.VehicleTripRegistrationId == VehicleTripId

                             select new PassengerDto
                             {
                                 Amount = pass.Amount,
                                 BookingReferenceCode = pass.BookingReferenceCode,
                                 FullName = pass.FullName,
                                 Gender = pass.Gender,
                                 PhoneNumber = pass.PhoneNumber,
                                 SeatNumber = pass.SeatNumber,
                                 Route = route.Name
                             };

            return Task.FromResult(passengers.ToList());
        }


        public Task<List<VehicleTripRegistrationDTO>> GetvehicleTripByTerminalId(int terminalId)
        {
            var departureDate = DateTime.Now.Date;
            var vehicleTripRegistrations =
                from vehicleTripRegistration in GetAll()
                where vehicleTripRegistration.Trip.Route.DepartureTerminalId == terminalId && vehicleTripRegistration.DepartureDate == departureDate && !vehicleTripRegistration.IsVirtualBus


                select new VehicleTripRegistrationDTO
                {
                    VehicleTripRegistrationId = vehicleTripRegistration.Id,
                    PhysicalBusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                    DepartureDate = vehicleTripRegistration.DepartureDate,
                    IsVirtualBus = vehicleTripRegistration.IsVirtualBus,
                    IsBusFull = vehicleTripRegistration.IsBusFull,
                    VehicleCaptaindetails = vehicleTripRegistration.PhysicalBusRegistrationNumber + " :: " + vehicleTripRegistration.DriverCode + " (" + vehicleTripRegistration.Trip.Route.Name + " - " + vehicleTripRegistration.Trip.DepartureTime + ")",
                    IsBlownBus = vehicleTripRegistration.IsBlownBus,
                    DriverCode = vehicleTripRegistration.DriverCode,
                    BookingTypeId = vehicleTripRegistration.BookingTypeId,
                    TripId = vehicleTripRegistration.TripId,
                    VehicleModelId = vehicleTripRegistration.VehicleModelId
                };


            return vehicleTripRegistrations.ToListAsync(); ;
        }


        public async Task<List<VehicleTripRegistrationDTO>> GetvehicleTripByTerminalId()
        {
            var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            var terminalid = await _employeeSvc.GetAssignedTerminal(email.Email);
            var tripRegistrations = new List<VehicleTripRegistrationDTO>();
            if (terminalid != null)
            {
                tripRegistrations = await GetvehicleTripByTerminalId(terminalid.GetValueOrDefault());

            }

            return tripRegistrations;
        }

        public async Task BlowVehicle(VehicleTripRegistrationDTO blowVehicleDetails)
        {
            var BlowBus = new VehicleTripRegistration
            {
                PhysicalBusRegistrationNumber = blowVehicleDetails.PhysicalBusRegistrationNumber,
                DepartureDate = DateTime.UtcNow.Date,
                DriverCode = blowVehicleDetails.DriverCode,
                IsBlownBus = true,
                IsVirtualBus = false,
                JourneyType = JourneyType.Blown,
                ManifestPrinted = true,
                TripId = blowVehicleDetails.TripId
            };
            _repo.Insert(BlowBus);

            //await _unitOfWork.SaveChangesAsync();

            //var vehicleTrip = await GetVehicleTripRegistrationDTO(blowVehicleDetails.TripId);
            //this is blow
            var journeyManagement = new JourneyManagement
            {
                VehicleTripRegistrationId = BlowBus.Id,
                JourneyStatus = JourneyStatus.InTransit,
                JourneyType = JourneyType.Blown,
                JourneyDate = BlowBus?.DepartureDate ?? Clock.Now
            };

            _journeyManagementRepo.Insert(journeyManagement);

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<IPagedList<VehicleTripRegistrationDTO>> GetBlownVehicleAsync(DateModel date, int pageNumber, int pageSize, string query)
        {
            var blowDeatils = from blowTrips in _repo.GetAll()
                              join trips in _tripSvc.GetAll() on blowTrips.TripId equals trips.Id
                              where blowTrips.DepartureDate >= date.StartDate && blowTrips.DepartureDate <= date.EndDate
                              && blowTrips.IsBlownBus == true
                              && blowTrips.JourneyType == JourneyType.Blown
                              orderby blowTrips.DepartureDate descending
            select new VehicleTripRegistrationDTO
            {
                Id = blowTrips.Id,
                DepartureDate = blowTrips.DepartureDate,
                DriverCode = blowTrips.DriverCode,
                PhysicalBusRegistrationNumber = blowTrips.PhysicalBusRegistrationNumber,
                JourneyType = blowTrips.JourneyType,
                RouteName = trips.Route.Name
            };


            return blowDeatils.AsNoTracking().ToPagedListAsync(pageNumber, pageSize); ;
        }

        public async Task UpdateVehicleTripId(Guid vehicleTripReg, Guid newTripId)
        {
            var vehicletrips = await _repo.GetAsync(vehicleTripReg);
            if (vehicletrips == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            //if (vehicletrips.TripId == vehicleTripReg)
            //{
            //    vehicletrips. = vehicletrips.TripId;
            //}
            vehicletrips.TripId = newTripId;

            _unitOfWork.SaveChanges();

        }
        public Task<List<PassengerDto>> GetAllPassengersInTrip(Guid VehicleTripId, int bookingtype)
        {
            var booking = new BookingTypes();
            if (bookingtype == 4)
                booking = BookingTypes.All;
            if (bookingtype == 2)
                booking = BookingTypes.Online;
            if (bookingtype == 1)
                booking = BookingTypes.Advanced;
            if (bookingtype == 0)
                booking = BookingTypes.Terminal;

            var passengers = from pass in _seatManagementService.GetAll()
                             join manifest in _manRepo.GetAll()
                             on pass.VehicleTripRegistrationId equals manifest.VehicleTripRegistrationId
                             join route in _routerepo.GetAll()
                             on pass.RouteId equals route.Id
                             where pass.VehicleTripRegistrationId == VehicleTripId
                             && (booking != BookingTypes.All ? pass.BookingType == booking : pass.VehicleTripRegistrationId == VehicleTripId)

                             select new PassengerDto
                             {
                                 Amount = pass.Amount,
                                 BookingReferenceCode = pass.BookingReferenceCode,
                                 FullName = pass.FullName,
                                 Gender = pass.Gender,
                                 PhoneNumber = pass.PhoneNumber,
                                 SeatNumber = pass.SeatNumber,
                                 Route = route.Name,
                                 BookingType = pass.BookingType,
                                 Dispatch = manifest.Dispatch
                             };

            return Task.FromResult(passengers.ToList());
        }

        public Task<IPagedList<FleetHistoryDTO>> SearchFleetHistory(SearchDTO date, int pageNumber, int pageSize, string query)
        {

            if (date.Driver == null && date.PhysicalBusRegistrationNumber == "Select a Vehicle")
            {
                var fleetHistory1 = from fleet in _repo.GetAll()

                                    join trips in _tripSvc.GetAll() on fleet.TripId equals trips.Id

                                    join route in _routerepo.GetAll() on trips.RouteId equals route.Id

                                    join journeyman in _journeyManagementRepo.GetAll() on fleet.Id equals journeyman.VehicleTripRegistrationId

                                    join vehicl in _vehRepo.GetAll() on fleet.PhysicalBusRegistrationNumber equals vehicl.RegistrationNumber

                                    join drives in _driverSvc.GetAll() on fleet.DriverCode equals drives.Code

                                    join manifes in _manRepo.GetAll() on fleet.Id equals manifes.VehicleTripRegistrationId

                                    where manifes.IsPrinted == true

                                    where (fleet.DepartureDate >= date.StartDate && fleet.DepartureDate <= date.EndDate)

                                    where string.IsNullOrWhiteSpace(query)
                                    || fleet.DriverCode.Contains(query, StringComparison.OrdinalIgnoreCase) || journeyman.JourneyType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || fleet.PhysicalBusRegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || route.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || drives.Name.Contains(query, StringComparison.OrdinalIgnoreCase)

                                    orderby fleet.DepartureDate descending

                                    select new FleetHistoryDTO
                                    {
                                        DepartureDate = fleet.DepartureDate,
                                        PhysicalBusRegistrationNumber = fleet.PhysicalBusRegistrationNumber,
                                        RouteName = route.Name,
                                        DriverCode = fleet.DriverCode,
                                        DriverName = drives.Name,
                                        VehicleTripRegistrationId = journeyman.VehicleTripRegistrationId,
                                        PurchaseDate = vehicl.PurchaseDate == null ? new DateTime() : vehicl.PurchaseDate,
                                        JourneyType = journeyman.JourneyType,
                                        JourneyTypes = journeyman.JourneyType.ToString(),
                                        JourneyStatus = journeyman.JourneyStatus,
                                        JourneyStatuses = journeyman.JourneyStatus.ToString(),
                                        IsPrinted = manifes.IsPrinted
                                    };
                return fleetHistory1.AsNoTracking().ToPagedListAsync(pageNumber, pageSize); ;
            }

            if (date.PhysicalBusRegistrationNumber == "Select a Vehicle")
            {
                var fleetHistory2 = from fleet in _repo.GetAll()

                                    join trips in _tripSvc.GetAll() on fleet.TripId equals trips.Id

                                    join route in _routerepo.GetAll() on trips.RouteId equals route.Id

                                    join journeyman in _journeyManagementRepo.GetAll() on fleet.Id equals journeyman.VehicleTripRegistrationId

                                    join vehicl in _vehRepo.GetAll() on fleet.PhysicalBusRegistrationNumber equals vehicl.RegistrationNumber

                                    join drives in _driverSvc.GetAll() on fleet.DriverCode equals drives.Code

                                    join manifes in _manRepo.GetAll() on fleet.Id equals manifes.VehicleTripRegistrationId

                                    where manifes.IsPrinted == true

                                    where (fleet.DepartureDate >= date.StartDate && fleet.DepartureDate <= date.EndDate)
                                    && (fleet.DriverCode == date.Driver)                                   

                                    where string.IsNullOrWhiteSpace(query)
                                    || fleet.DriverCode.Contains(query, StringComparison.OrdinalIgnoreCase) || journeyman.JourneyType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || fleet.PhysicalBusRegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || route.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || drives.Name.Contains(query, StringComparison.OrdinalIgnoreCase)

                                    orderby fleet.DepartureDate descending

                                    select new FleetHistoryDTO
                                    {
                                        DepartureDate = fleet.DepartureDate,
                                        PhysicalBusRegistrationNumber = fleet.PhysicalBusRegistrationNumber,
                                        RouteName = route.Name,
                                        DriverCode = fleet.DriverCode,
                                        DriverName = drives.Name,
                                        VehicleTripRegistrationId = journeyman.VehicleTripRegistrationId,
                                        PurchaseDate = vehicl.PurchaseDate == null ? new DateTime() : vehicl.PurchaseDate,
                                        JourneyType = journeyman.JourneyType,
                                        JourneyTypes = journeyman.JourneyType.ToString(),
                                        JourneyStatus = journeyman.JourneyStatus,
                                        JourneyStatuses = journeyman.JourneyStatus.ToString(),
                                        IsPrinted = manifes.IsPrinted
                                    };

                return fleetHistory2.AsNoTracking().ToPagedListAsync(pageNumber, pageSize); ;
            }
            if (date.Driver == null)
            {
                var fleetHistory3 = from fleet in _repo.GetAll()

                                    join trips in _tripSvc.GetAll() on fleet.TripId equals trips.Id

                                    join route in _routerepo.GetAll() on trips.RouteId equals route.Id

                                    join journeyman in _journeyManagementRepo.GetAll() on fleet.Id equals journeyman.VehicleTripRegistrationId

                                    join vehicl in _vehRepo.GetAll() on fleet.PhysicalBusRegistrationNumber equals vehicl.RegistrationNumber

                                    join drives in _driverSvc.GetAll() on fleet.DriverCode equals drives.Code

                                    join manifes in _manRepo.GetAll() on fleet.Id equals manifes.VehicleTripRegistrationId

                                    where manifes.IsPrinted == true

                                    where (fleet.DepartureDate >= date.StartDate && fleet.DepartureDate <= date.EndDate)
                                    && fleet.PhysicalBusRegistrationNumber == date.PhysicalBusRegistrationNumber


                                    where string.IsNullOrWhiteSpace(query)
                                    || fleet.DriverCode.Contains(query, StringComparison.OrdinalIgnoreCase) || journeyman.JourneyType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || fleet.PhysicalBusRegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase)
                                    || route.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || drives.Name.Contains(query, StringComparison.OrdinalIgnoreCase)

                                    orderby fleet.DepartureDate descending

                                    select new FleetHistoryDTO
                                    {
                                        DepartureDate = fleet.DepartureDate,
                                        PhysicalBusRegistrationNumber = fleet.PhysicalBusRegistrationNumber,
                                        RouteName = route.Name,
                                        DriverCode = fleet.DriverCode,
                                        DriverName = drives.Name,
                                        VehicleTripRegistrationId = journeyman.VehicleTripRegistrationId,
                                        PurchaseDate = vehicl.PurchaseDate == null ? new DateTime() : vehicl.PurchaseDate,
                                        JourneyType = journeyman.JourneyType,
                                        JourneyTypes = journeyman.JourneyType.ToString(),
                                        JourneyStatus = journeyman.JourneyStatus,
                                        JourneyStatuses = journeyman.JourneyStatus.ToString(),
                                        IsPrinted = manifes.IsPrinted
                                    };

                return fleetHistory3.AsNoTracking().ToPagedListAsync(pageNumber, pageSize); ;
            }
            else
            {
                var fleetHistory = from fleet in _repo.GetAll()

                                   join trips in _tripSvc.GetAll() on fleet.TripId equals trips.Id

                                   join route in _routerepo.GetAll() on trips.RouteId equals route.Id

                                   join journeyman in _journeyManagementRepo.GetAll() on fleet.Id equals journeyman.VehicleTripRegistrationId

                                   join vehicl in _vehRepo.GetAll() on fleet.PhysicalBusRegistrationNumber equals vehicl.RegistrationNumber

                                   join drives in _driverSvc.GetAll() on fleet.DriverCode equals drives.Code

                                   join manifes in _manRepo.GetAll() on fleet.Id equals manifes.VehicleTripRegistrationId

                                   where manifes.IsPrinted == true

                                   where (fleet.DepartureDate >= date.StartDate && fleet.DepartureDate <= date.EndDate)
                                   && fleet.PhysicalBusRegistrationNumber.Contains(date.PhysicalBusRegistrationNumber)
                                   && fleet.DriverCode == date.Driver

                                   where string.IsNullOrWhiteSpace(query)
                                   || fleet.DriverCode.Contains(query, StringComparison.OrdinalIgnoreCase) || journeyman.JourneyType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
                                   || fleet.PhysicalBusRegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase)
                                   || route.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || drives.Name.Contains(query, StringComparison.OrdinalIgnoreCase)

                                   orderby fleet.DepartureDate descending

                                   select new FleetHistoryDTO
                                   {
                                       DepartureDate = fleet.DepartureDate,
                                       PhysicalBusRegistrationNumber = fleet.PhysicalBusRegistrationNumber,
                                       RouteName = route.Name,
                                       DriverCode = fleet.DriverCode,
                                       DriverName = drives.Name,
                                       VehicleTripRegistrationId = journeyman.VehicleTripRegistrationId,
                                       PurchaseDate = vehicl.PurchaseDate == null ? new DateTime() : vehicl.PurchaseDate,
                                       JourneyType = journeyman.JourneyType,
                                       JourneyTypes = journeyman.JourneyType.ToString(),
                                       JourneyStatus = journeyman.JourneyStatus,
                                       JourneyStatuses = journeyman.JourneyStatus.ToString(),
                                       IsPrinted = manifes.IsPrinted
                                   };

                return fleetHistory.AsNoTracking().ToPagedListAsync(pageNumber, pageSize); ;
            }
        }
    }
}