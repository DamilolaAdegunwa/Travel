using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Business.Services
{
    public interface IManifestService
    {
        Task<ManifestDetailDTO> GetManifestById(Guid id);
        Task AddManifest(ManifestDTO manifest);
        Task<ManifestDetailDTO> GetManifestPassengersByVehicleTripIdAsync(Guid vehicleTripId);
        Task<ManifestDetailDTO> PrintManifestPassengersByVehicleTripIdAsync(Guid vehicleTripId);
        Task<ManifestDetailDTO> PrintManifestViewModelById(Guid vehicleTripRegistrationId);
        Task<ManifestDTO> GetManifestManagementsByVehicleTripIdAsync(Guid vehicleTripRegistrationId);
        Task<ManifestExt> GetManifestByVehicleTripIdAsync(Guid vehicleTripRegistrationId);
        Task UpdateDispatchManifestManagement(ManifestExt manifestManagement);
        Task UpdateBusType(VehicleTripRegistrationDTO manifestManagement);
        Task<VehicleTripRegistrationDTO> GetVehicleTripRegistrationDTO(Guid id);
        Task<decimal?> GetMainTripFare(Guid vehicleTripRegistrationId);
        Task<decimal?> GetTripFare(int subrouteId, Guid vehicleTripRegistrationId);
        Task<decimal?> GetPassengerFare(string passengerInfo);
        Task UpdateRouteFare(Guid vehicleTripReg, decimal Amount);
        Task UpdateOpenManifest(Guid vehicleRegistrationid);

    }

    public class ManifestService : IManifestService
    {
        private readonly IRepository<Manifest, Guid> _repository;
        private readonly IRepository<SubRoute> _subRouteRepo;
        private readonly IRepository<VehicleTripRegistration, Guid> _repo;
        private readonly IRepository<Driver> _driverRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<SeatManagement, long> _seatRepo;
        private readonly IRepository<PickupPoint> _pickupRepo;
        private readonly ISeatManagementService _seatManagemengtSvc;
        private readonly IFareService _fareSvc;
        private readonly IJourneyManagementService _journeyMgtSvc;
        private readonly IEmployeeService _employeeRepo;
        private readonly IVehicleTripRegistrationService _vehicleTripRepo;

        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRouteService _routeSvc;
        private readonly BookingConfig bookingConfig;
        private readonly IPassportTypeService _passportTypeSvc;
        


        public ManifestService(IOptions<BookingConfig> _bookingConfig, IRepository<Manifest, Guid> repository,
            IRepository<SeatManagement, long> seatRepo,
            IRepository<VehicleTripRegistration, Guid> repo,

            IRepository<SubRoute> subRouteRepo,
            IRepository<PickupPoint> pickupRepo,
            IServiceHelper serviceHelper,
            ISeatManagementService seatManagemengtRepo,
            IVehicleTripRegistrationService vehicleTripRepo,
            IRepository<Driver> driverRepo,
            IEmployeeService employeeRepo,
            IRepository<Terminal> terminalRepo,
            IUnitOfWork unitOfWork,
            IFareService fareSvc,
            IRouteService routeSvc,
            IJourneyManagementService journeyMgtSvc,
            IPassportTypeService passportTypeRepo)
        {
            bookingConfig = _bookingConfig.Value;
            _repository = repository;
            _repo = repo;
            _subRouteRepo = subRouteRepo;
            _routeSvc = routeSvc;
            _seatRepo = seatRepo;
            _pickupRepo = pickupRepo;
            _serviceHelper = serviceHelper;
            _seatManagemengtSvc = seatManagemengtRepo;
            _vehicleTripRepo = vehicleTripRepo;
            _driverRepo = driverRepo;
            _employeeRepo = employeeRepo;
            _unitOfWork = unitOfWork;
            _fareSvc = fareSvc;
            _journeyMgtSvc = journeyMgtSvc;
            _terminalRepo = terminalRepo;
            _passportTypeSvc = passportTypeRepo;
        }

        public Task<ManifestDTO> GetManifestManagementsByVehicleTripIdAsync(Guid vehicleTripRegistrationId)
        {
            var manifestManagements =
                from manifestManagement in _repository.GetAll()
                where manifestManagement.VehicleTripRegistrationId == vehicleTripRegistrationId
                select new ManifestDTO
                {
                    Id = manifestManagement.Id,
                    NumberOfSeats = manifestManagement.NumberOfSeats,
                    ManifestPrintedTime = manifestManagement.ManifestPrintedTime,
                    IsPrinted = manifestManagement.IsPrinted,

                    VehicleTripRegistrationId = manifestManagement.VehicleTripRegistrationId,
                    Employee = manifestManagement.Employee,
                    RouteId = manifestManagement.VehicleTripRegistration.Trip.RouteId
                };

            return manifestManagements.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task UpdateOpenManifest(Guid vehicleRegistrationId)
        {
            var manifest = await GetManifestPassengersByVehicleTripIdAsync(vehicleRegistrationId);
            var existingManifestManagement = await _repository.GetAsync(manifest.Id);

            if (existingManifestManagement == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }


            existingManifestManagement.IsPrinted = false;


            await _unitOfWork.SaveChangesAsync();
        }
        public Task<ManifestExt> GetManifestByVehicleTripIdAsync(Guid vehicleTripRegistrationId)
        {
            var manifestManagements =
                from manifestManagement in _repository.GetAll()
                join vehicleTripRegistration in _vehicleTripRepo.GetAll() on manifestManagement.VehicleTripRegistrationId equals vehicleTripRegistration.Id

                where manifestManagement.VehicleTripRegistrationId == vehicleTripRegistrationId
                select new ManifestExt
                {
                    ManifestManagementId = manifestManagement.Id,
                    NumberOfSeats = manifestManagement.NumberOfSeats,
                    ManifestPrintedTime = manifestManagement.ManifestPrintedTime,
                    IsPrinted = manifestManagement.IsPrinted,
                    BusRegNum = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                    VehicleTripRegistrationId = manifestManagement.VehicleTripRegistrationId,
                    Employee = manifestManagement.Employee,
                    RouteId = manifestManagement.VehicleTripRegistration.Trip.RouteId
                };

            return manifestManagements.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<decimal?> GetTripFare(int subrouteId, Guid vehicleTripRegistrationId)
        {
            var vehicleTrip = await GetVehicleTripRegistrationDTO(vehicleTripRegistrationId);

            var subRoute = await _subRouteRepo.GetAsync(subrouteId);

            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare = new FareDTO();
            if(vehicleTrip.CurrentModelId != null)
            {
                tripFare = await _fareSvc.GetFareByVehicleTrip(
                                   subRoute.NameId, vehicleTrip.CurrentModelId.GetValueOrDefault());

            }
            else
            {
                tripFare = await _fareSvc.GetFareByVehicleTrip(
                                                  subRoute.NameId, vehicleTrip.VehicleModelId.GetValueOrDefault());

            }

            return tripFare?.Amount;
        }

        public async Task<decimal?> GetPassengerFare(string passengerInfo)
        {

            PassengerType passengerType = PassengerType.Adult;
            decimal childDisc = 0;
            int? subrouteId = null;
            Guid vehicleTripId = new Guid();
            if (passengerInfo != null)
            {
                var passInfo = passengerInfo.Split(',');
                passengerType = (PassengerType)Convert.ToInt32(passInfo[0]);
                subrouteId = passInfo[1] != null && passInfo[1] != "Select Subroute" && passInfo[1] != "" ? Convert.ToInt32(passInfo[1]) : 0;
                vehicleTripId = Guid.Parse(passInfo[2]);

            }

            var vehicleTrip = await GetVehicleTripRegistrationDTO(vehicleTripId);
            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }
            if (subrouteId != 0)
            {
                var subRoute = await _subRouteRepo.GetAsync(subrouteId.GetValueOrDefault());
                var tripFare = new FareDTO();

                if (vehicleTrip.CurrentModelId != null)
                {
                  tripFare =await _fareSvc.GetFareByVehicleTrip(
                  subRoute.NameId, vehicleTrip.CurrentModelId.GetValueOrDefault());
                }
                else
                {
                    tripFare = await _fareSvc.GetFareByVehicleTrip(
                   subRoute.NameId, vehicleTrip.VehicleModelId.GetValueOrDefault());
                }
                

                childDisc = (decimal)tripFare.ChildrenDiscountPercentage.GetValueOrDefault() / 100;
                return passengerType == PassengerType.Adult ? tripFare.Amount : tripFare.Amount - tripFare.Amount * childDisc;
            }

            var fare = new FareDTO();

            if (vehicleTrip.CurrentModelId != null)
            {
                fare = await _fareSvc.GetFareByVehicleTrip(
                             vehicleTrip.RouteId.GetValueOrDefault(), vehicleTrip.CurrentModelId.GetValueOrDefault());

            }
            else
            {
                fare = await _fareSvc.GetFareByVehicleTrip(
                             vehicleTrip.RouteId.GetValueOrDefault(), vehicleTrip.VehicleModelId.GetValueOrDefault());

            }

            childDisc = (decimal)fare.ChildrenDiscountPercentage.GetValueOrDefault() / 100;
            return passengerType == PassengerType.Adult ? fare.Amount : fare.Amount - fare.Amount * childDisc;
        }


        public async Task<decimal?> GetMainTripFare(Guid vehicleTripRegistrationId)
        {
            var vehicleTrip = await GetVehicleTripRegistrationDTO(vehicleTripRegistrationId);

            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare = new FareDTO();
            if (vehicleTrip.CurrentModelId != null)
            {
                tripFare = await _fareSvc.GetFareByVehicleTrip(
                                     vehicleTrip.RouteId.GetValueOrDefault(), vehicleTrip.CurrentModelId.GetValueOrDefault());

            }
            else
            {
                tripFare = await _fareSvc.GetFareByVehicleTrip(
                                                    vehicleTrip.RouteId.GetValueOrDefault(), vehicleTrip.VehicleModelId.GetValueOrDefault());

            }
        
            return tripFare?.Amount;
        }

        public async Task UpdateDispatchManifestManagement(ManifestExt manifestManagement)
        {

            var manifest = await GetManifestPassengersByVehicleTripIdAsync(manifestManagement.VehicleTripRegistrationId);
            var existingManifestManagement = await _repository.GetAsync(manifest.Id);

            if (existingManifestManagement == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }


            existingManifestManagement.Dispatch = manifestManagement.Dispatch;
            existingManifestManagement.DispatchSource = manifestManagement.BusRegNum;


            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateBusType(VehicleTripRegistrationDTO manifestManagement)
        {
            decimal amount_to_add = 0;
            int originalModelId = manifestManagement.OriginalModelId.GetValueOrDefault();
            int newModelId = manifestManagement.VehicleModelId.GetValueOrDefault();

            amount_to_add = await GetFareDifference(manifestManagement.VehicleTripRegistrationId.GetValueOrDefault(), originalModelId, newModelId);

            var manifest = await GetManifestPassengersByVehicleTripIdAsync(manifestManagement.VehicleTripRegistrationId.GetValueOrDefault());
            var existingManifestManagement = await _repository.GetAsync(manifest.Id);

            if (existingManifestManagement == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }


            existingManifestManagement.Amount = existingManifestManagement.Amount + amount_to_add;
            existingManifestManagement.VehicleModelId = newModelId;
            foreach (var seat in manifest.Passengers)
            {
                if (seat.BookingType == BookingTypes.Terminal)
                {
                    var seatMgt = await _seatRepo.GetAsync(seat.Id);
                    seatMgt.Amount = seatMgt.Amount + amount_to_add;

                }
            }

            var existingVehicletrip = await _repo.GetAsync(manifestManagement.VehicleTripRegistrationId.GetValueOrDefault());

            if (existingVehicletrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }
            existingVehicletrip.VehicleModelId = newModelId;
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<VehicleTripRegistrationDTO> GetVehicleTripRegistrationDTO(Guid id)
        {
            var query = from vehicleTripRegistration in _repo.GetAllIncluding(x=>x.Trip)
                        join manifest in _repository.GetAll() on vehicleTripRegistration.Id equals manifest.VehicleTripRegistrationId
              into manifests

                        from manifest in manifests.DefaultIfEmpty()

                        where vehicleTripRegistration.Id == id

                        select new VehicleTripRegistrationDTO
                        {
                            
                            Id = vehicleTripRegistration.Id,
                            CurrentModelId=manifest.VehicleModelId,
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


        public async Task<decimal> GetFareDifference(Guid vehicleTripRegistrationId,int originalmodelId,int newModelId)
        {
            var vehicleTrip = await _vehicleTripRepo.GetAsync(vehicleTripRegistrationId);
            decimal faredifference = 0;
            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare =
                await _fareSvc.GetFareByVehicleTrip(
                    vehicleTrip.Trip.RouteId, originalmodelId);

            var newfare = await _fareSvc.GetFareByVehicleTrip(
                    vehicleTrip.Trip.RouteId, newModelId);

            if(tripFare != null && newfare!= null)
            {
                faredifference = newfare.Amount - tripFare.Amount;
            }
            return faredifference;
        }

        public async Task AddManifest(ManifestDTO manifest)
        {
            var existingManifestManagement = await GetManifestManagementsByVehicleTripIdAsync(manifest.VehicleTripRegistrationId);

            var vehicleTripReg = await _vehicleTripRepo.GetAsync(manifest.VehicleTripRegistrationId);

            if (existingManifestManagement != null)
            {
                existingManifestManagement.IsPrinted = manifest.IsPrinted;
                existingManifestManagement.ManifestPrinted = manifest.ManifestPrinted;
                await UpdateManifestManagement(existingManifestManagement.Id, existingManifestManagement);
            }
            else
            {
                var listOfSeat = await _seatManagemengtSvc.GetByVehicleTripIdAsync(
               manifest.VehicleTripRegistrationId);

                var fare =
                    await GetTripFare(manifest.VehicleTripRegistrationId);
                decimal? amount = fare?.Amount;

                _repository.Insert(new Manifest
                {
                    NumberOfSeats = listOfSeat.Count,
                    Employee = _serviceHelper.GetCurrentUserEmail(),
                    VehicleTripRegistrationId = manifest.VehicleTripRegistrationId,
                    Amount = amount,
                    VehicleModelId = vehicleTripReg.VehicleModelId
                });



                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateManifestManagement(Guid manifestManagementId, ManifestDTO manifestManagement)
        {
            var existingManifestManagement = await _repository.GetAsync(manifestManagementId);

            if (existingManifestManagement == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }
            var listOfSeat =
                await _seatManagemengtSvc.GetByVehicleTripIdAsync(
                    existingManifestManagement.VehicleTripRegistrationId);

            existingManifestManagement.NumberOfSeats = listOfSeat.Count;
            
            //existingManifestManagement.IsPrinted = manifestManagement.IsPrinted;


            var tripRegistration = await _vehicleTripRepo.GetAsync(existingManifestManagement.VehicleTripRegistrationId);

            if (manifestManagement.IsPrinted)
            {
                await _journeyMgtSvc.AddJourneyManagementFromManifest(existingManifestManagement.VehicleTripRegistrationId, JourneyType.Loaded);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<FareDTO> GetTripFare(Guid vehicleTripRegistrationId)
        {
            var vehicleTrip = await _vehicleTripRepo.GetVehicleTripRegistrationDTO(vehicleTripRegistrationId);

            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare =
                await _fareSvc.GetFareByVehicleTrip(
                    vehicleTrip.RouteId.GetValueOrDefault(), vehicleTrip.VehicleModelId.GetValueOrDefault());

            // start fare calendar
           
            //---end farecalendar


            return tripFare;

        }
        //PrintManifestViewModelById
        public async Task<ManifestDetailDTO> GetManifestById(Guid id)
        {
            var manifestManagement = await GetManifestPassengersByVehicleTripIdAsync(id);

            if (manifestManagement is null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }
            foreach (var passenger in manifestManagement.Passengers)
            {
                var seat = await _seatManagemengtSvc.GetAsync(passenger.Id);
                if (seat.IsSub || seat.IsSubReturn)
                {

                    passenger.SubRouteName = seat.OnlineSubRouteName;
                }

                var departureterminal = seat.Route.DepartureTerminalId;

                var createdBy = passenger.CreatedBy;

                if (passenger.BookingType == BookingTypes.Advanced)
                {

                    var userTerminal = await _employeeRepo.GetAssignedTerminal(createdBy);
                    if (userTerminal != null)
                    {
                        var createdbyTerminal = userTerminal.GetValueOrDefault();
                        string createdTerminalName = "";
                        var terminal = await _terminalRepo.GetAsync(createdbyTerminal);
                        if (terminal != null)
                        {
                            createdTerminalName = terminal.Name;
                        }
                        if (departureterminal != createdbyTerminal)
                        {
                            passenger.IsCrossSell = true;
                            passenger.CreatedByTerminal = createdTerminalName;
                        }
                    }
                    if (userTerminal == null)
                    {

                        passenger.IsCrossSell = true;
                        passenger.CreatedByTerminal = "Not Available";
                    }
                }
            }

            return manifestManagement;
        }

        public Task<ManifestDetailDTO> GetManifestPassengersByVehicleTripIdAsync(Guid vehicleTripId)
        {
            var seatBooking = new List<Tuple<int, int, bool>>();

            var vehiclemanifest = _repository.GetAllIncluding(x=>x.VehicleModel)
                .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.VehicleModelId != null);

            var vehicletrip = _vehicleTripRepo.GetAll().Where(s => s.Id == vehicleTripId);

            var terminalCashBookings = _seatManagemengtSvc.GetAll()
                    .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.Cash);

            var partCashBookings = _seatManagemengtSvc.GetAll()
                 .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.CashAndPos);


            var rescheduleFee = _seatManagemengtSvc.GetAll()
      .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.IsRescheduled && s.RescheduleStatus == RescheduleStatus.PayAtTerminal);

            var rerouteFee = _seatManagemengtSvc.GetAll()
      .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.IsRerouted && s.RerouteStatus == RerouteStatus.PayAtTerminal);

            //RerouteFee = rerouteFee.Sum(i => i.RerouteFeeDiff),

            var allBookings = _seatManagemengtSvc.GetAll()
                .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingStatus == BookingStatus.Approved);
            List<int> bookingTypes = new List<int>();
            List<int> seatsBooking = new List<int>();
            List<int> seatClashes = new List<int>();
            List<bool> ticketPrintStatus = new List<bool>();
            List<int> remainingSeat = new List<int>();
            Dictionary<int, int> bookedSeat = new Dictionary<int, int>();
            Dictionary<int, int> clashedSeat = new Dictionary<int, int>();

            foreach (var booking in allBookings)
            {
                bookingTypes.Add((int)booking.BookingType);
                ticketPrintStatus.Add(booking.IsPrinted);
                seatsBooking.Add(booking.SeatNumber);
                if (!bookedSeat.ContainsValue(booking.SeatNumber))
                    bookedSeat.Add(booking.SeatNumber, booking.SeatNumber);
                else
                {
                    clashedSeat.Add(booking.SeatNumber, booking.SeatNumber);
                    seatClashes.Add(booking.SeatNumber);
                }
            }


            string modelname = string.Empty;
            if (vehiclemanifest.FirstOrDefault() != null)
            {
                modelname = vehiclemanifest.FirstOrDefault().VehicleModel.Name;
                for (int j = 1; j <= vehiclemanifest.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                {
                    if (!bookedSeat.ContainsKey(j) && (modelname == "Hiace Super Ex" || modelname == "Hiace Ex" ) && j == 2)
                    {
                        continue;
                    }
                    else if (!bookedSeat.ContainsKey(j) && (j == 5 || j == 14) && modelname == "Hiace Super Ex" )
                    {
                        continue;
                    }
                    else if  (!bookedSeat.ContainsKey(j))
                    {
                        remainingSeat.Add(j);
                    }
                }
            }
            else
            {
                if (vehicletrip.FirstOrDefault() != null)
                {
                    modelname = vehicletrip.FirstOrDefault().VehicleModel.Name;
                    for (int j = 1; j <= vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                    {
                        if (!bookedSeat.ContainsKey(j) && (modelname == "Hiace Super Ex" || modelname == "Hiace Ex") && j == 2)
                        {
                            continue;
                        }
                        else if (!bookedSeat.ContainsKey(j) && (j == 5 || j == 14) && modelname == "Hiace Super Ex")
                        {
                            continue;
                        }
                        else if (!bookedSeat.ContainsKey(j))
                        {
                            remainingSeat.Add(j);
                        }
                    }
                }
            }

            var manifestManagements =
                from manifestManagement in _repository.GetAll()
                join vehicleTripRegistration in _vehicleTripRepo.GetAll() on manifestManagement.VehicleTripRegistrationId equals vehicleTripRegistration.Id

                let selectedSeats = _seatManagemengtSvc.GetAll()
                           .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingStatus == BookingStatus.Approved)

                let captain = _driverRepo.GetAll().Where(s => s.Code == vehicleTripRegistration.DriverCode)
                let originalcaptain = _driverRepo.GetAll().Where(s => s.Code == vehicleTripRegistration.OriginalDriverCode)

                where manifestManagement.VehicleTripRegistrationId == vehicleTripId

                select new ManifestDetailDTO
                {
                    Id = manifestManagement.Id,
                    NumberOfSeats = manifestManagement.NumberOfSeats,
                    IsPrinted = manifestManagement.IsPrinted,
                    DispatchSource = manifestManagement.DispatchSource,
                    PrintedBy = manifestManagement.Employee,
                    TotalSeats = vehiclemanifest.FirstOrDefault() != null ? vehiclemanifest.FirstOrDefault().VehicleModel.NumberOfSeats : vehicleTripRegistration.VehicleModel.NumberOfSeats,
                    DepartureDate = vehicleTripRegistration.DepartureDate,
                    DepartureTime = vehicleTripRegistration.Trip.DepartureTime,
                    VehicleTripRegistrationId = vehicleTripId,
                    Route = vehicleTripRegistration.Trip.Route.Name,
                    TotalSold = terminalCashBookings.Sum(i => i.Amount - i.Discount) + partCashBookings.Sum(i => i.PartCash),
                    RescheduleFee = rescheduleFee.Count() * 500,
                    RerouteFee = rerouteFee.Where(i => i.RerouteFeeDiff >= 0).Sum(i => i.RerouteFeeDiff) + rerouteFee.Count() * 500,
                    Dispatch = manifestManagement.Dispatch,

                    VehicleModel = manifestManagement.VehicleModel.Name ?? vehicleTripRegistration.VehicleModel.Name,

                    DriverCode = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? vehicleTripRegistration.OriginalDriverCode : vehicleTripRegistration.DriverCode,
                    DriverName = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? originalcaptain.FirstOrDefault().Name : captain.FirstOrDefault().Name,
                    DriverPhone = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? originalcaptain.FirstOrDefault().Phone1 : captain.FirstOrDefault().Phone1,
                    RemainingSeat = remainingSeat,
                    BookSeat = seatsBooking,
                    ClashingSeats = seatClashes,
                    TicketPrintStatus = ticketPrintStatus,
                    BookingTypes = bookingTypes,

                    RemainingSeatCount = vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats - selectedSeats.Count(),
                    Amount = manifestManagement.Amount,
                    BusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                    Passengers = selectedSeats.Select(s => new SeatManagementDTO
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        BookingReferenceCode = s.BookingReferenceCode,
                        NextOfKinName = s.NextOfKinName,
                        NextOfKinPhoneNumber = s.NextOfKinPhoneNumber,
                        IsPrinted = s.IsPrinted,
                        PhoneNumber = s.PhoneNumber,
                        FullName = s.FullName,
                        PassengerType = s.PassengerType,
                        Gender = s.Gender,
                        VehicleTripRegistrationId = s.VehicleTripRegistrationId,
                        Amount = s.Amount ?? manifestManagement.Amount,
                        PartCash = s.PartCash,
                        POSReference = s.POSReference,
                        Discount = s.Discount,
                        RouteId = s.RouteId ?? vehicleTripRegistration.Trip.RouteId,
                        RouteName = vehicleTripRegistration.Trip.Route.Name,
                        PaymentMethod = s.PaymentMethod,
                        BookingType = s.BookingType,
                        SubRouteId = s.SubRouteId,
                        SubRouteName = s.SubRoute.Name ?? s.Route.Name,
                        DateCreated = s.CreationTime,
                        DateModified = s.LastModificationTime,
                        TravelStatus = s.TravelStatus,
                        RescheduleStatus = s.RescheduleStatus,
                        IsRescheduled = s.IsRescheduled,
                        RerouteStatus = s.RerouteStatus,
                        IsRerouted = s.IsRerouted,
                        IsUpgradeDowngrade = s.IsUpgradeDowngrade,
                        UpgradeDowngradeDiff = s.UpgradeDowngradeDiff,
                        UpgradeType = s.UpgradeType,
                        RerouteFeeDiff = s.RerouteFeeDiff,
                        CreatedBy = s.CreatedBy,
                        PickUpPointId = s.PickUpPointId,
                        PickupStatus = s.PickupStatus,
                        LastModificationTime = s.LastModificationTime,
                        IsGhanaRoute = s.IsGhanaRoute,
                        PassportType = s.PassportType,
                        PassportId = s.PassportId,
                        PlaceOfIssue = s.PlaceOfIssue,
                        IssuedDate = s.IssuedDate,
                        ExpiredDate = s.ExpiredDate,
                        Nationality = s.Nationality
                    }).OrderByDescending(e => e.LastModificationTime)
                };

            return manifestManagements.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<ManifestDetailDTO> PrintManifestViewModelById(Guid vehicleTripRegistrationId)
        {
            var manifestPrintedTime = DateTime.Now;
            var manifestManagement = await PrintManifestPassengersByVehicleTripIdAsync(vehicleTripRegistrationId);

            if (manifestManagement == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.MANIFEST_MANAGEMENT_NOT_EXIST);
            }
            //var tripRegistration = await _uow.VehicleTripRegistrations.GetAsync(x => x.VehicleTripRegistrationId == vehicleTripRegistrationId);

            //tripRegistration.ManifestPrinted = true;
            var manifestToUpdate = await _repository.GetAsync(manifestManagement.Id);

            manifestToUpdate.IsPrinted = true;
            manifestToUpdate.Employee = _serviceHelper.GetCurrentUserEmail();
            manifestToUpdate.ManifestPrintedTime = manifestPrintedTime;

 
            foreach (var passenger in manifestManagement.Passengers)
            {
                var seat = await _seatManagemengtSvc.GetAsync(passenger.Id);
                seat.TravelStatus = TravelStatus.Travelled;
            }


            await _unitOfWork.SaveChangesAsync();

            foreach (var passenger in manifestManagement.Passengers)
            {
                var seat = await _seatManagemengtSvc.GetAsync(passenger.Id);
                var departureterminal = seat.Route.DepartureTerminalId;

                var createdBy = passenger.CreatedBy;
                if (passenger.PickupStatus == PickupStatus.PendingPickup && passenger.PickUpPointId != null)
                {
                    var pickup = await _pickupRepo.GetAsync(passenger.PickUpPointId.GetValueOrDefault());
                    if (pickup != null)
                    {
                        passenger.PickupPointName = pickup.Name;
                    }
                }

                if (passenger.BookingType == BookingTypes.Advanced)
                {
                    var userTerminal = await _employeeRepo.GetAssignedTerminal(createdBy);
                    if (userTerminal != null)
                    {
                        var createdbyTerminal = userTerminal.GetValueOrDefault();
                        string createdTerminalName = "";
                        var terminal = await _terminalRepo.GetAsync(createdbyTerminal);
                        if (terminal != null)
                        {
                            createdTerminalName = terminal.Name;
                        }
                        if (departureterminal != createdbyTerminal)
                        {
                            passenger.IsCrossSell = true;
                            passenger.CreatedByTerminal = createdTerminalName;
                        }
                    }
                    if (userTerminal == null)
                    {
                        passenger.IsCrossSell = true;
                        passenger.CreatedByTerminal = "Not Available";
                    }
                }

            }
            return manifestManagement;
        }

        public Task<ManifestDetailDTO> PrintManifestPassengersByVehicleTripIdAsync(Guid vehicleTripId)
        {
            var seatBooking = new List<Tuple<int, int, bool>>();

            var vehiclemanifest = _repository.GetAllIncluding(x => x.VehicleModel)
                .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.VehicleModelId != null);

            var vehicletrip = _vehicleTripRepo.GetAll().Where(s => s.Id == vehicleTripId);

            var terminalCashBookings = _seatManagemengtSvc.GetAll()
                    .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.Cash);

            var partCashBookings = _seatManagemengtSvc.GetAll()
                 .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.CashAndPos);


            var rescheduleFee = _seatManagemengtSvc.GetAll()
      .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.IsRescheduled && s.RescheduleStatus == RescheduleStatus.PayAtTerminal);

            var rerouteFee = _seatManagemengtSvc.GetAll()
      .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.IsRerouted && s.RerouteStatus == RerouteStatus.PayAtTerminal);

            //RerouteFee = rerouteFee.Sum(i => i.RerouteFeeDiff),
           

            var allBookings = _seatManagemengtSvc.GetAll()
                .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingStatus == BookingStatus.Approved);
            List<int> bookingTypes = new List<int>();
            List<int> seatsBooking = new List<int>();
            List<int> seatClashes = new List<int>();
            List<bool> ticketPrintStatus = new List<bool>();
            List<int> remainingSeat = new List<int>();
            Dictionary<int, int> bookedSeat = new Dictionary<int, int>();
            Dictionary<int, int> clashedSeat = new Dictionary<int, int>();

            foreach (var booking in allBookings)
            {
                bookingTypes.Add((int)booking.BookingType);
                ticketPrintStatus.Add(booking.IsPrinted);
                seatsBooking.Add(booking.SeatNumber);
                if (!bookedSeat.ContainsValue(booking.SeatNumber))
                    bookedSeat.Add(booking.SeatNumber, booking.SeatNumber);
                else
                {
                    clashedSeat.Add(booking.SeatNumber, booking.SeatNumber);
                    seatClashes.Add(booking.SeatNumber);
                }
            }


            string modelname = string.Empty;
            if (vehiclemanifest.FirstOrDefault() != null)
            {
                modelname = vehiclemanifest.FirstOrDefault().VehicleModel.Name;
                for (int j = 1; j <= vehiclemanifest.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                {
                    if (!bookedSeat.ContainsKey(j) && (modelname == "Hiace Super Ex" || modelname == "Hiace Ex") && j == 2)
                    {
                        continue;
                    }
                    else if (!bookedSeat.ContainsKey(j) && (j == 5 || j == 14) && modelname == "Hiace Super Ex")
                    {
                        continue;
                    }
                    else if (!bookedSeat.ContainsKey(j))
                    {
                        remainingSeat.Add(j);
                    }
                }
            }
            else
            {
                if (vehicletrip.FirstOrDefault() != null)
                {
                    modelname = vehicletrip.FirstOrDefault().VehicleModel.Name;
                    for (int j = 1; j <= vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                    {
                        if (!bookedSeat.ContainsKey(j) && (modelname == "Hiace Super Ex" || modelname == "Hiace Ex") && j == 2)
                        {
                            continue;
                        }
                        else if (!bookedSeat.ContainsKey(j) && (j == 5 || j == 14) && modelname == "Hiace Super Ex")
                        {
                            continue;
                        }
                        else if (!bookedSeat.ContainsKey(j))
                        {
                            remainingSeat.Add(j);
                        }
                    }
                }
            }

            var manifestManagements =
                from manifestManagement in _repository.GetAll()
                join vehicleTripRegistration in _vehicleTripRepo.GetAll() on manifestManagement.VehicleTripRegistrationId equals vehicleTripRegistration.Id

                let selectedSeats = _seatManagemengtSvc.GetAll()
                           .Where(s => s.VehicleTripRegistrationId == vehicleTripId && s.BookingStatus == BookingStatus.Approved)

                let captain = _driverRepo.GetAll().Where(s => s.Code == vehicleTripRegistration.DriverCode)
                let originalcaptain = _driverRepo.GetAll().Where(s => s.Code == vehicleTripRegistration.OriginalDriverCode)

                where manifestManagement.VehicleTripRegistrationId == vehicleTripId

                select new ManifestDetailDTO
                {
                    Id = manifestManagement.Id,
                    NumberOfSeats = manifestManagement.NumberOfSeats,
                    IsPrinted = manifestManagement.IsPrinted,
                    PrintedBy = manifestManagement.Employee,
                    DepartureDate = vehicleTripRegistration.DepartureDate,
                    DepartureTime = vehicleTripRegistration.Trip.DepartureTime,
                    VehicleTripRegistrationId = vehicleTripId,
                    Route = vehicleTripRegistration.Trip.Route.Name,
                    TotalSold = terminalCashBookings.Sum(i => i.Amount - i.Discount) + partCashBookings.Sum(i => i.PartCash),
                    RescheduleFee = rescheduleFee.Count() * 500,
                    RerouteFee = rerouteFee.Where(i => i.RerouteFeeDiff >= 0).Sum(i => i.RerouteFeeDiff) + rerouteFee.Count() * 500,
                    VehicleModel = vehicleTripRegistration.VehicleModel.Name,
                    DispatchSource = manifestManagement.DispatchSource,
                    DriverCode = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? vehicleTripRegistration.OriginalDriverCode : vehicleTripRegistration.DriverCode,
                    DriverName = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? originalcaptain.FirstOrDefault().Name : captain.FirstOrDefault().Name,
                    DriverPhone = captain.Any() && captain.FirstOrDefault().DriverType == DriverType.Virtual ? originalcaptain.FirstOrDefault().Phone1 : captain.FirstOrDefault().Phone1,
                    TotalSeats = vehiclemanifest.FirstOrDefault() != null ? vehiclemanifest.FirstOrDefault().VehicleModel.NumberOfSeats : vehicleTripRegistration.VehicleModel.NumberOfSeats,
                    RemainingSeat = remainingSeat,
                    BookSeat = seatsBooking,
                    ClashingSeats = seatClashes,
                    TicketPrintStatus = ticketPrintStatus,
                    BookingTypes = bookingTypes,

                    Dispatch = manifestManagement.Dispatch,

                    RemainingSeatCount = vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats - selectedSeats.Count(),
                    Amount = manifestManagement.Amount,
                    BusRegistrationNumber = vehicleTripRegistration.PhysicalBusRegistrationNumber,
                    Passengers = selectedSeats.Select(s => new SeatManagementDTO
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        BookingReferenceCode = s.BookingReferenceCode,
                        NextOfKinName = s.NextOfKinName,
                        NextOfKinPhoneNumber = s.NextOfKinPhoneNumber,
                        IsPrinted = s.IsPrinted,
                        PhoneNumber = s.PhoneNumber,
                        FullName = s.FullName,
                        PassengerType = s.PassengerType,
                        Gender = s.Gender,
                        VehicleTripRegistrationId = s.VehicleTripRegistrationId,
                        Amount = s.Amount ?? manifestManagement.Amount,
                        PartCash = s.PartCash,
                        POSReference = s.POSReference,
                        Discount = s.Discount,
                        RouteId = s.RouteId ?? vehicleTripRegistration.Trip.RouteId,
                        RouteName = vehicleTripRegistration.Trip.Route.Name,
                        PaymentMethod = s.PaymentMethod,
                        BookingType = s.BookingType,
                        SubRouteId = s.SubRouteId,
                        SubRouteName = s.SubRoute.Name ?? s.Route.Name,
                        DateCreated = s.CreationTime,
                        DateModified = s.LastModificationTime,
                        TravelStatus = s.TravelStatus,
                        RescheduleStatus = s.RescheduleStatus,
                        IsRescheduled = s.IsRescheduled,
                        RerouteStatus = s.RerouteStatus,
                        IsRerouted = s.IsRerouted,
                        RerouteFeeDiff = s.RerouteFeeDiff,
                        IsUpgradeDowngrade = s.IsUpgradeDowngrade,
                        UpgradeDowngradeDiff = s.UpgradeDowngradeDiff,
                        UpgradeType = s.UpgradeType,
                        CreatedBy = s.CreatedBy,
                        PickUpPointId = s.PickUpPointId,
                        PickupStatus = s.PickupStatus,
                        IsGhanaRoute = s.IsGhanaRoute,
                        PassportType = s.PassportType,
                        PassportId = s.PassportId,
                        PlaceOfIssue = s.PlaceOfIssue,
                        IssuedDate = s.IssuedDate,
                        ExpiredDate = s.ExpiredDate,
                        Nationality = s.Nationality,

                    }).OrderBy(e => e.SeatNumber)
                };

            return manifestManagements.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateRouteFare(Guid vehicleTripRegistrationId, decimal Amount)
        {
            //var fare = await GetTripFare(vehicleTripReg);
            //var vehicleTrip = await _vehicleTripRepo.GetAsync(vehicleTripReg);
            //if (vehicleTrip == null)
            //{
            //    throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            //}
            //if (vehicleTrip.Id == vehicleTripReg)
            //{
            //    var rerouteFee = _repository.GetAll().Where(s => s.VehicleTripRegistrationId == vehicleTripReg);
            //   // var tripFare = await _fareSvc.GetFareByVehicleTrip(newTrip.RouteId, vehicleTrip.VehicleModelId.GetValueOrDefault());
            //}

            var manifestdetail = _repository.GetAll().Where(s => s.VehicleTripRegistrationId == vehicleTripRegistrationId);

            //manifestdetail.Amount = Amount;
            manifestdetail.FirstOrDefault().Amount = Amount;

            _unitOfWork.SaveChanges();
        }
    }
}