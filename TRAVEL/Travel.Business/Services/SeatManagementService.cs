using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface ISeatManagementService
    {
        IQueryable<SeatManagement> GetAll();
        Task<string> GetRefCode();
        void Add(SeatManagement seatManagement);
        SeatManagement FirstOrDefault(Expression<Func<SeatManagement, bool>> filter);
        Task<bool> ExistAsync(Expression<Func<SeatManagement, bool>> predicate);
        Task<SeatManagementDTO> GetSeatByRefcodeForClientAsync(string refcode);
        Task<SeatManagement> GetAsync(long id);
        Task<List<SeatManagementDTO>> GetAllSeatByRefcodeAsync(string refcode);
        Task<SeatManagementDTO> GetSeatByRefcodeAsync(string refcode);
        Task<List<SeatManagementDTO>> GetBeneficiarySeatByRefcodeAsync(string refcode);
        Task<IEnumerable<SeatManagementDTO>> GetSeatHistoryByPhoneAsync(string phoneNo);
        Task<IEnumerable<SeatManagementDTO>> GetSeatHistoryAsync();
        Task<List<SeatManagementDTO>> GetByVehicleTripIdAsync(Guid vehicleTripRegistrationId);
        Task RemoveSeatFromManifest(int seatManagementId);
        Task RescheduleSeatFromManifest(int seatManagementId);
        Task<SeatManagementDTO>  GetSeatByIdUpdatePrint(int seatManagementId, bool isPrinted);
        Task<SeatManagementDTO> GetSeatAndVehicleModelByRefcodeAsync(string refcode);


    }

    public class SeatManagementService : ISeatManagementService
    {
        private readonly IRepository<SeatManagement, long> _repo;
        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<VehicleTripRegistration, Guid> _vehicleTripRepo;
        private readonly IRepository<Trip, Guid> _tripRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountTransactionService _bookingAcctSvc;
        private readonly IServiceHelper _serviceHelper;
        public SeatManagementService(
            IRepository<SeatManagement, long> repo,
            IRepository<VehicleTripRegistration, Guid> vehicleTripRepo,
               IRepository<Trip, Guid> tripRepo,
            IRepository<Route> routeRepo,
              IServiceHelper serviceHelper,
             IAccountTransactionService bookingAcctSvc,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _routeRepo = routeRepo;
            _unitOfWork = unitOfWork;
            _vehicleTripRepo = vehicleTripRepo;
            _tripRepo = tripRepo;
            _bookingAcctSvc = bookingAcctSvc;
            _serviceHelper = serviceHelper;
          
        }

        public Task<List<SeatManagementDTO>> GetAllSeatByRefcodeAsync(string refcode)
        {
            var seat =
                from allSeat in _repo.GetAll()
                join route in _routeRepo.GetAll() on allSeat.RouteId equals route.Id
                where allSeat.MainBookerReferenceCode == refcode || allSeat.BookingReferenceCode == refcode
                select new SeatManagementDTO
                {
                    Id = allSeat.Id,
                    SeatNumber = allSeat.SeatNumber,
                    RemainingSeat = allSeat.RemainingSeat,
                    IsRescheduled = allSeat.IsRescheduled,
                    RescheduleStatus = allSeat.RescheduleStatus,

                    RerouteStatus = allSeat.RerouteStatus,
                    IsRerouted = allSeat.IsRerouted,
                    IsUpgradeDowngrade = allSeat.IsUpgradeDowngrade,
                    UpgradeDowngradeDiff = allSeat.UpgradeDowngradeDiff,
                    UpgradeType = allSeat.UpgradeType,
                    RerouteFeeDiff = allSeat.RerouteFeeDiff,
                    CreatedBy = allSeat.CreatedBy,
                    BookingReferenceCode = allSeat.BookingReferenceCode,
                    FromTransload = allSeat.FromTransload,
                    Amount = allSeat.Amount,
                    Discount = allSeat.Discount,
                    PassengerType = allSeat.PassengerType,
                    PaymentMethod = allSeat.PaymentMethod,
                    BookingType = allSeat.BookingType,
                    RouteId = allSeat.RouteId,
                    VehicleTripRegistrationId = allSeat.VehicleTripRegistrationId,
                    FullName = allSeat.FullName,
                    Gender = allSeat.Gender,
                    PhoneNumber = allSeat.PhoneNumber,
                    NextOfKinName = allSeat.NextOfKinName,
                    NextOfKinPhoneNumber = allSeat.NextOfKinPhoneNumber,
                    DateCreated = allSeat.CreationTime,
                    IsMainBooker = allSeat.IsMainBooker,
                    HasReturn = allSeat.HasReturn,
                    MainBookerReferenceCode = allSeat.MainBookerReferenceCode,
                    IsReturn = allSeat.IsReturn,
                    NoOfTicket = allSeat.NoOfTicket,
                    RouteName = route.Name,
                    VehicleTripRegistration = allSeat.VehicleTripRegistrationId.ToString(),
                    DepartureDate = allSeat.VehicleTripRegistration.DepartureDate,
                    DepartureTime = allSeat.VehicleTripRegistration.Trip.DepartureTime,
                    DepartureTerminalName = allSeat.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                    PickupPointName = allSeat.PickupPoint.Name,
                    VehicleName = allSeat.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    BookingStatus = allSeat.BookingStatus,
                    TravelStatus = allSeat.TravelStatus
                };

            return seat.AsNoTracking().ToListAsync();
        }

        public void Add(SeatManagement seatManagement)
        {
            _repo.Insert(seatManagement);
        }

        public IQueryable<SeatManagement> GetAll()
        {
            return _repo.GetAllIncluding(x => x.VehicleModel, y=>y.SubRoute,w=>w.VehicleTripRegistration);
        }

        public SeatManagement FirstOrDefault(Expression<Func<SeatManagement, bool>> filter)
        {
            return _repo.FirstOrDefault(filter);
        }

        //Todo: Causes performance hit as db grows!!!
        public async Task<string> GetRefCode()
        {
            bool isUnique = false;
            string otp = "";
            while (isUnique == false) {
                otp = Guid.NewGuid().ToString().Remove(8).ToUpper();

                var exists = await _repo.ExistAsync(a => a.BookingReferenceCode == otp);

                isUnique = !exists;
            }
            return otp;
        }

        public Task<bool> ExistAsync(Expression<Func<SeatManagement, bool>> predicate)
        {
            return _repo.ExistAsync(predicate);
        }

        public Task<SeatManagementDTO> GetSeatByRefcodeForClientAsync(string refcode)
        {
            var allseat = GetAll()
                    .Where(s => s.MainBookerReferenceCode == refcode);

            var seat =
                from allSeat in GetAll()
                join route in _routeRepo.GetAll() on allSeat.RouteId equals route.Id
                where allSeat.BookingReferenceCode == refcode
                select new SeatManagementDTO
                {
                    refCode = allSeat.BookingReferenceCode,
                    Id = allSeat.Id,
                    SeatNumber = allSeat.SeatNumber,
                    RemainingSeat = allSeat.RemainingSeat,
                    CreatedBy = allSeat.CreatedBy,
                    BookingReferenceCode = allSeat.BookingReferenceCode,
                    FromTransload = allSeat.FromTransload,
                    Amount = allseat.Sum(i => i.Amount),
                    Discount = allSeat.Discount,
                    PassengerType = allSeat.PassengerType,
                    PaymentMethod = allSeat.PaymentMethod,
                    BookingType = allSeat.BookingType,
                    RouteId = allSeat.RouteId,
                    VehicleTripRegistrationId = allSeat.VehicleTripRegistrationId,
                    FullName = allSeat.FullName,
                    Gender = allSeat.Gender,
                    PhoneNumber = allSeat.PhoneNumber,
                    NextOfKinName = allSeat.NextOfKinName,
                    NextOfKinPhoneNumber = allSeat.NextOfKinPhoneNumber,
                    DateCreated = allSeat.CreationTime,
                    IsMainBooker = allSeat.IsMainBooker,
                    HasReturn = allSeat.HasReturn,
                    MainBookerReferenceCode = allSeat.MainBookerReferenceCode,
                    IsReturn = allSeat.IsReturn,
                    NoOfTicket = allSeat.NoOfTicket,
                    RouteName = route.Name,
                    VehicleTripRegistration = allSeat.VehicleTripRegistrationId.ToString(),
                    DepartureDate = allSeat.VehicleTripRegistration.DepartureDate,
                    PickupPointName = allSeat.PickupPoint.Name,
                    VehicleName = allSeat.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    BookingStatus = allSeat.BookingStatus,
                    TravelStatus = allSeat.TravelStatus,
                    DepartureTime = allSeat.VehicleTripRegistration.Trip.DepartureTime,
                    DepartureTerminalName = allSeat.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                    DestinationTerminalName = allSeat.VehicleTripRegistration.Trip.Route.DestinationTerminal.Name,
                    DestinationTerminalId = allSeat.Route.DestinationTerminalId,
                    DepartureTerminald = allSeat.Route.DepartureTerminalId,
                    Rated = allSeat.Rated,
                    IsRescheduled = allSeat.IsRescheduled,
                    RescheduleStatus = allSeat.RescheduleStatus,
                    HasCoupon = allSeat.HasCoupon,
                    CouponCode = allSeat.CouponCode
                };

            return seat.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<IEnumerable<SeatManagementDTO>> GetSeatHistoryByPhoneAsync(string phoneNo)
        {
            var seat =
                from seatMgt in GetAll()
                join route in _routeRepo.GetAll() on seatMgt.RouteId equals route.Id
                //join vtr in _vehicleTripRepo.GetAll() on allSeat.VehicleTripRegistrationId equals vtr.Id into  joined
                //from vtr in joined.DefaultIfEmpty()
                where seatMgt.PhoneNumber == phoneNo
                orderby seatMgt.CreationTime descending

                select new SeatManagementDTO
                {
                    refCode = seatMgt.BookingReferenceCode,
                    Id = seatMgt.Id,
                    SeatNumber = seatMgt.SeatNumber,
                    RemainingSeat = seatMgt.RemainingSeat,
                    CreatedBy = seatMgt.CreatedBy,
                    BookingReferenceCode = seatMgt.BookingReferenceCode,
                    FromTransload = seatMgt.FromTransload,
                    Amount = seatMgt.Amount,
                    Discount = seatMgt.Discount,
                    PassengerType = seatMgt.PassengerType,
                    PaymentMethod = seatMgt.PaymentMethod,
                    BookingType = seatMgt.BookingType,
                    RouteId = seatMgt.RouteId,
                    VehicleTripRegistrationId = seatMgt.VehicleTripRegistrationId,
                    FullName = seatMgt.FullName,
                    Gender = seatMgt.Gender,
                    PhoneNumber = seatMgt.PhoneNumber,
                    NextOfKinName = seatMgt.NextOfKinName,
                    NextOfKinPhoneNumber = seatMgt.NextOfKinPhoneNumber,
                    DateCreated = seatMgt.CreationTime,
                    IsMainBooker = seatMgt.IsMainBooker,
                    HasReturn = seatMgt.HasReturn,
                    MainBookerReferenceCode = seatMgt.MainBookerReferenceCode,
                    IsReturn = seatMgt.IsReturn,
                    NoOfTicket = seatMgt.NoOfTicket,
                    RouteName = route.Name,
                    VehicleTripRegistration = seatMgt.VehicleTripRegistrationId.ToString(),
                    DepartureDate = seatMgt.VehicleTripRegistration.DepartureDate,
                    PickupPointName = seatMgt.PickupPoint.Name,
                    VehicleName = seatMgt.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    BookingStatus = seatMgt.BookingStatus,
                    TravelStatus = seatMgt.TravelStatus,
                    DepartureTime = seatMgt.VehicleTripRegistration.Trip.DepartureTime,
                    DepartureTerminalName = seatMgt.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                    DestinationTerminalName = seatMgt.VehicleTripRegistration.Trip.Route.DestinationTerminal.Name,
                    DestinationTerminalId = seatMgt.Route.DestinationTerminalId,
                    DepartureTerminald = seatMgt.Route.DepartureTerminalId,
                    Rated = seatMgt.Rated,
                    IsRescheduled = seatMgt.IsRescheduled,
                    RescheduleStatus = seatMgt.RescheduleStatus,
                    HasCoupon = seatMgt.HasCoupon,
                    CouponCode = seatMgt.CouponCode
                };

            return Task.FromResult(seat.AsNoTracking().AsEnumerable());
        }

        public async Task<SeatManagement> GetAsync(long id)
        {
            //return _repo.GetAsync(id);
            return await Task.FromResult(_repo.GetAllIncluding(x => x.Route).FirstOrDefault(x => x.Id == id));
        }

        public Task<SeatManagementDTO> GetSeatByRefcodeAsync(string refcode)
        {
            var seat =
                from allSeat in GetAll()
                join route in _routeRepo.GetAll() on allSeat.RouteId equals route.Id
                where allSeat.BookingReferenceCode == refcode
                select new SeatManagementDTO
                {
                    refCode = allSeat.BookingReferenceCode,
                    Id = allSeat.Id,
                    SeatNumber = allSeat.SeatNumber,
                    RemainingSeat = allSeat.RemainingSeat,
                    CreatedBy = allSeat.CreatedBy,
                    BookingReferenceCode = allSeat.BookingReferenceCode,
                    FromTransload = allSeat.FromTransload,
                    Amount = allSeat.Amount,
                    Discount = allSeat.Discount,
                    PassengerType = allSeat.PassengerType,
                    PaymentMethod = allSeat.PaymentMethod,
                    BookingType = allSeat.BookingType,
                    RouteId = allSeat.RouteId,
                    VehicleTripRegistrationId = allSeat.VehicleTripRegistrationId,
                    FullName = allSeat.FullName,
                    Gender = allSeat.Gender,
                    PhoneNumber = allSeat.PhoneNumber,
                    NextOfKinName = allSeat.NextOfKinName,
                    NextOfKinPhoneNumber = allSeat.NextOfKinPhoneNumber,
                    DateCreated = allSeat.CreationTime,
                    IsMainBooker = allSeat.IsMainBooker,
                    HasReturn = allSeat.HasReturn,
                    HasCoupon = allSeat.HasCoupon,
                    CouponCode = allSeat.CouponCode,
                    MainBookerReferenceCode = allSeat.MainBookerReferenceCode,
                    IsReturn = allSeat.IsReturn,
                    NoOfTicket = allSeat.NoOfTicket,
                    RouteName = route.Name,
                    VehicleTripRegistration = allSeat.VehicleTripRegistrationId.ToString(),
                    DepartureDate = allSeat.VehicleTripRegistration.DepartureDate,
                    PickupPointName = allSeat.PickupPoint.Name,
                    VehicleName = allSeat.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    BookingStatus = allSeat.BookingStatus,
                    TravelStatus = allSeat.TravelStatus,
                    DepartureTime = allSeat.VehicleTripRegistration.Trip.DepartureTime,
                    DepartureTerminalName = allSeat.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                    DestinationTerminalName = allSeat.VehicleTripRegistration.Trip.Route.DestinationTerminal.Name,
                    Rated = allSeat.Rated,
                    IsRescheduled = allSeat.IsRescheduled,
                    RescheduleStatus = allSeat.RescheduleStatus
                };

            return seat.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<SeatManagementDTO> GetSeatByIdAsync(long seatManagementId)
        {
            var seat =
                from allSeat in GetAll()
              
                where allSeat.Id == seatManagementId

                let vehicletrip = _vehicleTripRepo.GetAll().FirstOrDefault(a => a.Id == allSeat.VehicleTripRegistrationId)
                let tripcode = _tripRepo.GetAll().FirstOrDefault(a => a.Id == vehicletrip.TripId)
                let route = _routeRepo.GetAll().FirstOrDefault(a => a.Id == allSeat.RouteId)

                select new SeatManagementDTO
                {
                    refCode = allSeat.BookingReferenceCode,
                    Id = allSeat.Id,
                    SeatNumber = allSeat.SeatNumber,
                    RemainingSeat = allSeat.RemainingSeat,
                    FromTransload = allSeat.FromTransload,
                    CreatedBy = allSeat.CreatedBy,
                    BookingReferenceCode = allSeat.BookingReferenceCode,
                    TripCode = tripcode.TripCode,
                    VehicleModelId = tripcode.VehicleModelId,
                    Amount = allSeat.Amount - allSeat.Discount,
                    Discount = allSeat.Discount,
                    PassengerType = allSeat.PassengerType,
                    PaymentMethod = allSeat.PaymentMethod,
                    BookingType = allSeat.BookingType,
                    RouteId = allSeat.RouteId,
                    PreviousRouteId = allSeat.RouteId,
                    DepartureDate = allSeat.VehicleTripRegistration.DepartureDate,
                    VehicleName = allSeat.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    DepartureTime = allSeat.VehicleTripRegistration.Trip.DepartureTime,
                    RouteName = allSeat.SubRoute.Name ?? route.Name,
                    VehicleTripRegistrationId = allSeat.VehicleTripRegistrationId,
                    IsPrinted = allSeat.IsPrinted,
                    FullName = allSeat.FullName,
                    Gender = allSeat.Gender,
                    PhoneNumber = allSeat.PhoneNumber,
                    NextOfKinName = allSeat.NextOfKinName,
                    NextOfKinPhoneNumber = allSeat.NextOfKinPhoneNumber,
                    IsMainBooker = allSeat.IsMainBooker,
                    HasReturn = allSeat.HasReturn,
                    MainBookerReferenceCode = allSeat.MainBookerReferenceCode,
                    IsReturn = allSeat.IsReturn,
                    TravelStatus = allSeat.TravelStatus,
                    IsRescheduled = allSeat.IsRescheduled,
                    RescheduleStatus = allSeat.RescheduleStatus,
                    IsRerouted = allSeat.IsRerouted,
                    RerouteStatus = allSeat.RerouteStatus,
                    IsSub = allSeat.IsSub,
                    IsSubReturn = allSeat.IsSubReturn,
                    OnlineSubRouteName = allSeat.OnlineSubRouteName,
                    RerouteFeeDiff = allSeat.RerouteFeeDiff
                };

            return seat.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<List<SeatManagementDTO>> GetBeneficiarySeatByRefcodeAsync(string refcode)
        {
            var seat =
                from allSeat in GetAll()
                where allSeat.MainBookerReferenceCode == refcode && allSeat.IsMainBooker != true
                select new SeatManagementDTO
                {
                    Id = allSeat.Id,
                    SeatNumber = allSeat.SeatNumber,
                    RemainingSeat = allSeat.RemainingSeat,
                    BookingReferenceCode = allSeat.BookingReferenceCode,
                    FromTransload = allSeat.FromTransload,
                    CreatedBy = allSeat.CreatedBy,
                    Amount = allSeat.Amount,
                    Discount = allSeat.Discount,
                    PassengerType = allSeat.PassengerType,
                    PaymentMethod = allSeat.PaymentMethod,
                    BookingType = allSeat.BookingType,
                    RouteId = allSeat.RouteId,
                    VehicleTripRegistrationId = allSeat.VehicleTripRegistrationId,
                    IsPrinted = allSeat.IsPrinted,
                    FullName = allSeat.FullName,
                    Gender = allSeat.Gender,
                    PhoneNumber = allSeat.PhoneNumber,
                    NextOfKinName = allSeat.NextOfKinName,
                    NextOfKinPhoneNumber = allSeat.NextOfKinPhoneNumber,
                    DateCreated = allSeat.CreationTime,
                    IsMainBooker = allSeat.IsMainBooker,
                    HasReturn = allSeat.HasReturn,
                    MainBookerReferenceCode = allSeat.MainBookerReferenceCode,
                    IsReturn = allSeat.IsReturn,
                    TravelStatus = allSeat.TravelStatus,
                    IsRescheduled = allSeat.IsRescheduled,
                    RescheduleStatus = allSeat.RescheduleStatus
                };
            return seat.AsNoTracking().ToListAsync();
        }

        public Task<List<SeatManagementDTO>> GetByVehicleTripIdAsync(Guid vehicleTripRegistrationId)
        {
            var seatManagements =
                from seatManagement in _repo.GetAll()

                where seatManagement.VehicleTripRegistrationId == vehicleTripRegistrationId
                select new SeatManagementDTO
                {
                    Id = seatManagement.Id,
                    SeatNumber = seatManagement.SeatNumber,
                    RemainingSeat = seatManagement.RemainingSeat,
                    FromTransload = seatManagement.FromTransload,
                    BookingReferenceCode = seatManagement.BookingReferenceCode,
                    PhoneNumber = seatManagement.PhoneNumber,
                    CreatedBy = seatManagement.CreatedBy,
                    NextOfKinName = seatManagement.NextOfKinName,
                    Amount = seatManagement.Amount,
                    Discount = seatManagement.Discount,
                    NextOfKinPhoneNumber = seatManagement.NextOfKinPhoneNumber,
                    PickupPointImage = seatManagement.PickupPointImage,
                    FullName = seatManagement.FullName,
                    BookingStatus = seatManagement.BookingStatus,
                    Gender = seatManagement.Gender,
                    PickupStatus = seatManagement.PickupStatus,
                    BookingType = seatManagement.BookingType,
                    TravelStatus = seatManagement.TravelStatus,
                    RouteId = seatManagement.RouteId,

                    PickUpPointId = seatManagement.PickUpPointId,
                    NoOfTicket = seatManagement.NoOfTicket,
                    SubRouteId = seatManagement.SubRouteId,

                    PassengerType = seatManagement.PassengerType,
                    PaymentMethod = seatManagement.PaymentMethod,
                    IsMainBooker = seatManagement.IsMainBooker,
                    HasReturn = seatManagement.HasReturn,
                    MainBookerReferenceCode = seatManagement.MainBookerReferenceCode,
                    IsReturn = seatManagement.IsReturn,
                    IsRescheduled = seatManagement.IsRescheduled,
                    RescheduleStatus = seatManagement.RescheduleStatus
                };

            return seatManagements.AsNoTracking().ToListAsync();
        }

        public async Task RescheduleSeatFromManifest(int seatManagementId)
        {
            var existingSeatManagement = await GetAsync(seatManagementId);

            if (existingSeatManagement == null)
            {
                throw new LMEGenericException(ErrorConstants.SEAT_MANAGEMENT_NOT_EXIST);
            }

            existingSeatManagement.SeatNumber = 0;
            existingSeatManagement.VehicleTripRegistrationId = null;
            existingSeatManagement.TravelStatus = TravelStatus.Rescheduled;
            existingSeatManagement.IsRescheduled = true;
            existingSeatManagement.RescheduleMode = RescheduleMode.Admin;
            existingSeatManagement.RescheduleStatus = RescheduleStatus.PayAtTerminal;
            // existingSeatManagement.SubRouteId = null;
            //   existingSeatManagement.RouteId = null;

            await _unitOfWork.SaveChangesAsync();
        }


        public async Task RemoveSeatFromManifest(int seatManagementId)
        {
            var existingSeatManagement = await GetAsync(seatManagementId);

            if (existingSeatManagement == null)
            {
                throw new LMEGenericException("Seat Does not Exist", $"{(int)HttpStatusCode.BadRequest}");
            }


            var vehicleTripId = existingSeatManagement.VehicleTripRegistrationId;

            existingSeatManagement.SeatNumber = 0;
            existingSeatManagement.VehicleTripRegistrationId = null;
            existingSeatManagement.TravelStatus = TravelStatus.NoShow;
            // existingSeatManagement.SubRouteId = null;
            //   existingSeatManagement.RouteId = null;


            // Post a negative account transaction to update the account summary
            if (existingSeatManagement.BookingType == BookingTypes.Terminal)
                 _bookingAcctSvc.UpdateBookingAccount(existingSeatManagement.BookingType, existingSeatManagement.PaymentMethod,
                    existingSeatManagement.BookingReferenceCode, vehicleTripId.GetValueOrDefault(), (existingSeatManagement.Amount - existingSeatManagement.Discount).GetValueOrDefault(),
                    TransactionType.Debit);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SeatManagementDTO> GetSeatByIdUpdatePrint(int seatManagementId, bool isPrinted)
        {
            var seatManagement = await GetAsync(seatManagementId);

            if (seatManagement == null)
            {
                throw new LMEGenericException(ErrorConstants.SEAT_MANAGEMENT_NOT_EXIST);
            }

            //if (seatManagement.IsRescheduled == true && !seatManagement.IsPrinted && seatManagement.RescheduleStatus == RescheduleStatus.PayAtTerminal)
            //{
            //    var loginUserEmailAddress = _serviceHelper.GetCurrentUserEmail();

            //    OtherIncomeDto otherIncome = new OtherIncomeDto();

            //    otherIncome.Amount = 1000;
            //    otherIncome.PaymentName = "Reschedule Fee";
            //    otherIncome.PaymentDescription = "Payment for Terminal Reschedule";

            //    await _otherIncomeService.AddOtherIncome(otherIncome);
            //}
            //if (seatManagement.IsRerouted == true && !seatManagement.IsPrinted && seatManagement.RerouteStatus == RerouteStatus.PayAtTerminal)
            //{
            //    var loginUserEmailAddress = _serviceHelper.GetCurrentUserEmail();

            //    OtherIncomeDto otherIncome = new OtherIncomeDto();

            //    otherIncome.Amount = 1000;
            //    otherIncome.PaymentName = "Reroute Fee";
            //    otherIncome.PaymentDescription = "Payment for Reroute";

            //    await _otherIncomeService.AddOtherIncome(otherIncome);
            //}

            var seat = await GetAsync(seatManagementId);
            if (seat.IsRerouted == true && !seat.IsPrinted && seat.RerouteStatus == RerouteStatus.PayAtTerminal && seat.RerouteFeeDiff.GetValueOrDefault() > 0)
            {
                seat.Amount = seat.Amount + seat.RerouteFeeDiff;
            }
            seat.IsPrinted = isPrinted;

            await _unitOfWork.SaveChangesAsync();
            return await GetSeatByIdAsync(seatManagementId);
           
        }



        public Task<IEnumerable<SeatManagementDTO>> GetSeatHistoryAsync()
        {
            var seat =
                 from seatMgt in GetAll()
                 join route in _routeRepo.GetAll() on seatMgt.RouteId equals route.Id
                 where seatMgt.CreatorUserId == _serviceHelper.GetCurrentUserId()
                 orderby seatMgt.CreationTime descending

                 select new SeatManagementDTO
                 {
                     refCode = seatMgt.BookingReferenceCode,
                     Id = seatMgt.Id,
                     SeatNumber = seatMgt.SeatNumber,
                     RemainingSeat = seatMgt.RemainingSeat,
                     CreatedBy = seatMgt.CreatedBy,
                     BookingReferenceCode = seatMgt.BookingReferenceCode,
                     FromTransload = seatMgt.FromTransload,
                     Amount = seatMgt.Amount,
                     Discount = seatMgt.Discount,
                     PassengerType = seatMgt.PassengerType,
                     PaymentMethod = seatMgt.PaymentMethod,
                     BookingType = seatMgt.BookingType,
                     RouteId = seatMgt.RouteId,
                     VehicleTripRegistrationId = seatMgt.VehicleTripRegistrationId,
                     FullName = seatMgt.FullName,
                     Gender = seatMgt.Gender,
                     PhoneNumber = seatMgt.PhoneNumber,
                     NextOfKinName = seatMgt.NextOfKinName,
                     NextOfKinPhoneNumber = seatMgt.NextOfKinPhoneNumber,
                     DateCreated = seatMgt.CreationTime,
                     IsMainBooker = seatMgt.IsMainBooker,
                     HasReturn = seatMgt.HasReturn,
                     MainBookerReferenceCode = seatMgt.MainBookerReferenceCode,
                     IsReturn = seatMgt.IsReturn,
                     NoOfTicket = seatMgt.NoOfTicket,
                     RouteName = route.Name,
                     VehicleTripRegistration = seatMgt.VehicleTripRegistrationId.ToString(),
                     DepartureDate = seatMgt.VehicleTripRegistration.DepartureDate,
                     PickupPointName = seatMgt.PickupPoint.Name,
                     VehicleName = seatMgt.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                     BookingStatus = seatMgt.BookingStatus,
                     TravelStatus = seatMgt.TravelStatus,
                     DepartureTime = seatMgt.VehicleTripRegistration.Trip.DepartureTime,
                     DepartureTerminalName = seatMgt.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                     DestinationTerminalName = seatMgt.VehicleTripRegistration.Trip.Route.DestinationTerminal.Name,
                     DestinationTerminalId = seatMgt.Route.DestinationTerminalId,
                     DepartureTerminald = seatMgt.Route.DepartureTerminalId,
                     Rated = seatMgt.Rated,
                     IsRescheduled = seatMgt.IsRescheduled,
                     RescheduleStatus = seatMgt.RescheduleStatus,
                     HasCoupon = seatMgt.HasCoupon,
                     CouponCode = seatMgt.CouponCode
                 };

            return Task.FromResult(seat.AsNoTracking().AsEnumerable());
        }


        public Task<SeatManagementDTO> GetSeatAndVehicleModelByRefcodeAsync(string refcode)
        {
            var seat =
                from  SeatManagements in _repo.GetAll()
                join vehicleTripRegistration in _repo.GetAll() on SeatManagements.VehicleTripRegistrationId equals vehicleTripRegistration.VehicleTripRegistrationId
                join trip in _repo.GetAll() on vehicleTripRegistration.BookingId equals trip.BookingId
                join route in _repo.GetAll() on trip.RouteId equals route.RouteId


                where SeatManagements.BookingReferenceCode == refcode
                select new SeatManagementDTO
                {
                    refCode = SeatManagements.BookingReferenceCode,
                    Id = SeatManagements.Id,
                    SeatNumber = SeatManagements.SeatNumber,
                    RemainingSeat = SeatManagements.RemainingSeat,
                    CreatedBy = SeatManagements.CreatedBy,
                    BookingReferenceCode = SeatManagements.BookingReferenceCode,
                    FromTransload = SeatManagements.FromTransload,
                    Amount = SeatManagements.Amount,

                    VehicleModelId = vehicleTripRegistration.VehicleModelId,
                    Discount = SeatManagements.Discount,
                    PassengerType = SeatManagements.PassengerType,
                    PaymentMethod = SeatManagements.PaymentMethod,
                    BookingType = SeatManagements.BookingType,
                    RouteId = SeatManagements.RouteId,
                    VehicleTripRegistrationId = SeatManagements.VehicleTripRegistrationId,
                    FullName = SeatManagements.FullName,
                    Gender = SeatManagements.Gender,
                    PhoneNumber = SeatManagements.PhoneNumber,
                    NextOfKinName = SeatManagements.NextOfKinName,
                    NextOfKinPhoneNumber = SeatManagements.NextOfKinPhoneNumber,
                    DateCreated = SeatManagements.CreationTime,
                    IsMainBooker = SeatManagements.IsMainBooker,
                    HasReturn = SeatManagements.HasReturn,
                    //hasCoupon = SeatManagements.hasCoupon,
                    CouponCode = SeatManagements.CouponCode,
                    MainBookerReferenceCode = SeatManagements.MainBookerReferenceCode,
                    IsReturn = SeatManagements.IsReturn,
                    NoOfTicket = SeatManagements.NoOfTicket,
                    RouteName = SeatManagements.Route.Name,
                    VehicleTripRegistration = SeatManagements.VehicleTripRegistrationId.ToString(),
                    DepartureDate = SeatManagements.VehicleTripRegistration.DepartureDate,
                    //PickupPointName = SeatManagements.PickupPoint.PickupPointName,
                    VehicleName = SeatManagements.VehicleTripRegistration.PhysicalBusRegistrationNumber,
                    BookingStatus = SeatManagements.BookingStatus,
                    TravelStatus = SeatManagements.TravelStatus,
                    DepartureTime = SeatManagements.VehicleTripRegistration.Trip.DepartureTime,
                    DepartureTerminalName = SeatManagements.VehicleTripRegistration.Trip.Route.DepartureTerminal.Name,
                    DestinationTerminalName = SeatManagements.VehicleTripRegistration.Trip.Route.DestinationTerminal.Name,
                    Rated = SeatManagements.Rated,
                    IsRescheduled = SeatManagements.IsRescheduled,
                    RescheduleStatus = SeatManagements.RescheduleStatus
                };

            return seat.AsNoTracking().FirstOrDefaultAsync();
        }


    }
}