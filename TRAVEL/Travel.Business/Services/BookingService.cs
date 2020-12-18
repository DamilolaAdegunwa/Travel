using IPagedList;
using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Core.Timing;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Email;
using Travel.Core.Messaging.Sms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayStack.Net;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Travel.Core.Common.Enums.TripType;
using Travel.Core.Domain.Entities;
using Travel.Core.Messaging.Email;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Business.Services
{
    public interface IBookingService
    {
        Task<List<SeatManagementDTO>> GetTraveledCustomers(DateTime startdate);
        Task<IPagedList<SeatManagementDTO>> GetTraveledCustomersAsync(DateTime startdate, DateTime enddate, int pageNumber, int pageSize, string query);
        SeatManagementDTO GetFarePriceByVehicleTripId(Guid vehicleTripRegistrationId);
        Task<GroupedTripsDetailDTO> GetAvailableTripDetails(VehicleTripRouteSearchDTO tripBookingSearch);
        Task<GroupedTripsDetailDTO> GetAvailableTripTerminalDetails(VehicleTripRouteSearchDTO tripBookingSearch);
        Task<BookingResponseDTO> PostBooking(BookingDetailDTO bookingdetail);
        Task<SeatManagementDTO> GetBookingDetails(string refCode);
        Task<List<SeatManagementDTO>> GetAllBookingDetails(string refCode);
        Task UpdateBooking(BookingDetailDTO bookingdetail);
        Task<decimal?> GetTripFare(int subrouteId, Guid vehicleTripRegistrationId);   
        Task<decimal?> GetNewRouteFare(int routeId, int modelId);
        Task<decimal?> GetNonIdAmount(int routeId, int modelId);
        Task<decimal?> GetPassengerFare(string passengerInfo);
        Task<decimal?> GetMainTripFare(Guid vehicleTripRegistrationId);
        Task<RemainingSeatDTO> GetAvailableseats(Guid vehicletripregistrationId);
        Task<RemainingSeatDTO> GetseatsAvailable(Guid vehicletripregistrationId);
        Task UpgradeDowngradeTicket(ManifestExt ticketDetail);
        Task AddRefCodeToBooking(ManifestExt refCodeDetail);
        Task CancelBooking(string bookingReferenceCode);
        Task SuspendBooking(string bookingReferenceCode);
        Task ApproveBooking(long seatManagementId);
        Task SwapVehicle(SwapVehicleDTO swapvehicle);
        Task<BookingResponseDTO> ProcessPaystackPayment(string RefCode);
        Task<BookingResponseDTO> ProcessPaystackWebhook(string RefCode);
        Task<IEnumerable<SeatManagementDTO>> GetBookingHistory(string phoneNo);
        Task<IEnumerable<SeatManagementDTO>> GetBookingHistory();
        Task<BookingDTO> GetCustomerByPhone(string Phone);
        Task RevalidateAllPending(int time);
        Task<List<TicketRescheduleDTO>> RescheduleTicketSearch(TicketRescheduleDTO ticketbooking);
        Task<string> RescheduleBooking(RescheduleDTO reschedule);

        Task<string> Reroutebooking(RescheduleDTO reroute);


    }

    public class BookingService : IBookingService
    {
        private readonly BookingConfig bookingConfig;
        private readonly AppConfig appConfig;
        private readonly PaymentConfig.Paystack payStackConfig;

        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<SubRoute> _subRouteRepo;
        private readonly IRepository<AccountSummary, Guid> _accountSummaryRepo;
        private readonly IRepository<ExcludedSeat> _excludedSeatRepo;
        private readonly IRepository<VehicleMake> _vehicleMakeRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<Vehicle> _vehiclesRepo;
        private readonly IRepository<PayStackWebhookResponse> _paystackWebHookResponseRepo;

        private readonly ITripService _tripSvc;
        private readonly IRouteService _routeSvc;
        private readonly IAccountTransactionService _bookingAcctSvc;
        private readonly ITripAvailabilityService _tripAvailabilitySvc;
        private readonly IPickupPointService _pickupPointSvc;
        private readonly IUserService _userSvc;
        private readonly IDiscountService _discountSvc;
        private readonly IFareCalendarService _fareCalendarSvc;
        private readonly ISeatManagementService _seatManagemntSvc;
        private readonly IVehicleTripRegistrationService _vehicleTripRegSvc;

        private readonly IMailService _mailSvc;
        private readonly IEmployeeService _employeeSvc;
        private readonly IServiceHelper _serviceHelper;
        private readonly ISMSService _smsSvc;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICouponService _couponSvc;
        private readonly IFareService _fareSvc;
        private readonly IPassportTypeService _passportTypeService;

        public BookingService(IOptions<BookingConfig> _bookingConfig, IOptions<AppConfig> _appConfig,
            IOptions<PaymentConfig.Paystack> _payStackConfig,
            ITripService tripSvc,
            IRepository<Terminal> terminalRepo,
            IRepository<Booking> bookingRepo,
            IRepository<SubRoute> subRouteRepo,
            IRepository<AccountSummary, Guid> accountSummaryRepo,
            IRepository<ExcludedSeat> excludedSeatRepo,
            IRepository<VehicleMake> vehicleMakeRepo,
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<Vehicle> vehiclesRepo,
            IRepository<PayStackWebhookResponse> paystackWebHookResponseRepo,
            IRouteService routeSvc,
            IAccountTransactionService bookingAcctSvc,
            ITripAvailabilityService tripAvailabilitySvc,
            IPickupPointService pickupPointSvc,
            IUserService userSvc,
            IDiscountService discountSvc,
            IFareCalendarService fareCalendarSvc,
            ISeatManagementService seatManagemntSvc,
            IVehicleTripRegistrationService vehicleTripRegSvc,
            IMailService mailSvc, IEmployeeService employeeSvc,
            IServiceHelper serviceHelper,
            ISMSService smsSvc,
            IUnitOfWork unitOfWork,
            IHostingEnvironment hostingEnvironment,
            ICouponService couponSvc,
            IFareService fareSvc,
            IPassportTypeService passportTypeService)

        {
            bookingConfig = _bookingConfig.Value;
            appConfig = _appConfig.Value;
            payStackConfig = _payStackConfig.Value;
            _tripSvc = tripSvc;
            _terminalRepo = terminalRepo;
            _bookingRepo = bookingRepo;
            _subRouteRepo = subRouteRepo;
            _accountSummaryRepo = accountSummaryRepo;
            _excludedSeatRepo = excludedSeatRepo;
            _vehicleMakeRepo = vehicleMakeRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _vehiclesRepo = vehiclesRepo;
            _routeSvc = routeSvc;
            _bookingAcctSvc = bookingAcctSvc;
            _tripAvailabilitySvc = tripAvailabilitySvc;
            _pickupPointSvc = pickupPointSvc;
            _userSvc = userSvc;
            _discountSvc = discountSvc;
            _fareCalendarSvc = fareCalendarSvc;
            _seatManagemntSvc = seatManagemntSvc;
            _vehicleTripRegSvc = vehicleTripRegSvc;
            _mailSvc = mailSvc;
            _employeeSvc = employeeSvc;
            _serviceHelper = serviceHelper;
            _smsSvc = smsSvc;
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
            _couponSvc = couponSvc;
            _fareSvc = fareSvc;
            _passportTypeService = passportTypeService;
            _paystackWebHookResponseRepo = paystackWebHookResponseRepo;
        }

        public Task<List<TicketBookingDTO>> PendingTicketBooking(int transactionDate)
        {

            var TransactionDate = Clock.Now.AddDays(transactionDate);
            var UpwardLimit = Clock.Now.AddMinutes(-20);

            var todayticketbookings =
                 from seatmanagement in _seatManagemntSvc.GetAll()
                 join vehicletripregistration in _vehicleTripRegSvc.GetAll() on seatmanagement.VehicleTripRegistrationId equals vehicletripregistration.Id
                 where seatmanagement.CreationTime >= TransactionDate
                 && seatmanagement.CreationTime <= UpwardLimit
                 && seatmanagement.BookingStatus == BookingStatus.Pending
                 && (seatmanagement.BookingType == BookingTypes.Online)
                 && (seatmanagement.PaymentMethod == PaymentMethod.PayStack ||
                 seatmanagement.PaymentMethod == PaymentMethod.Isonhold
                 )
                 && !seatmanagement.FromTransload
                 && seatmanagement.IsMainBooker == true
                 && seatmanagement.VehicleTripRegistrationId != null


                 select new TicketBookingDTO
                 {
                     VehicleTripRegistrationId = seatmanagement.VehicleTripRegistrationId,
                     Refcode = seatmanagement.BookingReferenceCode,
                     CustomerNumber = seatmanagement.PhoneNumber,
                     BookedDate = seatmanagement.CreationTime,
                     DepartureDate = vehicletripregistration.DepartureDate,
                     Amount = seatmanagement.Amount,
                     BookingType = seatmanagement.BookingType,
                     VehicleModel = vehicletripregistration.VehicleModel.Name,
                     SeatNumber = seatmanagement.SeatNumber,
                     SeatManagementId = seatmanagement.Id,
                     CustomerName = seatmanagement.FullName,
                     NoofTicket = seatmanagement.NoOfTicket,
                     RouteId = seatmanagement.RouteId,
                     BookingStatus = seatmanagement.BookingStatus,
                     TravelStatus = seatmanagement.TravelStatus,
                     IsRescheduled = seatmanagement.IsRescheduled,
                     RescheduleStatus = seatmanagement.RescheduleStatus,
                     RerouteReferenceCode = seatmanagement.RerouteReferenceCode,
                     RerouteStatus = seatmanagement.RerouteStatus,
                     IsRerouted = seatmanagement.IsRerouted,
                     RerouteFeeDiff = seatmanagement.RerouteFeeDiff,
                     CreatedBy = seatmanagement.CreatedBy,
                     PaymentMethod = seatmanagement.PaymentMethod
                 };

            return todayticketbookings.ToListAsync();
        }

        public async Task RevalidateAllPending(int txnTime)
        {
            var allPendingBookings = await PendingTicketBooking(txnTime);

            if (allPendingBookings.Count > 0)
            {

                foreach (var booking in allPendingBookings)
                {
                    var newDate = booking.BookedDate.AddHours(2);
                    var newDate1Hour = booking.BookedDate.AddMinutes(90);


                    if ((booking.PaymentMethod == PaymentMethod.PayStack) && Clock.Now >= newDate)
                    {
                        try
                        {
                            await AutoReleaseSeat(booking.Refcode);
                        }
                        catch { }
                    }

                    else
                    {
                        if (booking.PaymentMethod == PaymentMethod.PayStack)
                        {
                            try
                            {

                                await ProcessPaystackPayment(booking.Refcode);
                            }
                            catch (Exception ex)
                            {
                                //nothing
                            }

                        }

                        if (booking.PaymentMethod == PaymentMethod.Isonhold)
                        {

                            if (booking.BookingStatus == BookingStatus.Pending && Clock.Now >= newDate)
                            {
                                try
                                {
                                    await AutoReleaseSeat(booking.Refcode);

                                }
                                catch { }
                                //await AutoReleaseSeat(booking.Refcode);
                            }
                        }
                    }
                }
            }
        }


        public async Task ApproveBooking(long seatManagementId)
        {
            if (seatManagementId != 0)
            {

                var booking = await _seatManagemntSvc.GetAsync(seatManagementId);
                if (booking != null)
                {
                    booking.BookingStatus = BookingStatus.Approved;
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SuspendBooking(string refCode)
        {
            if (refCode != null)
            {
                var refcode = refCode.Trim();
                var booking = await _seatManagemntSvc.GetSeatByRefcodeAsync(refcode);
                if (booking != null)
                {

                    var existingbooking = await _seatManagemntSvc.GetAsync(booking.Id);

                    existingbooking.VehicleTripRegistrationId = null;
                    existingbooking.SeatNumber = 0;

                    existingbooking.BookingStatus = BookingStatus.Suspended;
                }

                await _unitOfWork.SaveChangesAsync();

                string customEmail = "noemail@libmot.com";
                var mainbooker = await GetBookingByRefcodeAsync(booking?.MainBookerReferenceCode);
                string smsMessage = "";
                smsMessage =
                    $"Your booking with refcode: {refcode} has been suspended. Contact the customer experience center to reschedule.";
                try
                {
                    _smsSvc.SendSMSNow(smsMessage, recipient: booking.PhoneNumber.ToNigeriaMobile());
                    if (!mainbooker.Email.Equals(customEmail) && mainbooker.Email != null)
                    {

                        var mail = new Mail(appConfig.AppEmail, "Booking Suspension Notification!", mainbooker.Email)
                        {
                            Body = "Your booking with reference code: " + refcode +
                           " has been suspended.\r Please contact the customer experience center to reschedule for a later date." +
                           ".\r\n\r"
                        };

                        _mailSvc.SendMail(mail);
                    }
                }
                catch
                {
                }
            }
        }

        public async Task SwapVehicle(SwapVehicleDTO swapvehicle)
        {
            var vhicleName = await _vehiclesRepo.GetAsync(swapvehicle.VehicleId);

            if (swapvehicle == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var vehicletrip = await _vehicleTripRegSvc.GetAsync(swapvehicle.VehicleTripRegistrationId);

            vehicletrip.PhysicalBusRegistrationNumber = vhicleName.RegistrationNumber;
            vehicletrip.DriverCode = swapvehicle.DriverCode;


            if (!string.IsNullOrEmpty(swapvehicle.OriginalDriverCode))
            {
                vehicletrip.OriginalDriverCode = swapvehicle.OriginalDriverCode;
            }
            else
            {
                vehicletrip.OriginalDriverCode = "";
            }
            var email = await _userSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            var employee = await _employeeSvc.GetEmployeesByemailAsync(email.Email);

            if (employee != null && employee.TerminalId != null && vhicleName.Status != VehicleStatus.InWorkshop)
            {
                vhicleName.LocationId = employee.TerminalId;
            }

            if (vhicleName != null && vhicleName.Status != VehicleStatus.InWorkshop)
            {
                vhicleName.Status = VehicleStatus.TerminalUse;
            }


            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelBooking(string refCode)
        {
            if (refCode != null)
            {
                var refcode = refCode.Trim();
                var bookings = await _seatManagemntSvc.GetAllSeatByRefcodeAsync(refcode);
                if (bookings != null)
                {
                    foreach (var booking in bookings)
                    {

                        var existingbooking = await _seatManagemntSvc.GetAsync(booking.Id);

                        var vehicleTripId = existingbooking.VehicleTripRegistrationId;

                        existingbooking.VehicleTripRegistrationId = null;
                        existingbooking.SeatNumber = 0;

                        existingbooking.BookingStatus = BookingStatus.Cancelled;

                        // Post a negative transaction for this booking

                        _bookingAcctSvc.UpdateBookingAccount(existingbooking.BookingType,
                                               existingbooking.PaymentMethod,
                                               existingbooking.BookingReferenceCode,
                                               vehicleTripId.GetValueOrDefault(),
                                               (existingbooking.Amount - existingbooking.Discount).GetValueOrDefault(),
                                               TransactionType.Debit);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpgradeDowngradeTicket(ManifestExt refCodeDetail)
        {
            if (refCodeDetail?.Id != null)
            {
                var booking = await _seatManagemntSvc.GetAsync(refCodeDetail.Id.GetValueOrDefault());
                var vehicletrip = await _vehicleTripRegSvc.GetAsync(refCodeDetail.VehicleTripRegistrationId);
                var vehiclemodelid = vehicletrip.VehicleModelId;
                var routeid = booking.SubRouteId != null ? booking.SubRoute.NameId : booking.RouteId;
                var previousfare = await _fareSvc.GetFareByVehicleTrip(routeid.GetValueOrDefault(), booking.VehicleModelId.GetValueOrDefault());

                var fare = await _fareSvc.GetFareByVehicleTrip(routeid.GetValueOrDefault(), vehiclemodelid.GetValueOrDefault());
                var fareDiff = fare.Amount - previousfare.Amount;
                if (booking != null && fare != null && fareDiff != 0)
                {
                    booking.Amount = booking.Amount + Math.Round(fareDiff);
                    booking.UpgradeDowngradeDiff = Math.Round(fareDiff);
                    booking.VehicleModelId = vehiclemodelid;
                    booking.IsUpgradeDowngrade = true;
                    booking.UpgradeType = Math.Round(fareDiff) > 0 ? UpgradeType.Upgrade : UpgradeType.Downgrade;
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }


        public async Task AddRefCodeToBooking(ManifestExt refCodeDetail)
        {
            //Ticket Validity set to 30 days for now. This will be made dynamic to come from db later
            int ticketValidity = 30;

            if (refCodeDetail?.Refcode != null)
            {
                var refcode = refCodeDetail?.Refcode.Trim();
                var booking = await _seatManagemntSvc.GetSeatByRefcodeAsync(refcode);
                if (booking != null)
                {

                    var existingbooking = await _seatManagemntSvc.GetAsync(booking.Id);

                    if (DateTime.Now < existingbooking.CreationTime.AddDays(ticketValidity) && existingbooking.SeatNumber == 0 && existingbooking.VehicleTripRegistrationId == null && existingbooking.BookingStatus == BookingStatus.Approved)
                    {
                        existingbooking.VehicleTripRegistrationId = refCodeDetail.VehicleTripRegistrationId;
                        existingbooking.SeatNumber = (byte)refCodeDetail.RefcodeSeatNumber.GetValueOrDefault();

                        // Post a negative account transaction to update the account summary
                        if (booking.BookingType == BookingTypes.Terminal)
                        {
                            _bookingAcctSvc.UpdateBookingAccount(existingbooking.BookingType, existingbooking.PaymentMethod,
                               existingbooking.BookingReferenceCode, existingbooking.VehicleTripRegistrationId.GetValueOrDefault(), (existingbooking.Amount - existingbooking.Discount).GetValueOrDefault(),
                               TransactionType.Credit);
                            existingbooking.CreatedBy = _serviceHelper.GetCurrentUserEmail();
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public Task<RemainingSeatDTO> GetseatsAvailable(Guid vehicletripregistrationId)
        {
            var bookedSeats = _seatManagemntSvc.GetAll().Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId)
                             .Select(s => (int)s.SeatNumber);

            List<Tuple<int, int, bool>> seatBooking = new List<Tuple<int, int, bool>>();

            var vehicletrip = _vehicleTripRegSvc.GetAll().Where(s => s.Id == vehicletripregistrationId);

            var terminalCashBookings = _seatManagemntSvc.GetAll()
                    .Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.Cash);

            var allBookings = _seatManagemntSvc.GetAll().Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId);

            var excludedSeats = _excludedSeatRepo.GetAll().Select(s => s.SeatNumber);

            List<int> bookingTypes = new List<int>();
            List<int> seatsBooking = new List<int>();
            List<bool> ticketPrintStatus = new List<bool>();
            List<int> remainingSeat = new List<int>();
            Dictionary<int, int> bookedSeat = new Dictionary<int, int>();

            foreach (var booking in allBookings)
            {
                bookingTypes.Add((int)booking.BookingType);
                ticketPrintStatus.Add(booking.IsPrinted);
                seatsBooking.Add(booking.SeatNumber);
                bookedSeat.Add(booking.SeatNumber, booking.SeatNumber);

            }
            if (vehicletrip.FirstOrDefault() != null)
            {
                for (int j = 1; j <= vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                {
                    if (!bookedSeat.ContainsKey(j))
                    {
                        remainingSeat.Add(j);
                    }
                }

            }


            var seatRemaining = new RemainingSeatDTO
            {
                VehicleTripRegistrationId = vehicletripregistrationId,
                RemainingSeat = remainingSeat
            };

            return Task.FromResult(seatRemaining);
        }

        public Task<RemainingSeatDTO> GetAvailableseats(Guid vehicletripregistrationId)
        {
            var bookedSeats = _seatManagemntSvc.GetAll().
                Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId)
                             .Select(s => (int)s.SeatNumber);

            List<Tuple<int, int, bool>> seatBooking = new List<Tuple<int, int, bool>>();


            var vehicletrip = _vehicleTripRegSvc.GetAll().Where(s => s.Id == vehicletripregistrationId);

            var terminalCashBookings = _seatManagemntSvc.GetAll()
                     .Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId && s.BookingType == BookingTypes.Terminal && s.PaymentMethod == PaymentMethod.Cash);

            var allBookings = _seatManagemntSvc.GetAll().Where(s => s.VehicleTripRegistrationId == vehicletripregistrationId);
            var excludedSeats = _excludedSeatRepo.GetAll().Select(s => s.SeatNumber);

            List<int> bookingTypes = new List<int>();
            List<int> seatsBooking = new List<int>();
            List<bool> ticketPrintStatus = new List<bool>();
            List<int> remainingSeat = new List<int>();
            Dictionary<int, int> bookedSeat = new Dictionary<int, int>();

            foreach (var booking in allBookings)
            {
                bookingTypes.Add((int)booking.BookingType);
                ticketPrintStatus.Add(booking.IsPrinted);
                seatsBooking.Add(booking.SeatNumber);
                bookedSeat.Add(booking.SeatNumber, booking.SeatNumber);
            }

            if (vehicletrip.FirstOrDefault() != null)
            {
                for (int j = 1; j <= vehicletrip.FirstOrDefault().VehicleModel.NumberOfSeats; j++)
                {
                    if (!bookedSeat.ContainsKey(j))
                    {
                        remainingSeat.Add(j);
                    }
                }
            }

            foreach (var seat in excludedSeats)
            {
                if (remainingSeat.Contains(seat))
                {
                    remainingSeat.Remove(seat);
                }
            }

            var seatRemaining = new RemainingSeatDTO
            {
                VehicleTripRegistrationId = vehicletripregistrationId,
                RemainingSeat = remainingSeat
            };

            return Task.FromResult(seatRemaining);
        }

        public async Task<decimal?> GetMainTripFare(Guid vehicleTripRegistrationId)
        {
            var vehicleTrip = await _vehicleTripRegSvc.GetAsync(vehicleTripRegistrationId);

            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare =
                await _fareSvc.GetFareByVehicleTrip(
                    vehicleTrip.Trip.RouteId, vehicleTrip.VehicleModelId.GetValueOrDefault());
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

            var vehicleTrip = await _vehicleTripRegSvc.GetAsync(vehicleTripId);
            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }
            if (subrouteId != 0)
            {
                var subRoute = await _subRouteRepo.GetAsync(subrouteId.GetValueOrDefault());
                var tripFare =
              await _fareSvc.GetFareByVehicleTrip(
                  subRoute.NameId, vehicleTrip.VehicleModelId.GetValueOrDefault());
                childDisc = (decimal)tripFare.ChildrenDiscountPercentage.GetValueOrDefault() / 100;
                return passengerType == PassengerType.Adult ? tripFare.Amount : tripFare.Amount - tripFare.Amount * childDisc;
            }

            var fare =
                         await _fareSvc.GetFareByVehicleTrip(
                             vehicleTrip.Trip.RouteId, vehicleTrip.VehicleModelId.GetValueOrDefault());
            childDisc = (decimal)fare.ChildrenDiscountPercentage.GetValueOrDefault() / 100;
            return passengerType == PassengerType.Adult ? fare.Amount : fare.Amount - fare.Amount * childDisc;
        }

        public async Task<decimal?> GetTripFare(int subrouteId, Guid vehicleTripRegistrationId)
        {
            var vehicleTrip = await _vehicleTripRegSvc.GetAsync(vehicleTripRegistrationId);

            var subRoute = await _subRouteRepo.GetAsync(subrouteId);

            if (vehicleTrip == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLETRIP_NOT_EXIST);
            }

            var tripFare =
                await _fareSvc.GetFareByVehicleTrip(
                    subRoute.NameId, vehicleTrip.VehicleModelId.GetValueOrDefault());

            return tripFare?.Amount;
        }

        public async Task<decimal?> GetNewRouteFare(int routeId, int modelId)
        {
            var routeFare = await _fareSvc.GetFareByVehicleTrip(routeId, modelId);

            if (routeFare == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }
            return routeFare?.Amount;
        }

        public async Task<decimal?> GetNonIdAmount(int routeId, int modelId)
        {
            var routeFare = await _fareSvc.GetFareByVehicleTrip(routeId, modelId);

            if (routeFare == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }
            return routeFare?.NonIdAmount;
        }

        public async Task<List<SeatManagementDTO>> GetAllBookingDetails(string refCode)
        {
            var vehicleTrip = new VehicleTripRegistrationDTO();
            var seats = await _seatManagemntSvc.GetAllSeatByRefcodeAsync(refCode);
            if (seats != null)
            {
                foreach (var seat in seats)
                {
                    if (seat.VehicleTripRegistrationId != null)
                    {
                        vehicleTrip = await _vehicleTripRegSvc.GetVehicleTripRegistrationDTO(new Guid(seat.VehicleTripRegistrationId.ToString()));
                        var trip = await _tripSvc.GetAsync(vehicleTrip.TripId);
                        seat.DepartureTime = trip.DepartureTime;
                    }
                }
            }

            return seats;
        }

        public async Task<GroupedTripsDetailDTO> GetAvailableTripDetails(VehicleTripRouteSearchDTO search)
        {
            if (search.TripType == Return && !search.ReturnDate.HasValue)
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.BOOKING_INVALID_RETURN_DATE);

            var departures = await GetAvailableTripDetails(search.DepartureTerminalId, search.DestinationTerminalId, search.DepartureDate);

            var arrivals = new List<AvailableTripDetailDTO>();

            if (search.TripType == Return)
            {
                arrivals = await GetAvailableTripDetails(search.DestinationTerminalId, search.DepartureTerminalId, search.ReturnDate.Value, true);
            }

            return new GroupedTripsDetailDTO
            {
                TripType = search.TripType,
                Departures = departures,
                Arrivals = arrivals
            };
        }

        public async Task<Route> GetParentRouteDetails(int departureTerminalId, int destinationTerminalId)
        {
            var ParentRoute = await _routeSvc.SingleOrDefaultAsync(route => route.DepartureTerminalId == departureTerminalId && route.DestinationTerminalId == destinationTerminalId);
            return ParentRoute;
        }

        private async Task<List<AvailableTripDetailDTO>> GetAvailableTripDetails(int departureTerminalId, int destinationTerminalId, DateTime departureDate, bool isSubReturn = false)
        {

            var GetParentIfExist = await GetParentRouteDetails(departureTerminalId, destinationTerminalId);

            var bookingType = BookingTypes.Terminal;
            var discounts = new DiscountDTO();
            //{
            //    MemberDiscount = 0,
            //    MinorDiscount = 0,
            //    AdultDiscount = 0,
            //    ReturnDiscount = 0,
            //    PromoDiscount = 0,
            //    AppDiscountWeb = 0,
            //    AppReturnDiscountWeb = 0,
            //    AppDiscountAndroid = 0,
            //    AppReturnDiscountAndroid = 0,
            //    AppDiscountIos = 0,
            //    AppReturnDiscountIos = 0
            //};


            var username = _serviceHelper.GetCurrentUserEmail();

            if (username == CoreConstants.AndroidBookingAccount || username == CoreConstants.IosBookingAccount
                || username == CoreConstants.WebBookingAccount)
            {
                bookingType = BookingTypes.Online;
            }

            //
            DiscountDTO discount = null;
            if ((discount = await _discountSvc.GetDiscountByBookingTypeAsync(bookingType)) != null)
            {
                discounts = discount;
            }
            var term = await _terminalRepo.GetAsync(destinationTerminalId);

            DateTime campDate = !string.IsNullOrWhiteSpace(bookingConfig.CampTripEndDate) ? DateTime.Parse(bookingConfig.CampTripEndDate) : DateTime.Parse("2018-04-19 23:59");
            if (term != null && term.Name.Contains("Camp") && (departureDate > campDate || departureDate < campDate.AddDays(-2)))
            {
                return new List<AvailableTripDetailDTO>();
            }

            if (departureDate <= Clock.Now || departureDate > Clock.Now.AddDays(21))
            {
                return new List<AvailableTripDetailDTO>();
            }
            //Create gets trips  for the original route
            var trips = await GetAvailableRouteTripsAsync(departureTerminalId, destinationTerminalId);


            var adjustedTrips = new List<Trip>(); // The trips with parent adjustments


            /// For all trips that have a parent
            foreach (var trip in trips.Where(t => t.ParentTripId != null))
            {
                // Get the parent
                var parentTrip = await _tripSvc.GetAsync(trip.ParentTripId.GetValueOrDefault(), x => x.Route);
                // If the parent actually exists, add it up to adjustedTrips
                if (parentTrip != null)
                    adjustedTrips.Add(parentTrip);
            }

            // now add the normal trips to the adjusted list
            adjustedTrips.AddRange(trips.Where(t => t.ParentTripId == null));



            var originalTrips = await GetAvailableRouteTripsAsync(departureTerminalId, destinationTerminalId);
            var parentAvailableTripsArray = new List<List<AvailableTripDetailDTO>>();
            var parents = new List<Route>();


            if (trips == null || trips.Count == 0)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TRIP_NO_AVAILABLE_BUSES);
            }


            var virtualRegistrations = new List<VehicleTripRegistration>();


            foreach (var trip in adjustedTrips)
            {
                if (!await _vehicleTripRegSvc.ExistAsync(e => e.TripId == trip.Id && e.DepartureDate == departureDate))
                {


                    virtualRegistrations.Add(new VehicleTripRegistration
                    {
                        DepartureDate = departureDate,
                        IsVirtualBus = true,
                        IsBusFull = false,
                        TripId = trip.Id,
                        VehicleModelId = trip.VehicleModelId
                    });
                }
            }

            virtualRegistrations.ForEach(x => _vehicleTripRegSvc.Add(x));

            await _unitOfWork.SaveChangesAsync();


            var availableBuses = new List<AvailableTripDetailDTO>();

            foreach (var routeTrips in adjustedTrips.GroupBy(t => t.Route))
            {

                var buses = await GetAvailableTripsAsync(routeTrips.Key.DepartureTerminalId, routeTrips.Key.DestinationTerminalId, departureDate);
                if (buses != null && buses.Any())
                {

                    if (routeTrips.Key.DepartureTerminalId != departureTerminalId || routeTrips.Key.DestinationTerminalId != destinationTerminalId)
                    {
                        // for trips that are parent trips, they need some further adjustments
                        foreach (var bus in buses)
                        {
                            var originalTrip = trips.FirstOrDefault(t => t.ParentTripId == bus.TripId);

                            if (originalTrip == null)
                                continue; // If this guy is not part of the original set, forget it.

                            var price = GetFarePriceByForSubRoute(originalTrip.RouteId, originalTrip.VehicleModelId)?.Amount;

                            bus.RouteName = originalTrip?.Route?.Name; //controlBus.RouteName;
                            bus.FarePrice = price.GetValueOrDefault(); // controlBus.FarePrice;
                            bus.ParentRouteId = bus.RouteId;
                            bus.RouteId = originalTrip.RouteId; // controlBus.RouteId;
                            bus.IsSub = true;

                            if (isSubReturn == true)
                            {
                                bus.IsSubReturn = true;
                                bus.RouteIdReturn = originalTrip.RouteId; // controlBus.RouteId;
                            }


                            if (availableBuses.Any(b => b.RouteId == bus.RouteId && b.TripId == bus.TripId && b.DepartureDate == bus.DepartureDate && b.DepartureTime == bus.DepartureTime) == false)


                                availableBuses.Add(bus);
                        }
                    }
                    else
                    {
                        // For regular trips just add the buses 
                        foreach (var bus in buses)
                        {
                            if (availableBuses.Any(b => b.RouteId == bus.RouteId && b.TripId == bus.TripId && b.DepartureDate == bus.DepartureDate && b.DepartureTime == bus.DepartureTime) == false)
                            {
                                if (isSubReturn == true)
                                {
                                    bus.IsSubReturn = true;
                                    bus.RouteIdReturn = bus.RouteId; // controlBus.RouteId;
                                }
                                availableBuses.Add(bus);
                            }
                        }
                    }
                }
            }

            var tripListCopy = new List<AvailableTripDetailDTO>();

            tripListCopy = availableBuses;

            foreach (var trip in tripListCopy)
            {

                bool trippckup = (await _pickupPointSvc.GetTripPickupPoints(trip.TripId)).Any();

                trip.HasPickup = trippckup;
                trip.AdultFare = trip.FarePrice - discounts.AdultDiscount * trip.FarePrice / 100;
                trip.MemberFare = trip.FarePrice - discounts.MemberDiscount * trip.FarePrice / 100;
                trip.ChildFare = trip.FarePrice - discounts.MinorDiscount * trip.FarePrice / 100;
                trip.ReturnFare = trip.FarePrice - discounts.ReturnDiscount * trip.FarePrice / 100;
                trip.PromoFare = trip.FarePrice - discounts.PromoDiscount * trip.FarePrice / 100;
                if (username == CoreConstants.AndroidBookingAccount)
                {
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountAndroid * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountAndroid * trip.FarePrice / 100;
                }
                else if (username == CoreConstants.IosBookingAccount)
                {
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountIos * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountIos * trip.FarePrice / 100;
                }
                else if (username == CoreConstants.WebBookingAccount)
                {
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountWeb * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountWeb * trip.FarePrice / 100;
                }
            }

            List<AvailableTripDetailDTO> availableTrip = new List<AvailableTripDetailDTO>();
            foreach (var trip in tripListCopy)
            {
                var weekday = (int)departureDate.DayOfWeek;
                var routeId = trip.ParentRouteId ?? trip.RouteId;

                //segzy
                //var fareCalendar = await _fareCalendarSvc.GetFareCalendaByRoutesAsync(trip.RouteId, departureDate);

                //var fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);


                //---------fare calendar start

                var fareCalendar = new FareCalendarDTO();
                fareCalendar = null;
                var farecalendarList = new List<FareCalendarDTO>();

                farecalendarList = await _fareCalendarSvc.GetFareCalendaListByRoutesAsync(trip.RouteId, trip.DepartureDate);
                foreach (var calendar in farecalendarList)
                {
                    if (calendar.VehicleModelId != null)
                    {
                        if (trip.VehicleModelId == calendar.VehicleModelId)
                        {
                            fareCalendar = calendar;

                        }
                        break;
                    }

                    fareCalendar = calendar;
                }
                if (fareCalendar == null)
                {
                    var DepartureterminalId = await _routeSvc.GetDepartureterminalIdFromRoute(trip.RouteId);
                    farecalendarList = await _fareCalendarSvc.GetFareCalendaListByTerminalsAsync(DepartureterminalId, trip.DepartureDate);
                    foreach (var calendar in farecalendarList)
                    {
                        if (calendar.VehicleModelId != null)
                        {
                            if (trip.VehicleModelId == calendar.VehicleModelId)
                            {
                                fareCalendar = calendar;

                            }
                            break;
                        }

                        fareCalendar = calendar;
                    }
                }


                decimal fareDifference = 0;
                decimal fareDifferenceAdultFare = 0;
                decimal fareDifferenceAppFare = 0;
                decimal fareDifferenceAppReturnFare = 0;
                decimal fareDifferenceMemberFare = 0;
                decimal fareDifferencePromoFare = 0;
                decimal fareDifferenceReturnFare = 0;
                decimal fareDifferenceChildFare = 0;
                if (fareCalendar != null)
                {


                    if (fareCalendar.FareAdjustmentType == FareAdjustmentType.Percentage)
                    {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.FarePrice / 100);
                        fareDifferenceAdultFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.AdultFare / 100);

                        fareDifferenceAppFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.AppFare / 100);

                        fareDifferenceAppReturnFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.AppReturnFare / 100);

                        fareDifferenceMemberFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.MemberFare / 100);

                        fareDifferencePromoFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.PromoFare / 100);

                        fareDifferenceReturnFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.ReturnFare / 100);

                        fareDifferenceChildFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * trip.ChildFare / 100);

                    }
                    else
                    {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);
                        fareDifferenceAdultFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferenceAppFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferenceAppReturnFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferenceMemberFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferencePromoFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferenceReturnFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                        fareDifferenceChildFare = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                    }
                }


                //---------fare calendar end






                trip.FarePrice += fareDifference;
                trip.AdultFare += fareDifferenceAdultFare;
                trip.AppFare += fareDifferenceAppFare;
                trip.AppReturnFare += fareDifferenceAppReturnFare;
                trip.MemberFare += fareDifferenceMemberFare;
                trip.PromoFare += fareDifferencePromoFare;
                trip.ReturnFare += fareDifferenceReturnFare;
                trip.ChildFare += fareDifferenceChildFare;


                var TripsAvailable = _tripAvailabilitySvc.GetAvailableTripsForRoute(routeId, weekday);

                ///// Add FareCalendar to Search
                if (TripsAvailable.Any())
                {
                    if (TripsAvailable.Contains(trip.TripId))
                    {
                        availableTrip.Add(trip);
                    }
                }
                else
                {
                    availableTrip.Add(trip);
                }
            }
            return availableTrip.OrderBy(x => x.DepartureTime).ToList();
        }

        private async Task<List<AvailableTripDetailDTO>> GetAvailableTripsWIthoutExcludedAsync(int departureTerminalId, int destinationTerminalId, DateTime departureDate)
        {
            var date = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);

            var excludedSeats = _excludedSeatRepo.GetAll().Select(s => (int)s.SeatNumber);
            var numberOfExcludedSeats = await excludedSeats.CountAsync();
            var result =
                 from route in GetRoutesMatching(departureTerminalId, destinationTerminalId)
                 join trip in _tripSvc.GetAll() on route.Id equals trip.RouteId
                 join registration in _vehicleTripRegSvc.GetAll() on trip.Id equals registration.TripId
                 join vehicleModel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehicleModel.Id
                 join vehicleMake in _vehicleMakeRepo.GetAll() on vehicleModel.VehicleMakeId equals vehicleMake.Id
                 join fare in _fareSvc.GetAll() on new { route.Id, VehicleModelId = vehicleModel.Id } equals new { Id = fare.RouteId, fare.VehicleModelId }
                 let bookedSeats = _seatManagemntSvc.GetAll()
                           .Where(s => s.VehicleTripRegistrationId == registration.Id)
                           .Select(s => (int)s.SeatNumber)
                 let remainingAvailableSeats = vehicleModel.NumberOfSeats - (numberOfExcludedSeats + bookedSeats.Count())
                 where
                 registration.IsVirtualBus && !registration.IsBusFull &&
                 registration.DepartureDate.Date == date.Date
                 &&
                 remainingAvailableSeats >= 0 && trip.AvailableOnline

                 select new AvailableTripDetailDTO
                 {
                     TripId = trip.Id,
                     VehicleTripRegistrationId = registration.Id,
                     RouteName = route.Name,
                     VehicleName = vehicleMake.Name + " (" + vehicleModel.Name + ")",
                     DepartureTime = trip.DepartureTime,

                     FarePrice = fare.Amount,
                     AvailableNumberOfSeats = remainingAvailableSeats,
                     BookedSeats = bookedSeats,
                     DepartureDate = date,
                     DateCreated = trip.CreationTime,
                     TotalNumberOfSeats = vehicleModel.NumberOfSeats,
                     ExcludedSeats = excludedSeats.Union(bookedSeats),
                     VehicleModelId = (decimal)trip.VehicleModelId,
                     VehicleModel = trip.VehicleModel.Name,
                     RouteId = route.Id
                 };

            return await result.ToListAsync();
        }

        public async Task<List<AvailableTripDetailDTO>> GetAvailableTripsAsync(int departureTerminalId, int destinationTerminalId, DateTime departureDate)
        {
            var availableTrips = await GetAvailableTripsWIthoutExcludedAsync(departureTerminalId, destinationTerminalId, departureDate);

            IEnumerable<int> SiennaIncludedSeats = new List<int>() { 3 };
            foreach (var trip in availableTrips)
            {
                if (trip.TotalNumberOfSeats == 17 || trip.TotalNumberOfSeats == 16)
                {
                    trip.ExcludedSeats = trip.BookedSeats;
                    trip.AvailableNumberOfSeats = trip.TotalNumberOfSeats - trip.BookedSeats.Count();

                }
                else if (trip.TotalNumberOfSeats == 12 && trip.VehicleModel == "Hiace X")
                {
                    trip.ExcludedSeats = trip.BookedSeats;
                    trip.AvailableNumberOfSeats = trip.TotalNumberOfSeats - trip.BookedSeats.Count();
                }
                else if (trip.TotalNumberOfSeats == 13)
                {
                    trip.ExcludedSeats = trip.BookedSeats;
                    trip.AvailableNumberOfSeats = trip.TotalNumberOfSeats - trip.BookedSeats.Count();
                }

                //temporary means of adding seat 3 for Sienna only, ifits included in the exludedseats in the db
                else if (trip.TotalNumberOfSeats == 6 && !trip.BookedSeats.Contains(3))
                {
                    trip.ExcludedSeats = trip.ExcludedSeats.Except(SiennaIncludedSeats);
                    trip.AvailableNumberOfSeats = trip.TotalNumberOfSeats - trip.BookedSeats.Count();
                }
            }

            return availableTrips.OrderBy(d =>d.DepartureTime).ToList();
        }

        public SeatManagementDTO GetFarePriceByForSubRoute(int? routeId, int? vehicleModelId)
        {
            var departureDate = Clock.Now.Date;

            var fare = _fareSvc.FirstOrDefault(s => s.RouteId == routeId && s.VehicleModelId == vehicleModelId);

            return new SeatManagementDTO()
            {
                Amount = fare.Amount,
                RouteId = fare.RouteId
            };
        }

        private IQueryable<Route> GetRoutesMatching(int departureTerminalId, int destinationTerminalId)
        {
            return
                from route in _routeSvc.GetAll()
                where
                route.DepartureTerminalId == departureTerminalId && route.DestinationTerminalId == destinationTerminalId
                select route;
        }

        public Task<List<Trip>> GetAvailableRouteTripsAsync(int departureTerminalId, int destinationTerminalId)
        {
            var trips =
                from route in GetRoutesMatching(departureTerminalId, destinationTerminalId)
                join trip in _tripSvc.GetAll() on route.Id equals trip.RouteId
                join fare in _fareSvc.GetAll() on new { trip.RouteId, VehicleModelId = (int)trip.VehicleModelId } equals new { fare.RouteId, fare.VehicleModelId }
                where trip.AvailableOnline
                select trip;

            return trips.ToListAsync();
        }

        public async Task<GroupedTripsDetailDTO> GetAvailableTripTerminalDetails(VehicleTripRouteSearchDTO tripBookingSearch)
        {
            if (tripBookingSearch.TripType == Return && !tripBookingSearch.ReturnDate.HasValue)
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.BOOKING_INVALID_RETURN_DATE);

            var departures =
                await GetAvailableTripForTerminalDetails(tripBookingSearch.DepartureTerminalId, tripBookingSearch.DestinationTerminalId, tripBookingSearch.DepartureDate);

            var arrivals = new List<AvailableTripDetailDTO>();

            if (tripBookingSearch.TripType == Return)
            {
                arrivals = await GetAvailableTripForTerminalDetails(tripBookingSearch.DestinationTerminalId, tripBookingSearch.DepartureTerminalId, tripBookingSearch.ReturnDate.Value);
            }

            return new GroupedTripsDetailDTO
            {
                TripType = tripBookingSearch.TripType,
                Departures = departures,
                Arrivals = arrivals
            };
        }

        private async Task<List<AvailableTripDetailDTO>> GetAvailableTripForTerminalDetails(int departureTerminalId, int destinationTerminalId, DateTime departureDate)
        {

            BookingTypes bookingType = BookingTypes.Terminal;
            DiscountDTO discounts = new DiscountDTO()
            {
                MemberDiscount = 0,
                MinorDiscount = 0,
                AdultDiscount = 0,
                ReturnDiscount = 0,
                PromoDiscount = 0,
                AppDiscountWeb = 0,
                AppReturnDiscountWeb = 0,
                AppDiscountAndroid = 0,
                AppReturnDiscountAndroid = 0,
                AppDiscountIos = 0,
                AppReturnDiscountIos = 0
            };
            var username = _serviceHelper.GetCurrentUserEmail();
            if (username == CoreConstants.IosBookingAccount || username == CoreConstants.WebBookingAccount
                || username == CoreConstants.AndroidBookingAccount)
            {
                bookingType = BookingTypes.Online;
            }
            //
            DiscountDTO discount = null;
            if ((discount = await _discountSvc.GetDiscountByBookingTypeAsync(bookingType)) != null)
            {
                discounts = discount;
            }
            if (departureDate <= DateTime.Now || departureDate > DateTime.Now.AddDays(21))
            {
                return new List<AvailableTripDetailDTO>();
            }

            var trips = await GetAvailableRouteTripsForTerminalAsync(departureTerminalId, destinationTerminalId);

            if (trips == null || trips.Count == 0)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TRIP_NO_AVAILABLE_BUSES);
            }

            var virtualRegistrations = new List<VehicleTripRegistration>(trips.Count);

            foreach (var trip in trips)
            {
                if (!await _vehicleTripRegSvc.ExistAsync(e => e.TripId == trip.Id && e.DepartureDate == departureDate))
                {

                    virtualRegistrations.Add(new VehicleTripRegistration
                    {
                        DepartureDate = departureDate,
                        IsVirtualBus = true,
                        IsBusFull = false,
                        TripId = trip.Id,
                        CreatorUserId = _serviceHelper.GetCurrentUserId(),
                        VehicleModelId = trip.VehicleModelId
                    });
                }
            }

            virtualRegistrations.ForEach(item => _vehicleTripRegSvc.Add(item));

            await _unitOfWork.SaveChangesAsync();

            var tripList = await GetAvailableTripsAsync(departureTerminalId, destinationTerminalId, departureDate);
            foreach (var trip in tripList)
            {
                trip.AdultFare = trip.FarePrice - discounts.AdultDiscount * trip.FarePrice / 100;
                trip.MemberFare = trip.FarePrice - discounts.MemberDiscount * trip.FarePrice / 100;
                trip.ChildFare = trip.FarePrice - discounts.MinorDiscount * trip.FarePrice / 100;
                trip.ReturnFare = trip.FarePrice - discounts.ReturnDiscount * trip.FarePrice / 100;
                trip.PromoFare = trip.FarePrice - discounts.PromoDiscount * trip.FarePrice / 100;
                if (username == CoreConstants.AndroidBookingAccount)
                {
                    ///hkhk
                    ///
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountAndroid * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountAndroid * trip.FarePrice / 100;
                }
                else if (username == CoreConstants.IosBookingAccount)
                {
                    ///oikjui
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountIos * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountIos * trip.FarePrice / 100;
                }
                else if (username == CoreConstants.WebBookingAccount)
                {
                    trip.AppFare = trip.FarePrice - discounts.AppDiscountWeb * trip.FarePrice / 100;
                    trip.AppReturnFare = trip.FarePrice - discounts.AppReturnDiscountWeb * trip.FarePrice / 100;
                }
            }
            return tripList.OrderBy(x =>x.DepartureTime).ToList();
        }
                
        public Task<List<Trip>> GetAvailableRouteTripsForTerminalAsync(int departureTerminalId, int destinationTerminalId)
        {
            var trips =
                from route in GetRoutesMatching(departureTerminalId, destinationTerminalId)
                join trip in _tripSvc.GetAll() on route.Id equals trip.RouteId
                join fare in _fareSvc.GetAll() on new { trip.RouteId, VehicleModelId = (int)trip.VehicleModelId } equals new { fare.RouteId, fare.VehicleModelId }

                select trip;

            return trips.ToListAsync();
        }

        public async Task<BookingResponseDTO> PostBooking(BookingDetailDTO bookingdetail)
        {
            decimal totalAmount = 0;
            string tovehicleModel, returnVehicleModel = string.Empty;
            var subRouteNameReturn = string.Empty;

            var subRouteName = await _routeSvc.GetParentRouteNameAsync(bookingdetail.RouteId);

            if (bookingdetail.RouteIdReturn != null || bookingdetail.RouteIdReturn > 0)
            {
                subRouteNameReturn = await _routeSvc.GetParentRouteNameAsync(bookingdetail.RouteIdReturn);
            }

           


            // Check if seat registrations is null
            if (bookingdetail?.SeatRegistrations != null)
            {
                var returnVehicletrip = new Guid();
                List<string> tripList = new List<string>();
                List<int> seatTo = new List<int>();
                List<int> seatReturn = new List<int>();
                var bookingResponse = new BookingResponseDTO();


                // separates seat registrations into to and return trips
                foreach (var trip in bookingdetail.SeatRegistrations.Split(';'))
                {
                    tripList.Add(trip);
                }

                //gets to trip details
                var toTripDetail = tripList.FirstOrDefault().Split(':');
                var vehicletripTo = Guid.Parse(toTripDetail[0]);
                var Tovehicletrip = await _vehicleTripRegSvc.GetAsync(vehicletripTo);
                VehicleTripRegistration returnvehicletrip = null;
                tovehicleModel = Tovehicletrip.VehicleModel.Name;
                foreach (var seat in toTripDetail[1].Split(','))
                {
                    seatTo.Add(Convert.ToInt32(seat));
                }

                //gets return trip details
                if (tripList.Count > 1)
                {
                    var returnTrip = tripList.LastOrDefault().Split(':');
                    returnVehicletrip = Guid.Parse(returnTrip[0]);
                    returnvehicletrip = await _vehicleTripRegSvc.GetAsync(returnVehicletrip);
                    returnVehicleModel = returnvehicletrip.VehicleModel.Name;
                    foreach (var seat in returnTrip[1].Split(','))
                    {
                        seatReturn.Add(Convert.ToInt32(seat));
                    }
                }

                var mainBooker = new SeatManagement();

                string returnBookingRef = null;
                ////gets fare prices for both return and to trips
                //var farePrice1 = GetFarePriceByVehicleTripId(vehicletripTo);
                //var farePrice = GetFarePriceByVehicleTripId(returnVehicletrip);


                //Checks for seat availability
                foreach (int i in seatTo)
                {
                    var seat = new SeatManagement
                    {
                        SeatNumber = (byte)i,
                        VehicleTripRegistrationId = vehicletripTo
                    };

                    var isavailable = await CheckAvailability(seat, bookingdetail.BookingType);
                    if (!isavailable)
                    {
                        throw await _serviceHelper.GetExceptionAsync("Seat(s) no Longer Available");
                    }
                }

                //creates refcode
                string terminalCode = bookingConfig.TerminalKey;
                string refCodes = await _seatManagemntSvc.GetRefCode();
                string refCode = string.Empty;
                var termCode = await _routeSvc.GetDepartureCodeFromRoute(bookingdetail.RouteId);
                if (bookingdetail.BookingType == BookingTypes.Online)
                {
                    refCode = refCodes + "O" + termCode;
                }
                else if (bookingdetail.BookingType == BookingTypes.Advanced)
                {
                    refCode = refCodes + "A" + termCode;
                }

                else if (bookingdetail.BookingType == BookingTypes.Terminal)
                {
                    refCode = refCodes + "T" + termCode;
                }

                //separates beneficiary seats from main seat
                var allSeat = new ArrayList();
                var beneficarySeat = new ArrayList();
                foreach (int i in seatTo)
                {
                    allSeat.Add(i);
                }
                allSeat.RemoveAt(0);
                beneficarySeat = allSeat;

                int y = 0;

                //Saves Beneficiary Seats

                if (bookingdetail.PaymentMethod != PaymentMethod.Cash && bookingdetail.PaymentMethod != PaymentMethod.Pos && bookingdetail.PaymentMethod != PaymentMethod.CashAndPos)
                {
                    bookingdetail.BookingStatus = BookingStatus.Pending;
                }
                if (bookingdetail != null && bookingdetail.Beneficiaries.Any())
                {
                    foreach (var ben in bookingdetail.Beneficiaries)
                    {
                        ben.SeatNumber = (int)beneficarySeat[y];
                        y++;
                        if (bookingdetail.BookingType != BookingTypes.Terminal)
                        {
                            var Amount = await GetAmountByBookingDetails(vehicletripTo, bookingdetail.BookingType, ben.PassengerType, bookingdetail.IsLoggedIn, bookingdetail.IsSub, bookingdetail.RouteId, Tovehicletrip.VehicleModelId, bookingdetail.HasCoupon, bookingdetail.CouponCode, bookingdetail.PhoneNumber);
                            if (bookingdetail.PassportType != null)
                            {
                                var passname = await _passportTypeService.GetPassportTypeByPassportTypeName(bookingdetail.PassportType);
                                var Idfare = await _passportTypeService.GetPassportTypeByRouteAndId(Convert.ToInt32(passname.Id), 2);
                                bookingdetail.Amount = Convert.ToDecimal(Amount) + Convert.ToDecimal(Idfare.AddOnFare);
                            }
                            else
                            {
                                bookingdetail.Amount = Amount;

                            }

                        }
                        (string DepartureTerminal, string DestinationTerminal) = (await _routeSvc.GetDepartureCodeFromRoute(bookingdetail.RouteId), await _routeSvc.GetDestinationTerminalCodeFromRoute(bookingdetail.RouteId));

                        var rfcode = refCode + "-B" + y;
                        _seatManagemntSvc.Add(new SeatManagement
                        {
                            SeatNumber = (byte)ben.SeatNumber,
                            VehicleTripRegistrationId = vehicletripTo,
                            FullName = ben.FullName,
                            Gender = ben.Gender,
                            HasCoupon = bookingdetail.HasCoupon,
                            CouponCode = bookingdetail.CouponCode,
                            NextOfKinName = bookingdetail.NextOfKinName,
                            NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone,
                            PhoneNumber = bookingdetail.PhoneNumber,
                            BookingReferenceCode = rfcode,
                            Amount = Math.Round(bookingdetail.Amount.GetValueOrDefault()),
                            VehicleModelId = Tovehicletrip.VehicleModelId,
                            RouteId = bookingdetail.RouteId,
                            CreatorUserId = _serviceHelper.GetCurrentUserId(),
                            CreatedBy = _serviceHelper.GetCurrentUserEmail(),
                            Discount = bookingdetail.Discount.GetValueOrDefault(),
                            PassengerType = ben.PassengerType,
                            PaymentMethod = bookingdetail.PaymentMethod,
                            BookingType = bookingdetail.BookingType,
                            IsMainBooker = false,
                            IsReturn = false,
                            HasReturn = tripList.Count > 1,
                            MainBookerReferenceCode = refCode,
                            BookingStatus = bookingdetail.BookingStatus,
                            NoOfTicket = 0,
                            PickUpPointId = bookingdetail.PickUpId,
                            PickupStatus = bookingdetail.PickupStatus,
                            IsSub = bookingdetail.IsSub,
                            OnlineSubRouteName = subRouteName.ToString(),
                            LuggageType = bookingdetail.LuggageType
                        });

                        totalAmount += Math.Round(bookingdetail.Amount.GetValueOrDefault());


                        _bookingAcctSvc.UpdateBookingAccount(bookingdetail.BookingType, bookingdetail.PaymentMethod,
                           rfcode, vehicletripTo, Math.Round(bookingdetail.Amount.GetValueOrDefault()));
                    }
                }
                if (bookingdetail != null)
                {
                    if (bookingdetail.BookingType != BookingTypes.Terminal)
                    {

                        var Amount = await GetAmountByBookingDetails(vehicletripTo, bookingdetail.BookingType, bookingdetail.PassengerType, bookingdetail.IsLoggedIn, bookingdetail.IsSub, bookingdetail.RouteId, Tovehicletrip.VehicleModelId, bookingdetail.HasCoupon, bookingdetail.CouponCode, bookingdetail.PhoneNumber, true);
                        //this validate for ops advance booking only
                        if (bookingdetail.PassportType !=null)
                        {
                            var passname = await _passportTypeService.GetPassportTypeByPassportTypeName(bookingdetail.PassportType);
                            var Idfare = await _passportTypeService.GetPassportTypeByRouteAndId( Convert.ToInt32(passname.Id), 2);                        
                            bookingdetail.Amount = Convert.ToDecimal(Amount) + Convert.ToDecimal(Idfare.AddOnFare);
                        }
                        else
                        {
                            bookingdetail.Amount = Amount;
                        }
                      
                    }

                    mainBooker.SeatNumber = (byte)seatTo[0];
                    mainBooker.VehicleTripRegistrationId = vehicletripTo;
                    mainBooker.FullName = bookingdetail.FirstName + ' ' + bookingdetail.LastName;
                    mainBooker.Gender = bookingdetail.Gender;
                    mainBooker.PhoneNumber = bookingdetail.PhoneNumber;
                    mainBooker.HasCoupon = bookingdetail.HasCoupon;
                    mainBooker.CouponCode = bookingdetail.CouponCode;
                    mainBooker.NextOfKinName = bookingdetail.NextOfKinName;
                    mainBooker.NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone;
                    mainBooker.BookingReferenceCode = refCode;
                    mainBooker.VehicleModelId = Tovehicletrip.VehicleModelId;
                    mainBooker.Amount = Math.Round(bookingdetail.Amount.GetValueOrDefault());
                    mainBooker.PartCash = bookingdetail.PartCash;
                    mainBooker.Discount = bookingdetail.Discount.GetValueOrDefault();
                    mainBooker.PassengerType = bookingdetail.PassengerType;
                    mainBooker.PaymentMethod = bookingdetail.PaymentMethod;
                    mainBooker.BookingType = bookingdetail.BookingType;
                    mainBooker.SubRouteId = bookingdetail.SubrouteId;
                    mainBooker.IsMainBooker = true;
                    mainBooker.HasReturn = tripList.Count > 1;
                    mainBooker.IsReturn = false;
                    mainBooker.MainBookerReferenceCode = refCode;
                    mainBooker.PickUpPointId = bookingdetail.PickUpId;
                    mainBooker.PickupStatus = bookingdetail.PickupStatus;
                    mainBooker.BookingStatus = bookingdetail.BookingStatus;
                    mainBooker.NoOfTicket = tripList.Count > 1 ? (bookingdetail.Beneficiaries.Count + 1) * 2 : bookingdetail.Beneficiaries.Count + 1;
                    mainBooker.OnlineSubRouteName = subRouteName;
                    mainBooker.IsSub = bookingdetail.IsSub;
                    mainBooker.RouteId = bookingdetail.RouteId;
                    mainBooker.LuggageType = bookingdetail.LuggageType;
                    mainBooker.CreatorUserId = _serviceHelper.GetCurrentUserId();
                    mainBooker.CreatedBy = _serviceHelper.GetCurrentUserEmail();
                    mainBooker.POSReference = bookingdetail.PosRef;
                    mainBooker.IsGhanaRoute = bookingdetail.IsGhanaRoute;
                    mainBooker.PassportType = bookingdetail.PassportType;
                    mainBooker.PassportId = bookingdetail.PassportId;
                    mainBooker.PlaceOfIssue = bookingdetail.PlaceOfIssue;
                    mainBooker.IssuedDate = bookingdetail.IssuedDate;
                    mainBooker.ExpiredDate = bookingdetail.ExpiredDate;
                    mainBooker.Nationality = bookingdetail.Nationality;

                    _seatManagemntSvc.Add(mainBooker);
                    totalAmount += Math.Round(bookingdetail.Amount.GetValueOrDefault());

                    var accAmt = 0.0m;
                    accAmt = bookingdetail.PartCash.GetValueOrDefault() != 0 ? bookingdetail.PartCash.GetValueOrDefault() : bookingdetail.Amount.GetValueOrDefault();

                    _bookingAcctSvc.UpdateBookingAccount(bookingdetail.BookingType, bookingdetail.PaymentMethod, refCode, vehicletripTo, Math.Round(accAmt));
                }

                if (tripList.Count > 1)
                {
                    foreach (int i in seatReturn)
                    {
                        var returnseat = new SeatManagement()
                        {
                            SeatNumber = (byte)i,
                            VehicleTripRegistrationId = returnVehicletrip
                        };

                        var isavailable = await Getavailableseat(returnseat);
                        if (!isavailable)
                        {
                            throw await _serviceHelper.GetExceptionAsync("Seat(s) no Longer Available");
                        }
                    }

                    ArrayList retunallSeat = new ArrayList();
                    ArrayList returnbeneficarySeat = new ArrayList();
                    foreach (int i in seatReturn)
                    {

                        retunallSeat.Add(i);
                    }

                    retunallSeat.RemoveAt(0);
                    returnbeneficarySeat = retunallSeat;

                    int retTrip = 0;
                    var ReturntermCode = await _routeSvc.GetDestinationTerminalCodeFromRoute(bookingdetail.RouteId);
                    // Saves Beneficiary Seats
                    if (bookingdetail != null && bookingdetail.Beneficiaries.Any())
                    {
                        foreach (var ben in bookingdetail.Beneficiaries)
                        {
                            string rfcode = "";
                            ben.SeatNumber = (int)returnbeneficarySeat[retTrip];
                            retTrip++;

                            //rfcode = refCode + "-BR" + retTrip;

                            if (bookingdetail.BookingType == BookingTypes.Online)
                            {

                                rfcode = refCodes + "O" + ReturntermCode + "-BR" + retTrip;
                            }
                            else if (bookingdetail.BookingType == BookingTypes.Advanced)
                            {

                                rfcode = refCodes + "A" + ReturntermCode + "-BR" + retTrip;

                            }


                            //else if (bookingdetail.BookingType == BookingTypes.BookOnHold) {
                            //    if (tovehicleModel == "Hiace") {
                            //        rfcode = "BH-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (tovehicleModel == "Sprinter") {
                            //        rfcode = "BM-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (tovehicleModel == "Prime") {
                            //        rfcode = "BP-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (tovehicleModel == "Sienna") {
                            //        rfcode = "BS-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (returnVehicleModel == "Hiace X") {
                            //        rfcode = "BX-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (returnVehicleModel == "Jet Mover") {
                            //        rfcode = "BJM-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (returnVehicleModel == "Jet Prime") {
                            //        rfcode = "BJP-" + refCodes + "-BR" + retTrip;
                            //    }
                            //    else if (returnVehicleModel == "Jet Prime XL") {
                            //        rfcode = "BJX-" + refCodes + "-BR" + retTrip;
                            //    }

                            //}

                            else if (bookingdetail.BookingType == BookingTypes.Terminal)
                            {


                                rfcode = refCodes + "T" + ReturntermCode + "-BR" + retTrip;

                            }


                            if (bookingdetail.BookingType != BookingTypes.Terminal)
                            {
                                bookingdetail.Amount = await GetAmountByBookingDetails(returnVehicletrip, bookingdetail.BookingType, ben.PassengerType, bookingdetail.IsLoggedIn, bookingdetail.IsSubReturn, bookingdetail.RouteIdReturn, returnvehicletrip.VehicleModelId, bookingdetail.HasCoupon, bookingdetail.CouponCode);
                            }

                            _seatManagemntSvc.Add(new SeatManagement
                            {
                                CreatedBy = _serviceHelper.GetCurrentUserEmail(),
                                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                                SeatNumber = (byte)ben.SeatNumber,
                                VehicleTripRegistrationId = returnVehicletrip,
                                VehicleModelId = returnvehicletrip.VehicleModelId,
                                FullName = ben.FullName,
                                Gender = ben.Gender,
                                NextOfKinName = bookingdetail.NextOfKinName,
                                NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone,
                                PhoneNumber = bookingdetail.PhoneNumber,
                                HasCoupon = bookingdetail.HasCoupon,
                                CouponCode = bookingdetail.CouponCode,
                                BookingReferenceCode = rfcode,
                                Amount = Math.Round(bookingdetail.Amount.GetValueOrDefault()),
                                Discount = bookingdetail.Discount.GetValueOrDefault(),
                                PassengerType = ben.PassengerType,
                                RouteId = bookingdetail.RouteIdReturn,
                                PaymentMethod = bookingdetail.PaymentMethod,
                                BookingType = bookingdetail.BookingType,
                                IsMainBooker = false,
                                IsReturn = true,
                                HasReturn = false,
                                MainBookerReferenceCode = refCode,
                                BookingStatus = bookingdetail.BookingStatus,
                                NoOfTicket = 0,
                                PickUpPointId = bookingdetail.ReturnPickUpId,
                                PickupStatus = bookingdetail.ReturnPickupStatus,
                                IsSubReturn = bookingdetail.IsSubReturn,
                                OnlineSubRouteName = subRouteNameReturn.ToString(),
                                LuggageType = bookingdetail.LuggageType,
                                IsGhanaRoute = bookingdetail.IsGhanaRoute,
                                PassportType = bookingdetail.PassportType,
                                PassportId = bookingdetail.PassportId,
                                PlaceOfIssue = bookingdetail.PlaceOfIssue,
                                IssuedDate = bookingdetail.IssuedDate,
                                Nationality = bookingdetail.Nationality,
                            });

                            totalAmount += Math.Round(bookingdetail.Amount.GetValueOrDefault());

                            _bookingAcctSvc.UpdateBookingAccount(bookingdetail.BookingType, bookingdetail.PaymentMethod, rfcode, returnVehicletrip, Math.Round(bookingdetail.Amount.GetValueOrDefault()));
                        }
                    }

                    if (bookingdetail != null)
                    {
                        if (bookingdetail.BookingType != BookingTypes.Terminal)
                        {

                            var Amount = await GetAmountByBookingDetails(returnVehicletrip, bookingdetail.BookingType,
                                bookingdetail.PassengerType, bookingdetail.IsLoggedIn, bookingdetail.IsSubReturn,
                                bookingdetail.RouteIdReturn, returnvehicletrip.VehicleModelId, bookingdetail.HasCoupon,
                                bookingdetail.CouponCode);

                            if (bookingdetail.PassportType != null)
                            {
                                var passname = await _passportTypeService.GetPassportTypeByPassportTypeName(bookingdetail.PassportType);
                                var Idfare = await _passportTypeService.GetPassportTypeByRouteAndId(Convert.ToInt32(passname.Id), 2);
                                bookingdetail.Amount = Convert.ToDecimal(Amount) + Convert.ToDecimal(Idfare.AddOnFare);
                            }
                            else
                            {
                                bookingdetail.Amount = Amount;
                            }
                        }
                        var rfcode = "";

                        if (bookingdetail.BookingType == BookingTypes.Online)
                        {


                            rfcode = refCodes + "O" + ReturntermCode + "-R";

                        }
                        else if (bookingdetail.BookingType == BookingTypes.Advanced)
                        {

                            rfcode = refCodes + "A" + ReturntermCode + "-R";

                        }

                        //else if (bookingdetail.BookingType == BookingTypes.BookOnHold) {
                        //    if (returnVehicleModel == "Hiace") {
                        //        rfcode = "BH-" + refCodes + "-R";
                        //    }
                        //    else if (returnVehicleModel == "Sprinter") {
                        //        rfcode = "BM-" + refCodes + "-R";
                        //    }
                        //    else if (returnVehicleModel == "Prime") {
                        //        rfcode = "BP-" + refCodes + "-R";
                        //    }
                        //    else if (returnVehicleModel == "Sienna") {
                        //        rfcode = "BS-" + refCodes + "-R";
                        //    }
                        //    else if (returnVehicleModel == "Hiace X") {
                        //        rfcode = "BX-" + refCodes + "-R";
                        //    }
                        //    else if (returnVehicleModel == "Jet Mover") {
                        //        rfcode = "BJM-" + refCodes + "-R" + retTrip;
                        //    }
                        //    else if (returnVehicleModel == "Jet Prime") {
                        //        rfcode = "BJP-" + refCodes + "-R" + retTrip;
                        //    }
                        //    else if (returnVehicleModel == "Jet Prime XL") {
                        //        rfcode = "BJX-" + refCodes + "-R" + retTrip;
                        //    }
                        //}

                        else if (bookingdetail.BookingType == BookingTypes.Terminal)
                        {
                            rfcode = refCodes + "T" + ReturntermCode + "-R";
                        }

                        _seatManagemntSvc.Add(new SeatManagement
                        {
                            CreatorUserId = _serviceHelper.GetCurrentUserId(),
                            CreatedBy = _serviceHelper.GetCurrentUserEmail(),
                            SeatNumber = (byte)seatReturn[0],
                            VehicleTripRegistrationId = returnVehicletrip,
                            FullName = bookingdetail.FirstName + ' ' + bookingdetail.LastName,
                            Gender = bookingdetail.Gender,
                            NextOfKinName = bookingdetail.NextOfKinName,
                            NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone,
                            PhoneNumber = bookingdetail.PhoneNumber,
                            HasCoupon = bookingdetail.HasCoupon,
                            CouponCode = bookingdetail.CouponCode,
                            BookingReferenceCode = rfcode,
                            PassengerType = bookingdetail.PassengerType,
                            Amount = Math.Round(bookingdetail.Amount.GetValueOrDefault()),
                            Discount = bookingdetail.Discount.GetValueOrDefault(),
                            RouteId = bookingdetail.RouteIdReturn,
                            VehicleModelId = returnvehicletrip.VehicleModelId,
                            PaymentMethod = bookingdetail.PaymentMethod,
                            BookingType = bookingdetail.BookingType,
                            IsReturn = true,
                            MainBookerReferenceCode = refCode,
                            BookingStatus = bookingdetail.BookingStatus,
                            NoOfTicket = 0,
                            PickUpPointId = bookingdetail.ReturnPickUpId,
                            PickupStatus = bookingdetail.ReturnPickupStatus,
                            IsSubReturn = bookingdetail.IsSubReturn,
                            OnlineSubRouteName = subRouteNameReturn.ToString(),
                            LuggageType = bookingdetail.LuggageType,
                            IsGhanaRoute = bookingdetail.IsGhanaRoute,
                            PassportType = bookingdetail.PassportType,
                            PassportId = bookingdetail.PassportId,
                            PlaceOfIssue = bookingdetail.PlaceOfIssue,
                            IssuedDate = bookingdetail.IssuedDate,
                            Nationality = bookingdetail.Nationality,
                        });

                        returnBookingRef = rfcode;

                        totalAmount += Math.Round(bookingdetail.Amount.GetValueOrDefault());

                        _bookingAcctSvc.UpdateBookingAccount(bookingdetail.BookingType, bookingdetail.PaymentMethod, rfcode, returnVehicletrip, Math.Round(bookingdetail.Amount.GetValueOrDefault()));
                    }
                }

                if (bookingdetail != null)
                {

                    _bookingRepo.Insert(new Booking
                    {
                        
                        CreatorUserId = _serviceHelper.GetCurrentUserId(),
                        BookingReferenceCode = refCode,
                        FirstName = bookingdetail.FirstName,
                        LastName = bookingdetail.LastName,
                        Gender = bookingdetail.Gender,
                        Email = bookingdetail.Email,
                        PhoneNumber = bookingdetail.PhoneNumber,
                        Address = bookingdetail.Address,
                        NextOfKinName = bookingdetail.NextOfKinName,
                        NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone,
                        PaymentMethod = bookingdetail.PaymentMethod,
                        PassengerType = bookingdetail.PassengerType,
                        PosReference = bookingdetail.PosReference,
                        BookingStatus = bookingdetail.BookingStatus,
                        PickupStatus = bookingdetail.PickupStatus,
                        TravelStatus = bookingdetail.TravelStatus,
                        BookingDate = Clock.Now,
                        NoOfTicket = tripList.Count > 1 ? (bookingdetail.Beneficiaries.Count + 1) * 2 : bookingdetail.Beneficiaries.Count + 1,
                        IsGhanaRoute = bookingdetail.IsGhanaRoute,
                        PassportType = bookingdetail.PassportType,
                        PassportId = bookingdetail.PassportId,
                        PlaceOfIssue = bookingdetail.PlaceOfIssue,
                        IssuedDate = bookingdetail.IssuedDate,
                        Nationality = bookingdetail.Nationality,
                    });
                }
                try
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                    ex.InnerException.Message.ToString();
                    throw;
                }

                var tripcode = await GetBookingByRefcodeAsync(refCode);

                var details = await GetBookingDetails(refCode);
                decimal amount = details.Amount.GetValueOrDefault();

                if (bookingdetail.BookingStatus == BookingStatus.Approved &&
                    (bookingdetail.BookingType != BookingTypes.Terminal))
                {

                    if (tripList.Count > 1 && !string.IsNullOrWhiteSpace(returnBookingRef))
                    {

                        var returnDetails = await GetBookingDetails(returnBookingRef);

                        await SendBookingEmail(bookingdetail.Email, details, returnDetails);
                    }
                    else
                    {
                        await SendBookingEmail(bookingdetail.Email, details);
                    }

                    await SendBookingSMS(bookingdetail.PhoneNumber, details);

                    try
                    {
                        var busCount = await ApprovedTicketCountPerBus(details.RouteId, details.VehicleTripRegistrationId);
                        if (busCount.Count == 11)
                            await SendSeatCompleteNotificationEmail(details, busCount.Count);
                    }
                    catch (Exception)
                    {

                    }
                }

                long mainBookerId = mainBooker.Id;
                return new BookingResponseDTO()
                {
                    Response = bookingdetail.BookingStatus.ToString(),
                    MainBookerId = mainBookerId,
                    BookingReferenceCode = refCode,
                    Amount = totalAmount
                };
            }

            return new BookingResponseDTO();
        }

        private async Task SendBookingSMS(string phoneNumber, SeatManagementDTO details)
        {
            string smsMessage = $"Your LIBMOT.COM booking for {details.DepartureTime} with ref:{details.refCode} was successful." +
                $" Kindly be at the terminal 30 mins before departure.";

            try
            {
                await Task.Factory.StartNew(() => _smsSvc.SendSMSNow(smsMessage, recipient: phoneNumber.ToNigeriaMobile()));
            }

            catch (Exception)
            {
            }
        }

        public Task<List<SeatManagement>> ApprovedTicketCountPerBus(int? routeId, Guid? vehicleTripRegistrationId)
        {
            var seatCountResult =
                from seatCount in _seatManagemntSvc.GetAll()
                where seatCount.RouteId == routeId
                && seatCount.BookingStatus == BookingStatus.Approved
                && seatCount.VehicleTripRegistrationId == vehicleTripRegistrationId
                select seatCount;

            return seatCountResult.ToListAsync();
        }

        private async Task SendSeatCompleteNotificationEmail(SeatManagementDTO details, int busCount)
        {
            var _emailTo = new List<string>();

            var seatCountReceiver = bookingConfig.BookingCountReciever;

            foreach (var receipient in seatCountReceiver.Split(','))
            {
                _emailTo.Add(receipient);
            }
            string countMessage = "Dear Staff, <br><br>This is to notify you that a bus Travelling from <strong>" + details.RouteName + "</strong> on " + details.DepartureDate.GetValueOrDefault().ToString(CoreConstants.DateFormat) + " by " + details.DepartureTime + " has <strong>" + busCount + " Passengers</strong>.<br><br>You can create a new Vehicle for the route if required.<br><br>Regards";
            var mail = new Mail(appConfig.AppEmail, "Seat Count Notification for " + details.RouteName, _emailTo.ToArray())
            {
                Body = countMessage
            };

            await _mailSvc.SendMailAsync(mail);
        }

        private async Task SendBookingEmail(string email, SeatManagementDTO details, SeatManagementDTO returninfo = null)
        {
            var replacement = new StringDictionary
            {
                ["Name"] = details.FullName,
                ["Refcode"] = details.BookingReferenceCode,
                ["Amount"] = details.Amount.ToString(),
                ["Ticketno"] = details.NoOfTicket.GetValueOrDefault().ToString(),
                ["DateOfBooking"] = details.DateCreated.ToString(),
                ["RouteOfTrip"] = details.RouteName,
                ["SeatNo"] = details.SeatNumber.ToString(),
                ["DepatureDate"] = details.DepartureDate.GetValueOrDefault().ToString(CoreConstants.DateFormat),
                ["BookingStatus"] = details.BookingStatus.ToString(),
                ["DepartureTime"] = details.DepartureTime,
                ["ExpiryDate"] = details.DateCreated.AddDays(30).ToString(CoreConstants.DateFormat),
                ["BookingType"] = details.BookingType.ToString(),
                ["PhoneNo"] = details.PhoneNumber.ToString(),
                ["NoOfTickets"] = details.NoOfTicket.ToString()
            };

            if (returninfo != null)
            {
                replacement["ArrivalDate"] = returninfo.DepartureDate.GetValueOrDefault().ToString(CoreConstants.DateFormat);
                replacement["ArrivalTime"] = returninfo.DepartureTime;
                replacement["ArrivalSeatNo"] = returninfo.SeatNumber.ToString();
                replacement["ArrivalRouteOfTrip"] = returninfo.RouteName;
            }

            try
            {
                var mail = new Mail(appConfig.AppEmail, "Successful Booking Notification!", email)
                {
                    BodyIsFile = true
                };

                if (returninfo is null)
                    mail.BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.BookingSuccessEmail);
                else mail.BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.BookingAndReturnSuccessEmail);

                await _mailSvc.SendMailAsync(mail, replacement);
            }
            catch (Exception)
            {
            }
        }

        public async Task<SeatManagementDTO> GetBookingDetails(string refCode)
        {
            var vehicleTrip = new VehicleTripRegistration();
            var trip = new Trip();

            var seat = await _seatManagemntSvc.GetSeatByRefcodeForClientAsync(refCode);

            if (seat == null)
                return null;

            if (seat.VehicleTripRegistrationId != null)
            {
                vehicleTrip = await _vehicleTripRegSvc.GetAsync(new Guid(seat.VehicleTripRegistrationId.ToString()));
                trip = await _tripSvc.GetAsync(vehicleTrip.TripId);
            }

            seat.DepartureTime = trip.DepartureTime;
            return seat;
        }

        public bool GetCustByPhone(string p)
        {
            var query = from booking in _bookingRepo.GetAll()
                        where booking.PhoneNumber == p
                        && booking.BookingStatus == BookingStatus.Approved
                        select booking;

            return query.FirstOrDefault() == null ? true : false;
        }

        public Task<BookingDTO> GetCustomerByPhone(string Phone)
        {
            var query = from booking in _bookingRepo.GetAll()
                        where booking.PhoneNumber == Phone
                        select new BookingDTO
                        {

                            FirstName = booking.FirstName,
                            MiddleName = booking.MiddleName,
                            LastName = booking.LastName,
                            Gender = booking.Gender,
                            Email = booking.Email,
                            Address = booking.Address,
                            PhoneNumber = booking.PhoneNumber,
                            NextOfKinName = booking.NextOfKinName,
                            NextOfKinPhoneNumber = booking.NextOfKinPhoneNumber,
                            IsGhanaRoute = booking.IsGhanaRoute,
                            PassportType = booking.PassportType,
                            PassportId = booking.PassportId,
                            PlaceOfIssue = booking.PlaceOfIssue,
                            IssuedDate = booking.IssuedDate,
                            ExpiredDate = booking.ExpiredDate,
                            Nationality = booking.Nationality

                        };

            return query.AsNoTracking().LastOrDefaultAsync();
        }

        public async Task<IEnumerable<SeatManagementDTO>> GetBookingHistory(string phoneNo)
        {
            return await _seatManagemntSvc.GetSeatHistoryByPhoneAsync(phoneNo);
        }

        private async Task<decimal> GetAmountByBookingDetails(Guid vehicletrip, BookingTypes bookingTypes, PassengerType passengerType = PassengerType.Adult, bool isLoggedIn = false, bool isSub = false, int? routeId = 0, int? vehicleModelId = 0, bool hasCoupon = false, string couponCode = null, string phone = null, bool isMainbooker = false, bool isreturn = false)
        {
            decimal amountToPay = 0;
            decimal farePrice = 0;
            //     decimal difference = 0;
            var bookingDiscount = new DiscountDTO();
            var discounts = new DiscountDTO()
            {
                MemberDiscount = 0,
                MinorDiscount = 0,
                AdultDiscount = 0,
                ReturnDiscount = 0,
                PromoDiscount = 0,
                AppDiscountWeb = 0,
                AppReturnDiscountWeb = 0,
                AppDiscountAndroid = 0,
                AppReturnDiscountAndroid = 0,
                AppDiscountIos = 0,
                AppReturnDiscountIos = 0,
                CustomerDiscount = 0
            };

            var username = _serviceHelper.GetCurrentUserEmail();
            bookingDiscount = await _discountSvc.GetDiscountByBookingTypeAsync(bookingTypes);
            if (bookingDiscount != null && bookingTypes == BookingTypes.Online)
            {
                discounts = bookingDiscount;

            }
            else if (bookingDiscount != null && bookingTypes == BookingTypes.Advanced)
            {
                discounts = bookingDiscount;
            }
            var farePrice1 = GetFarePriceByVehicleTripId(vehicletrip);
            if (isSub == true)
            {
                farePrice1 = GetFarePriceByForSubRoute(routeId, vehicleModelId);
            }

            if (farePrice1 != null)
            {
                farePrice = farePrice1.Amount.GetValueOrDefault();
                amountToPay = farePrice;
            }

            //apply android discount for either one way or return
            if (username == CoreConstants.AndroidBookingAccount && true)
            {
                if (!isreturn && passengerType == PassengerType.Adult)
                {
                    if (isLoggedIn)
                    {
                        if(!GetCustByPhone(phone))
                        {
                            amountToPay = farePrice - discounts.AppDiscountAndroid * farePrice / 100;
                        }
                        else
                        {
                            amountToPay = farePrice - discounts.CustomerDiscount * farePrice / 100;
                        }
                    }
                    else
                    {
                        amountToPay = farePrice - discounts.AppDiscountAndroid * farePrice / 100;
                    }

                }
                else if (isreturn && passengerType == PassengerType.Adult)
                {
                    if (isLoggedIn)
                    {
                        if (!GetCustByPhone(phone))
                        {
                            amountToPay = farePrice - discounts.AppReturnDiscountAndroid * farePrice / 100;
                        }
                        else
                        {
                            amountToPay = farePrice - discounts.CustomerDiscount * farePrice / 100;
                        }
                    }
                    else
                    {
                        amountToPay = farePrice - discounts.AppReturnDiscountAndroid * farePrice / 100;
                    }
                }
            }
            //apply ios discount for either one way or return
            // else if (username == CoreConstants.IosBookingAccount  && isLoggedIn) -- If you want the discount to be for only logged in users
            else if (username == CoreConstants.IosBookingAccount && true)
            {
                if (!isreturn && passengerType == PassengerType.Adult)
                {
                    //amountToPay = farePrice - discounts.AppDiscountIos * farePrice / 100;
                    if (isLoggedIn)
                    {
                        //string u = "";
                        //var m = GetCustomerByPhone(phone);
                        //u = m.ToString();
                        //if(u == "")
                        //{

                        //}
                        if (!GetCustByPhone(phone))
                        {
                            amountToPay = farePrice - discounts.AppDiscountIos * farePrice / 100;
                        }
                        else
                        {
                            amountToPay = farePrice - discounts.CustomerDiscount * farePrice / 100;
                        }
                    }
                    else
                    {
                        amountToPay = farePrice - discounts.AppDiscountIos * farePrice / 100;
                    }
                }
                else if (isreturn && passengerType == PassengerType.Adult)
                {
                    if (isLoggedIn)
                    {
                        if (!GetCustByPhone(phone))
                        {
                            amountToPay = farePrice - discounts.AppReturnDiscountIos * farePrice / 100;
                        }
                        else
                        {
                            amountToPay = farePrice - discounts.CustomerDiscount * farePrice / 100;
                        }
                    }
                    else
                    {
                        amountToPay = farePrice - discounts.AppReturnDiscountIos * farePrice / 100;
                    }
                    
                }
            }

            //apply website discount for either one way or return
            // else if ((username == CoreConstants.WebBookingAccount) && isLoggedIn)
            else if ((username == CoreConstants.WebBookingAccount) && isLoggedIn)
            {
                if (!isreturn && passengerType == PassengerType.Adult)
                {
                    amountToPay = farePrice - discounts.MemberDiscount * farePrice / 100;
                }
                else if (isreturn && passengerType == PassengerType.Adult)
                {
                    amountToPay = farePrice - discounts.ReturnDiscount * farePrice / 100;
                }
            }
            else if (bookingTypes == BookingTypes.Online && !isLoggedIn)
            {
                amountToPay = farePrice - discounts.AdultDiscount * farePrice / 100;
            }
            //apply child discount irrespective of platform
            if (passengerType == PassengerType.Children)
            {
                amountToPay = farePrice - discounts.MinorDiscount * farePrice / 100;
            }
            // Get departure date using vehicle trip registration id.
            var vehicleTripDetails = await _vehicleTripRegSvc.GetVehicleTripRegistrationDTO(vehicletrip);
            var depDate = vehicleTripDetails.DepartureDate;
            int? newRouteId = vehicleTripDetails.RouteId;
            if (isSub == true)
            {
                newRouteId = routeId;
            }
            if (vehicleModelId == null)
            {
                vehicleModelId = vehicleTripDetails.VehicleModelId;
            }
            var departureDate = vehicleTripDetails.DepartureDate;
            if (routeId != null)
            {

            }
            var fareCalendar = new FareCalendarDTO();
            fareCalendar = null;
            var farecalendarList = new List<FareCalendarDTO>();

            farecalendarList = await _fareCalendarSvc.GetFareCalendaListByRoutesAsync(newRouteId.GetValueOrDefault(), depDate);
            foreach (var calendar in farecalendarList)
            {
                fareCalendar = calendar;
                if (vehicleModelId == calendar.VehicleModelId)
                {
                    break;
                }
            }
            if (fareCalendar == null)
            {
                var DepartureterminalId = await _routeSvc.GetDepartureterminalIdFromRoute(newRouteId.GetValueOrDefault());
                farecalendarList = await _fareCalendarSvc.GetFareCalendaListByTerminalsAsync(DepartureterminalId, depDate);
                foreach (var calendar in farecalendarList)
                {

                    fareCalendar = calendar;
                    if (vehicleModelId == calendar.VehicleModelId)
                    {
                        break;
                    }
                }
            }


            decimal fareDifference = 0;
            if (fareCalendar != null)
            {
                if (fareCalendar.FareAdjustmentType == FareAdjustmentType.Percentage)
                {
                    fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * amountToPay / 100);

                }
                else
                {
                    fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                }

            }
            amountToPay += fareDifference;

            if (hasCoupon && couponCode != null)
            {
                var coupon = new CouponDTO()
                {
                    CouponValue = 0,
                    CouponType = 0
                };
                try
                {
                    coupon = await _couponSvc.GetValidCouponByPhone(couponCode, phone);

                }
                catch (Exception)
                {
                    // ignored
                }

                if (coupon != null)
                {
                    if (coupon.CouponType == CouponType.Percentage)
                    {
                        amountToPay = amountToPay - (coupon.CouponValue * amountToPay / 100);
                    }

                    else
                    {
                        if (isMainbooker)
                        {
                            amountToPay = amountToPay - coupon.CouponValue;
                        }
                    }
                }
            }

            return amountToPay;
        }

        public SeatManagementDTO GetFarePriceByVehicleTripId(Guid vehicleTripRegistrationId)
        {
            var todayticketbookings =
                from vehicletripregistration in _vehicleTripRegSvc.GetAll()
                join trip in _tripSvc.GetAll() on vehicletripregistration.TripId equals trip.Id

                let fare = _fareSvc.GetAll().FirstOrDefault(s => s.RouteId == trip.RouteId && s.VehicleModelId == trip.VehicleModelId)

                where vehicletripregistration.Id == vehicleTripRegistrationId

                select new SeatManagementDTO()
                {
                    VehicleTripRegistrationId = vehicletripregistration.Id,
                    Amount = fare.Amount,
                    RouteId = trip.RouteId
                };
            return todayticketbookings.FirstOrDefault();
        }

        //public async Task<bool> CheckIsBookedBefore(string phoneNumber)
        //{
        //    var record = from booking in _bookingRepo.GetAll()
        //                 where booking.PhoneNumber == phoneNumber
        //                 && booking.BookingStatus == BookingStatus.Approved
        //                 select booking;


        //    return record == null ? false : true;
        //}

        private async Task<bool> CheckAvailability(SeatManagement seat, BookingTypes bookingType)
        {
            if (bookingType != BookingTypes.Terminal)
            {
                var available = await Getavailableseat(seat);
                return available;
            }
            else
            {
                var available = await Getavailableseat(seat);
                if (!available)
                {
                    var pendingbookings = await PendingBookings(seat);

                    foreach (var pendingbooking in pendingbookings)
                    {
                        var booking = await _seatManagemntSvc.GetAsync(pendingbooking.Id);
                        booking.SeatNumber = 0;
                        booking.VehicleTripRegistrationId = null;
                        await _unitOfWork.SaveChangesAsync();
                    }
                    var checkavailable = await Getavailableseat(seat);
                    return checkavailable;
                }
                return true;
            }
        }

        public Task<BookingDTO> GetBookingByRefcodeAsync(string refcode)
        {
            var booking = from allBooking in _bookingRepo.GetAll()
                          where allBooking.BookingReferenceCode == refcode

                          let bookerTripcode = _seatManagemntSvc.GetAll().FirstOrDefault(a => a.BookingReferenceCode == allBooking.BookingReferenceCode)
                          let vehicletrip = _vehicleTripRegSvc.GetAll().FirstOrDefault(a => a.Id == bookerTripcode.VehicleTripRegistrationId)
                          let tripcode = _tripSvc.GetAll().FirstOrDefault(a => a.Id == vehicletrip.TripId)

                          select new BookingDTO
                          {
                              Id = allBooking.Id,
                              PosReference = allBooking.PosReference,
                              BookingReferenceCode = allBooking.BookingReferenceCode,
                              MiddleName = allBooking.MiddleName,
                              TripCode = tripcode.TripCode,
                              Address = allBooking.Address,
                              NumberOfTicketsPrinted = allBooking.NumberOfTicketsPrinted,
                              PickupPointImage = allBooking.PickupPointImage,
                              BookingDate = allBooking.BookingDate,
                              BookingStatus = allBooking.BookingStatus,
                              PickupStatus = allBooking.PickupStatus,
                              TravelStatus = allBooking.TravelStatus,
                              DepartureDate = vehicletrip.DepartureDate,
                              DepartureTime = tripcode.DepartureTime,
                              FirstName = allBooking.FirstName,
                              LastName = allBooking.LastName,
                              Gender = allBooking.Gender,
                              PhoneNumber = allBooking.PhoneNumber,
                              NextOfKinName = allBooking.NextOfKinName,
                              NextOfKinPhoneNumber = allBooking.NextOfKinPhoneNumber,
                              Email = allBooking.Email
                          };
            return booking.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<List<SeatManagementDTO>> PendingBookings(SeatManagement seat)
        {
            var pendingbookings =
                 from seatmanagement in _seatManagemntSvc.GetAll()
                 join vehicletripregistration in _vehicleTripRegSvc.GetAll() on seatmanagement.VehicleTripRegistrationId equals vehicletripregistration.Id
                 where seatmanagement.SeatNumber == seat.SeatNumber && seatmanagement.VehicleTripRegistrationId == seat.VehicleTripRegistrationId && seatmanagement.BookingStatus != BookingStatus.Approved

                 select new SeatManagementDTO
                 {
                     Id = seatmanagement.Id,
                     SeatNumber = seatmanagement.SeatNumber,
                     RemainingSeat = seatmanagement.RemainingSeat,
                     FromTransload = seatmanagement.FromTransload,
                     PassengerType = seatmanagement.PassengerType,
                     CreatedBy = seatmanagement.CreatedBy,
                     PaymentMethod = seatmanagement.PaymentMethod,
                     IsMainBooker = seatmanagement.IsMainBooker,
                     HasReturn = seatmanagement.HasReturn,
                     IsPrinted = seatmanagement.IsPrinted,
                     MainBookerReferenceCode = seatmanagement.MainBookerReferenceCode,
                     IsReturn = seatmanagement.IsReturn,
                     TravelStatus = seatmanagement.TravelStatus,
                 };

            return pendingbookings.ToListAsync();
        }

        public async Task<bool> Getavailableseat(SeatManagement seats)
        {
            var exists = await _seatManagemntSvc.ExistAsync(a => a.VehicleTripRegistrationId == seats.VehicleTripRegistrationId && a.SeatNumber == seats.SeatNumber);
            return !exists;
        }

        public async Task UpdateBooking(BookingDetailDTO bookingdetail)
        {
            if (bookingdetail != null)
            {
                List<string> tripList = new List<string>();
                List<int> seatTo = new List<int>();

                foreach (var trip in bookingdetail.SeatRegistrations.Split(';'))
                {
                    tripList.Add(trip);
                }

                var toTripDetail = tripList.FirstOrDefault().Split(':');

                foreach (var seats in toTripDetail[1].Split(','))
                {
                    seatTo.Add(Convert.ToInt32(seats));
                }


                var bookingByRef = await GetBookingByRefcodeAsync(bookingdetail.BookingReference);
                var seatByRef = await _seatManagemntSvc.GetSeatByRefcodeAsync(bookingdetail.BookingReference);



                var booking = await _bookingRepo.GetAsync(bookingByRef.Id);
                var seat = await _seatManagemntSvc.GetAsync(seatByRef.Id);

                booking.FirstName = bookingdetail.FirstName;
                booking.LastName = bookingdetail.LastName;
                booking.Gender = bookingdetail.Gender;
                booking.PhoneNumber = bookingdetail.PhoneNumber;
                booking.NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone;
                booking.NextOfKinName = bookingdetail.NextOfKinName;

                booking.IsGhanaRoute = bookingdetail.IsGhanaRoute;
                booking.PassportType = bookingdetail.PassportType;
                booking.PassportId = bookingdetail.PassportId;
                booking.PlaceOfIssue = bookingdetail.PlaceOfIssue;
                booking.IssuedDate = bookingdetail.IssuedDate;
                booking.ExpiredDate = bookingdetail.ExpiredDate;
                booking.Nationality = bookingdetail.Nationality;


                //check if amount has changed or previous payment method was cash (meaning, it must have had accounttransactions entry)
                if ((seat.Amount != bookingdetail.Amount && seat.PaymentMethod == PaymentMethod.Cash)
                    || (seat.Amount == bookingdetail.Amount && seat.PaymentMethod == PaymentMethod.Cash && bookingdetail.PaymentMethod != PaymentMethod.Cash)
                    )
                {

                    // Negate Previous transaction and update summary only if bookingtype is terminal
                    if (seat.BookingType == BookingTypes.Terminal)
                        _bookingAcctSvc.UpdateBookingAccount(
                                 bookingdetail.BookingType, seat.PaymentMethod, seat.BookingReferenceCode,
                                 seat.VehicleTripRegistrationId.GetValueOrDefault(), (seat.Amount - seat.Discount).GetValueOrDefault(), TransactionType.Debit);
                }



                // check if payment method has changed and the new payment method is cash, or the amount has changed
                if ((bookingdetail.PaymentMethod != seat.PaymentMethod && bookingdetail.PaymentMethod == PaymentMethod.Cash) || (seat.Amount != bookingdetail.Amount && bookingdetail.PaymentMethod == PaymentMethod.Cash))
                {
                    // Only execute if bookingytype is terminal
                    if (seat.BookingType == BookingTypes.Terminal)
                        _bookingAcctSvc.UpdateBookingAccount(
                                 bookingdetail.BookingType, bookingdetail.PaymentMethod, seat.BookingReferenceCode,
                                 seat.VehicleTripRegistrationId.GetValueOrDefault(), (seat.Amount - seat.Discount).GetValueOrDefault(), TransactionType.Credit);
                }

                seat.FullName = bookingdetail.FirstName + ' ' + bookingdetail.LastName;
                seat.NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone;
                seat.NextOfKinName = bookingdetail.NextOfKinName;
                seat.PhoneNumber = bookingdetail.PhoneNumber;
                seat.SeatNumber = (byte)seatTo[0];
                seat.Amount = bookingdetail.Amount;
                seat.Discount = bookingdetail.Discount.GetValueOrDefault();
                seat.PaymentMethod = bookingdetail.PaymentMethod;
                seat.Gender = bookingdetail.Gender;

                seat.IsGhanaRoute = bookingdetail.IsGhanaRoute;
                seat.PassportType = bookingdetail.PassportType;
                seat.PassportId = bookingdetail.PassportId;
                seat.PlaceOfIssue = bookingdetail.PlaceOfIssue;
                seat.IssuedDate = bookingdetail.IssuedDate;
                seat.ExpiredDate = bookingdetail.ExpiredDate;
                seat.Nationality = bookingdetail.Nationality;

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<BookingResponseDTO> ProcessPaystackPayment(string RefCode)
        {
            var bookingToUpdate = await GetBookingByRefcodeAsync(RefCode);

            if (bookingToUpdate == null)
            {
                return new BookingResponseDTO() { Response = "Booking Not Found" };
            }

            var booker = await _seatManagemntSvc.GetSeatByRefcodeAsync(RefCode);

            var vehicleTrip = new VehicleTripRegistration();
            var trip = new TripDTO();

            if (booker.VehicleTripRegistrationId != null)
            {
                vehicleTrip = await _vehicleTripRegSvc.GetAsync(new Guid(booker.VehicleTripRegistrationId.ToString()));
                trip = await _tripSvc.GetTripById(vehicleTrip.TripId);
            }

            if (bookingToUpdate == null)
            {
                return new BookingResponseDTO() { Response = "Booking Not Found" };
            }

            if (bookingToUpdate.BookingStatus != (int)BookingStatus.Pending)
            {
                BookingResponseDTO notPendingBookingResponseData = new BookingResponseDTO
                {
                    Response = bookingToUpdate.BookingStatus.ToString(),
                    Amount = booker.Amount,
                    BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                    Route = booker.RouteName,
                    DepartureDate = booker.DepartureDate.ToString(),
                    DepartureTime = trip.DepartureTime,
                    SeatNumber = booker.SeatNumber,
                };
                return notPendingBookingResponseData;
            }

            var transactionReference = RefCode;
            // TransactionVerifyResponse verifyResponse = null;
            PaystackVerifyResponseDto paystackVerifyResponseDto = null;
            try
            {
                //var api = new PayStackApi(payStackConfig.Secret);
                //var payresponse= api.Transactions.Verify(transactionReference);

                #region newPaystackVerifyImplementation
                var client = new RestClient("https://api.paystack.co/transaction/verify/" + transactionReference);
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", "Bearer " + payStackConfig.Secret);
                IRestResponse response = await client.ExecuteTaskAsync(request);
                var payResponse = response.Content;
                paystackVerifyResponseDto = JsonConvert.DeserializeObject<PaystackVerifyResponseDto>(response.Content);
                #endregion
                // verifyResponse = api.Transactions.Verify(transactionReference);


            }
            catch (Exception ex)
            {
                var x = 1;
                //do nothing
            }

            //var responseData = verifyResponse?.Data;
            var responseData = paystackVerifyResponseDto?.data;

            //if not found of paystack
            if (booker.PaymentMethod == PaymentMethod.PayStack && responseData == null)
            {
                await AutoReleaseSeat(transactionReference);
                return new BookingResponseDTO
                {
                    Response = "Transaction Not Found On PayStack",
                    Amount = 0,
                    BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                    Route = booker.RouteName,
                    DepartureDate = booker.DepartureDate.ToString(),
                    DepartureTime = trip.DepartureTime,
                    SeatNumber = booker.SeatNumber,
                };
            }

            // var authorization = responseData.Authorization;
            var authorization = responseData.authorization;
            var paystackPaymentResponse = new PayStackPaymentResponse
            {
                Reference = responseData.reference,
                ApprovedAmount = responseData.amount / 100,
                AuthorizationCode = authorization.authorization_code,
                CardType = authorization.card_type,
                Last4 = authorization.last4,
                Reusable = authorization.reusable,
                Bank = authorization.bank,
                ExpireMonth = authorization.exp_month,
                ExpireYear = authorization.exp_year,
                TransactionDate = responseData.transaction_date.GetValueOrDefault(),
                Channel = responseData.channel,
                Status = responseData.status


                //AuthorizationCode = authorization.authorizationCode,
                //CardType = authorization.cardType,
                //Last4 = authorization.last4,
                //Reusable = authorization.reusable.HasValue && authorization.reusable.Value,
                //Bank = authorization.Bank,
                //ExpireMonth = authorization.ExpMonth,
                //ExpireYear = authorization.ExpYear,
                //TransactionDate = responseData.TransactionDate,
                //Channel = responseData.Channel,
                //Status = responseData.Status
            };
            //
            //if (verifyResponse.Data.Status.ToLower() != "success")
            if (paystackVerifyResponseDto.data.status.ToLower() != "success")
            {
                var response = await PaystackWebHookRefAsync(RefCode);

                if (response != null)
                {
                    paystackPaymentResponse = new PayStackPaymentResponse
                    {
                        Reference = response.Reference,
                        ApprovedAmount = response.ApprovedAmount / 100,
                        AuthorizationCode = response.AuthorizationCode,
                        CardType = response.CardType,
                        Last4 = response.Last4,
                        Reusable = response.Reusable,
                        Bank = response.Bank,
                        ExpireMonth = response.ExpireMonth,
                        ExpireYear = response.ExpireYear,
                        TransactionDate = response.TransactionDate,
                        Channel = response.Channel,
                        Status = response.Status
                    };
                }
            }

            bookingToUpdate.PayStackPaymentResponse = paystackPaymentResponse;

            var payStackSuccess = paystackPaymentResponse.Status.ToLower() == "success";

            bookingToUpdate.BookingStatus = payStackSuccess ? BookingStatus.Approved : BookingStatus.Unsuccessful;

            bookingToUpdate.PaymentMethod = PaymentMethod.PayStack;
            //bookingToUpdate.GatewayResponseCode = responseData.Status;
            bookingToUpdate.PayStackResponse = responseData.status;

            await _unitOfWork.SaveChangesAsync();

            BookingDetailDTO bookingData = new BookingDetailDTO
            {
                FirstName = bookingToUpdate.FirstName,
                LastName = bookingToUpdate.LastName,
                Gender = bookingToUpdate.Gender,
                Email = bookingToUpdate.Email,
                PhoneNumber = bookingToUpdate.PhoneNumber,
                Address = bookingToUpdate.Address,
                NextOfKinName = bookingToUpdate.NextOfKinName,

                PaymentMethod = bookingToUpdate.PaymentMethod,
                PosReference = bookingToUpdate.PosReference,
                BookingStatus = bookingToUpdate.BookingStatus,
                PickupStatus = bookingToUpdate.PickupStatus,
                PickUpId = bookingToUpdate.PickUpPointId,
                TravelStatus = bookingToUpdate.TravelStatus,
                BookingReference = bookingToUpdate.BookingReferenceCode,
                PayStackPaymentResponse = bookingToUpdate.PayStackPaymentResponse,
                PayStackReference = bookingToUpdate.PayStackReference,
                PayStackResponse = bookingToUpdate.PayStackResponse
            };

            if (paystackPaymentResponse.Status.ToLower() == "ongoing")
            {
                BookingResponseDTO OngoingBookingResponseData = new BookingResponseDTO
                {

                    Response = bookingToUpdate.BookingStatus.ToString(),
                    Amount = booker.Amount,
                    BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                    Route = booker.RouteName,
                    DepartureDate = booker.DepartureDate.ToString(),
                    DepartureTime = trip.DepartureTime,
                    SeatNumber = booker.SeatNumber,
                };
                return OngoingBookingResponseData;
            }

            try
            {
                await UpdateOnlineBooking(bookingData);
            }
            catch (Exception)
            {
                BookingResponseDTO bookingResponseData2 = new BookingResponseDTO
                {
                    Response = bookingToUpdate.BookingStatus.ToString(),
                    Amount = paystackVerifyResponseDto.data.amount / 100,
                    BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                    Route = booker.RouteName,
                    DepartureDate = booker.DepartureDate.ToString(),
                    DepartureTime = trip.DepartureTime,
                    SeatNumber = booker.SeatNumber,
                };
                return bookingResponseData2;
            }


            BookingResponseDTO bookingResponseData = new BookingResponseDTO
            {

                Response = bookingToUpdate.BookingStatus.ToString(),
                Amount = paystackVerifyResponseDto.data.amount / 100,
                BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                Route = booker.RouteName,
                DepartureDate = booker.DepartureDate.ToString(),
                DepartureTime = trip.DepartureTime,
                SeatNumber = booker.SeatNumber,

            };
            return bookingResponseData;

        }
        public async Task<string> AutoReleaseSeat(string refCode)
        {
            if (refCode != string.Empty)
            {
                var bookingByRef = await GetBookingByRefcodeAsync(refCode);
                var booking = await _bookingRepo.GetAsync(bookingByRef.Id);
                //seat management table
                var seatByRef = await _seatManagemntSvc.GetSeatByRefcodeAsync(refCode);
                var seat = await _seatManagemntSvc.GetAsync((int)seatByRef.Id);
                seat.BookingStatus = booking.BookingStatus;

                await RemoveSeat(refCode);

                //remove beneficiaries 
                if (booking.NoOfTicket > 1)
                {
                    var benToUpdate = await _seatManagemntSvc.GetBeneficiarySeatByRefcodeAsync(refCode);
                    foreach (SeatManagementDTO beneficiary in benToUpdate)
                    {
                        await RemoveSeat(beneficiary.BookingReferenceCode);
                    }

                }
                await _unitOfWork.SaveChangesAsync();

                await SendUnsuccessfulMail(refCode);
            }

            return "seat removed";
        }

        public async Task SendUnsuccessfulMail(string RefCode)
        {
            var booking = await GetBookingByRefcodeAsync(RefCode);
            var details = await GetBookingDetails(RefCode);

            var replacement = new StringDictionary
            {
                ["Name"] = details.FullName,
                ["Refcode"] = details.BookingReferenceCode,
                ["Amount"] = details.Amount.ToString(),
                ["Ticketno"] = details.NoOfTicket.GetValueOrDefault().ToString(),
                ["DateOfBooking"] = details.DateCreated.ToString(),
                ["RouteOfTrip"] = details.RouteName,
                ["SeatNo"] = details.SeatNumber.ToString(),
                ["DepatureDate"] = details.DepartureDate.GetValueOrDefault().ToString(CoreConstants.DateFormat),
                ["BookingStatus"] = details.BookingStatus.ToString(),
                ["DepartureTime"] = details.DepartureTime,
                ["BookingType"] = details.BookingType.ToString(),
                ["PhoneNo"] = details.PhoneNumber.ToString(),
                ["FailedTime"] = Clock.Now.ToString(CoreConstants.TimeFormat),
            };

            var mail = new Mail(appConfig.AppEmail, "UnSuccessful Booking Notification!", booking.Email)
            {
                BodyIsFile = true,
                BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.BookingUnSuccessEmail)
            };

            var smsMessage = $"Your booking with BRN: {RefCode} was unsuccessful. If debited, " +
                            $"please visit your bank. For further enquiry, call 09031565022";
            try
            {
                _smsSvc.SendSMSNow(smsMessage, recipient: details.PhoneNumber.ToNigeriaMobile());
                await _mailSvc.SendMailAsync(mail, replacement);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        public async Task<BookingResponseDTO> ProcessPaystackWebhook(string RefCode)
        {
            var testOrLiveSecret = payStackConfig.Secret;

            var api = new PayStackApi(testOrLiveSecret);

            var bookingByRef = await GetBookingByRefcodeAsync(RefCode);

            var bookingToUpdate = await _bookingRepo.GetAsync((int)bookingByRef.Id);


            var booker = await _seatManagemntSvc.GetSeatByRefcodeAsync(RefCode);
            VehicleTripRegistration vehicleTrip = new VehicleTripRegistration();
            TripDTO trip = new TripDTO();
            if (booker.VehicleTripRegistrationId != null)
            {
                vehicleTrip = await _vehicleTripRegSvc.GetAsync(new Guid(booker.VehicleTripRegistrationId.ToString()));
                trip = await _tripSvc.GetTripById(vehicleTrip.TripId);
            }

            PaystackVerifyResponseDto paystackVerifyResponseDto = null;
            #region newPaystackVerifyImplementation
            var client = new RestClient("https://api.paystack.co/transaction/verify/" + RefCode);
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("Authorization", "Bearer " + payStackConfig.Secret);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            var payResponse = response.Content;
            paystackVerifyResponseDto = JsonConvert.DeserializeObject<PaystackVerifyResponseDto>(response.Content);
            #endregion

            var responseData = paystackVerifyResponseDto?.data;
            var authorization = responseData.authorization;

            var PayStackPaymentResponse = new PayStackWebhookResponse
            {
                Reference = responseData.reference,
                ApprovedAmount = responseData.amount / 100,
                AuthorizationCode = authorization.authorization_code,
                CardType = authorization.card_type,
                Last4 = authorization.last4,
                Reusable = authorization.reusable,
                Bank = authorization.bank,
                ExpireMonth = authorization.exp_month,
                ExpireYear = authorization.exp_year,
                TransactionDate = responseData.transaction_date.GetValueOrDefault(),
                Channel = responseData.channel,
                Status = responseData.status
            };

            bookingToUpdate.PayStackWebhookResponse = PayStackPaymentResponse;

            await _unitOfWork.SaveChangesAsync();

            BookingResponseDTO bookingResponseData = new BookingResponseDTO
            {
                Response = bookingToUpdate.BookingStatus.ToString(),
                Amount = responseData.amount / 100,
                BookingReferenceCode = bookingToUpdate.BookingReferenceCode,
                Route = booker.RouteName,
                DepartureDate = booker.DepartureDate.ToString(),
                DepartureTime = trip.DepartureTime,
                SeatNumber = booker.SeatNumber,
            };
            return bookingResponseData;
        }
        public async Task RemoveSeat(string refcode)
        {
            var seatByRef = await _seatManagemntSvc.GetSeatByRefcodeAsync(refcode);
            var seat = await _seatManagemntSvc.GetAsync((int)seatByRef.Id);

            seat.SeatNumber = 0;
            seat.VehicleTripRegistrationId = null;
            if (seat.PaymentMethod == PaymentMethod.GtbUssd)
            {
                if (seat.PaymentMethod == PaymentMethod.GtbUssd)
                {
                    seat.BookingStatus = BookingStatus.GtbCancelled;
                }

            }
            else
            {
                seat.BookingStatus = BookingStatus.Cancelled;
            }


        }
        public Task<PayStackWebhookResponseDTO> PaystackWebHookRefAsync(string refcode)
        {
            var paystackWebHookDetails =
                from allPaystackBooking in _paystackWebHookResponseRepo.GetAll()
                where allPaystackBooking.Reference == refcode
                select new PayStackWebhookResponseDTO
                {
                    Reference = allPaystackBooking.Reference,
                    ApprovedAmount = allPaystackBooking.ApprovedAmount / 100,
                    AuthorizationCode = allPaystackBooking.AuthorizationCode,
                    CardType = allPaystackBooking.CardType,
                    Last4 = allPaystackBooking.Last4,
                    Reusable = allPaystackBooking.Reusable,
                    Bank = allPaystackBooking.Bank,
                    ExpireMonth = allPaystackBooking.ExpireMonth,
                    ExpireYear = allPaystackBooking.ExpireYear,
                    TransactionDate = allPaystackBooking.TransactionDate,
                    Channel = allPaystackBooking.Channel,
                    Status = allPaystackBooking.Status
                };
            return paystackWebHookDetails.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateOnlineBooking(BookingDetailDTO bookingdetail)
        {

            if (bookingdetail != null)
            {
                var bookingByRef = await GetBookingByRefcodeAsync(bookingdetail.BookingReference);

                var booking = await _bookingRepo.GetAsync(bookingByRef.Id);

                //booking table
                booking.Id = booking.Id;
                booking.FirstName = bookingdetail.FirstName;
                booking.LastName = bookingdetail.LastName;
                booking.Gender = bookingdetail.Gender;
                booking.PhoneNumber = bookingdetail.PhoneNumber;
                booking.NextOfKinPhoneNumber = bookingdetail.NextOfKinPhone;
                booking.NextOfKinName = bookingdetail.NextOfKinName;
                booking.BookingStatus = bookingdetail.BookingStatus;
                booking.PayStackPaymentResponse = bookingdetail.PayStackPaymentResponse;
                booking.PayStackReference = bookingdetail.PayStackReference;
                booking.PayStackResponse = bookingdetail.PayStackResponse;
                booking.QuickTellerReference = bookingdetail.QuickTellerReference;
                booking.QuickTellerResponse = bookingdetail.QuickTellerResponse;
                booking.ApprovedBy = bookingdetail.ApprovedBy;

                //seat management table
                var seatByRef = await _seatManagemntSvc.GetSeatByRefcodeAsync(bookingdetail.BookingReference);
                var seat = await _seatManagemntSvc.GetAsync(seatByRef.Id);

                seat.BookingStatus = bookingdetail.BookingStatus;

                SeatManagementDTO @return = null;

                if (booking.NoOfTicket > 1)
                {
                    var benToUpdate = await _seatManagemntSvc.GetBeneficiarySeatByRefcodeAsync(bookingdetail.BookingReference);

                    //approve beneficiaries
                    foreach (SeatManagementDTO beneficiary in benToUpdate)
                    {
                        var benSeatByRef = await _seatManagemntSvc.GetSeatByRefcodeAsync(beneficiary.BookingReferenceCode);
                        var benSeat = await _seatManagemntSvc.GetAsync(benSeatByRef.Id);
                        benSeat.BookingStatus = bookingdetail.BookingStatus;
                    }

                    var returnBookingCode = benToUpdate?.FirstOrDefault(x => x.IsReturn)?.BookingReferenceCode;
                    @return = string.IsNullOrWhiteSpace(returnBookingCode) ? null : await GetBookingDetails(returnBookingCode);
                }

                //when it is not successfull
                if (bookingdetail.BookingStatus != BookingStatus.Approved)
                {
                    await RemoveSeat(bookingdetail.BookingReference);

                    //remove beneficiaries 
                    if (booking.NoOfTicket > 1)
                    {
                        var benToUpdate = await _seatManagemntSvc.GetBeneficiarySeatByRefcodeAsync(bookingdetail.BookingReference);
                        foreach (SeatManagementDTO beneficiary in benToUpdate)
                        {
                            await RemoveSeat(beneficiary.BookingReferenceCode);
                        }
                    }
                    //send unsuccessful email
                    await SendUnsuccessfulMail(booking.BookingReferenceCode);
                }

                await _unitOfWork.SaveChangesAsync();
                var details = await GetBookingDetails(bookingdetail.BookingReference);
                decimal amount = details.Amount.GetValueOrDefault();

                if (bookingdetail.BookingStatus.Equals(BookingStatus.Approved))
                {
                    await SendBookingSMS(booking.PhoneNumber, details);
                    await SendBookingEmail(booking.Email, details, @return);
                }
            }
        }

        private string GetTerminalCode(string terminalName)
        {

            switch (terminalName)
            {
                case "Okota - Ago":
                    return "AG";
                case "Ejigbo":
                    return "EJ";
                case "IY":
                    return "AG";
                case "Mazamaza ":
                    return "MZ";
                case "Ijesha":
                    return "IJ";
                case "Bariga":
                    return "BA";
                case "Mile 2":
                    return "ML";
                case "Owerri":
                    return "OW";
                case "Enugu":
                    return "EG";
                case "Aba":
                    return "AB";
                case "Umuahia":
                    return "UM";
                case "Asaba":
                    return "AS";

                case "PHC":
                    return "PH";
                case "Mbaise":
                    return "MB";

                case "Awka":
                    return "AW";
                case "Ekwulobia":
                    return "EK";

                case "Nnewi":
                    return "NW";
                case "Anara":
                    return "AN";
                case "Warri":
                    return "WR";
                default:
                    return "UK";
            }
        }

        public async Task<IEnumerable<SeatManagementDTO>> GetBookingHistory()
        {
            return await _seatManagemntSvc.GetSeatHistoryAsync();
        }

        public Task<List<TicketRescheduleDTO>> RescheduleTicketSearch(TicketRescheduleDTO ticketbooking)
        {

            var ticketbookingseach =

            from seatmanagement in _seatManagemntSvc.GetAll()
            join vehicletripregistration in _vehicleTripRegSvc.GetAll() on seatmanagement.VehicleTripRegistrationId equals vehicletripregistration.Id
               into vehicletripregistrations
            from vehicletripregistration in vehicletripregistrations.DefaultIfEmpty()

            join trip in _tripSvc.GetAll() on vehicletripregistration.TripId equals trip.Id
            into trips
            from trip in trips.DefaultIfEmpty()

            join route in _routeSvc.GetAll() on trip.RouteId equals route.Id
            into routes
            from route in routes.DefaultIfEmpty()

            join terminal in _terminalRepo.GetAll() on route.DepartureTerminalId equals terminal.Id
              into terminals
            from terminal in terminals.DefaultIfEmpty()


            join booking in _bookingRepo.GetAll() on seatmanagement.BookingReferenceCode equals booking.BookingReferenceCode
              into bookings
            from booking in bookings.DefaultIfEmpty()

            where seatmanagement.BookingStatus == BookingStatus.Approved && seatmanagement.TravelStatus != TravelStatus.Travelled

            select new TicketRescheduleDTO
            {
                VehicleTripRegistrationId = seatmanagement.VehicleTripRegistrationId,
                SeatManagementId = seatmanagement.Id,
                Refcode = seatmanagement.BookingReferenceCode,
                MainBookerRefcode = seatmanagement.MainBookerReferenceCode,
                CustomerNumber = seatmanagement.PhoneNumber,
                BookedDate = seatmanagement.CreationTime,
                DepartureDate = vehicletripregistration.DepartureDate,
                VehicleModel = vehicletripregistration.VehicleModel.Name,
                Amount = seatmanagement.Amount,
                SeatNumber = seatmanagement.SeatNumber,
                NokNumber = seatmanagement.NextOfKinPhoneNumber,
                NokName = seatmanagement.NextOfKinName,
                CustomerName = seatmanagement.FullName,
                NoofTicket = seatmanagement.NoOfTicket,
                BookingType = seatmanagement.BookingType,
                RouteId = seatmanagement.RouteId,
                TripId = trip.Id,
                Email = booking.Email,
                Route = route.Name,
                BookingStatus = seatmanagement.BookingStatus,
                TravelStatus = seatmanagement.TravelStatus,
                TerminalId = terminal.Id,
                RescheduleReferenceCode = seatmanagement.RescheduleReferenceCode,
                RescheduleStatus = seatmanagement.RescheduleStatus,
                IsRescheduled = seatmanagement.IsRescheduled,
                CreatedBy = seatmanagement.CreatedBy
            };


            if (ticketbooking.TerminalId > 0)
            {
                ticketbookingseach = ticketbookingseach.Where(x => x.TerminalId == ticketbooking.TerminalId);
            }

            if (!string.IsNullOrWhiteSpace(ticketbooking.Refcode))
            {
                var refcode = ticketbooking.Refcode.Trim().ToUpper();
                ticketbookingseach = ticketbookingseach.Where(x => x.MainBookerRefcode == refcode || x.Refcode == refcode);
            }
            if (ticketbooking.BookingType >= 0)
            {
                ticketbookingseach = ticketbookingseach.Where(x => x.BookingType == ticketbooking.BookingType);
            }
            if (ticketbooking.BookingStatus >= 0)
            {
                ticketbookingseach = ticketbookingseach.Where(x => x.BookingStatus == ticketbooking.BookingStatus);
            }

            return ticketbookingseach.AsNoTracking().ToListAsync();
        }

        public async Task<string> RescheduleBooking(RescheduleDTO reschedule)
        {
            string vtype = reschedule.vType;
            var vehicletrip = await _vehicleTripRegSvc.GetAsync(reschedule.VehicleTripRegistrationId.GetValueOrDefault());

            reschedule.VehicleModel = vehicletrip.VehicleModelId;
            string reschedulesubmit = "";
            if (vtype == "Re-Allocate")
            {
                reschedulesubmit = ReAllocateSeat(reschedule);
            }
            else
            {
                 reschedulesubmit = Reschedulebooking(reschedule);


            }
            

            if (reschedulesubmit != "" && reschedule.RescheduleMode == RescheduleMode.Admin && string.IsNullOrEmpty(vtype))
            {

                string customEmail = "noemail@libmot.com"; //this is the email used in case user dont supply email
                var tripcode = await _seatManagemntSvc.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); // _uow.SeatManagements.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); 
                var mainbooker = await GetBookingByRefcodeAsync(tripcode?.MainBookerReferenceCode);
                var smsMessage = $"Your LIBMOT Reschedule was successful with BRN: {reschedule.BookingReferenceCode}. Your new Departure Date is:" + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                             ". Kindly be at the terminal 30mins before departure";
                try
                {
                    _smsSvc.SendSMSNow(smsMessage, recipient: tripcode.PhoneNumber.ToNigeriaMobile());
                }
                catch { }

                if (!mainbooker.Email.Equals(customEmail) && mainbooker.Email != null)
                {

                    await SendBookingRescheduleSuccessEmail(mainbooker.Email, new SeatManagementDTO
                    {
                        BookingReferenceCode = reschedule.BookingReferenceCode,
                        TripCode = tripcode.TripCode,
                        DepartureDate = tripcode.DepartureDate,
                        DepartureTime = tripcode.DepartureTime
                    });
                }
            }
            else if (reschedulesubmit != "" && reschedule.RescheduleMode == RescheduleMode.Admin && vtype == "Re-Allocate")
            {
                string customEmail = "support@libmot.com"; //this is the email used in case user dont supply email
                var tripcode = await _seatManagemntSvc.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); // _uow.SeatManagements.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); 
                var mainbooker = await GetBookingByRefcodeAsync(tripcode?.MainBookerReferenceCode);
                string smsMessage = "";
                smsMessage =
                    $"Seat Allocation was successful for BRN : {reschedule.BookingReferenceCode}. New Seat :  { tripcode.SeatNumber}. Your Departure Date is:" + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                            ". Check-in closes 30mins before departure";
                try
                {
                    //_smsSvc.SendSMSNow(smsMessage, new[] { tripcode.PhoneNumber.NigerianMobile() });
                    _smsSvc.SendSMSNow(smsMessage, recipient: tripcode.PhoneNumber.ToNigeriaMobile());
                    if (!mainbooker.Email.Equals(customEmail) && mainbooker.Email != null)
                    {

                        var mail = new Mail(appConfig.AppEmail, "Seat Allocation Notification!", customEmail)
                        {
                            IsBodyHtml = true,
                            BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, "New Seat Allocation with reference code: " + reschedule.BookingReferenceCode +
                             ". New Seat: " + tripcode.SeatNumber + ". and Trip Code: " + tripcode.TripCode +
                             " was successful.\r\nYour Departure Date is:" + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                             ".\r\n\r\nCheck-in closes 30mins before departure. Note: only one medium sized box is allowed" +
                             ".\r\n\r\nSent from libmot.com")
                        };
                        await _mailSvc.SendMailAsync(mail);
                    }

                }
                catch (Exception)
                {
                    //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
                }
            }
            return reschedulesubmit;
        }

        private async Task SendBookingRescheduleSuccessEmail(string email, SeatManagementDTO reschedule)
        {
            var replacement = new StringDictionary
            {
                ["TripCode"] = reschedule.TripCode,
                ["Refcode"] = reschedule.BookingReferenceCode,
                ["DepatureDate"] = reschedule.DepartureDate.GetValueOrDefault().ToString(CoreConstants.DateFormat),
                ["DepartureTime"] = reschedule.DepartureTime,
            };

            try
            {
                var mail = new Mail(appConfig.AppEmail, "Reschedule Booking Notification!", email)
                {
                    IsBodyHtml = true,
                    BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.RescheduleSuccessEmail)
                };
                await _mailSvc.SendMailAsync(mail, replacement);
            }
            catch { }
        }

        private string ReAllocateSeat(RescheduleDTO reschedule)
        {
            var seatnumber = (byte)reschedule.SeatNumber;

                var reschedulerefcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reschedule.BookingReferenceCode);

                if (reschedulerefcode is null)
                    return null;

                reschedulerefcode.VehicleTripRegistrationId = reschedule.VehicleTripRegistrationId;
                reschedulerefcode.SeatNumber = seatnumber;
                reschedulerefcode.VehicleModelId = reschedule.VehicleModel;

                //For DatatSync app not to overwrite tickets that have been rescheduled online

                //for cancelled tickets whose bookingsatus by now would be SUSPENDED
                reschedulerefcode.BookingStatus = BookingStatus.Approved;

                // reschedulerefcode.RouteId = reschedule.RouteId;
                _unitOfWork.SaveChanges();

                //  Get reschedule refcode from seatmanagement table
                var rescheduleRefcodes = _seatManagemntSvc.FirstOrDefault(a => a.RescheduleReferenceCode == reschedulerefcode.RescheduleReferenceCode);

            return rescheduleRefcodes.RescheduleReferenceCode;
        }


        private string Reschedulebooking(RescheduleDTO reschedule)
        {
            var seatnumber = (byte)reschedule.SeatNumber;

            if (reschedule.RescheduleMode == RescheduleMode.Admin)
            {
                var reschedulerefcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reschedule.BookingReferenceCode);

                if (reschedulerefcode is null)
                    return null;

                reschedulerefcode.VehicleTripRegistrationId = reschedule.VehicleTripRegistrationId;
                reschedulerefcode.SeatNumber = seatnumber;
                reschedulerefcode.RescheduleReferenceCode = reschedule.BookingReferenceCode + "-RS";

                reschedulerefcode.RescheduleStatus = reschedule.RescheduleStatus;

                //reschedulerefcode.RescheduleStatus = RescheduleStatus.PayAtTerminal;
                reschedulerefcode.VehicleModelId = reschedule.VehicleModel;
                reschedulerefcode.IsRescheduled = true;

                //For DatatSync app not to overwrite tickets that have been rescheduled online
                reschedulerefcode.RescheduleType = RescheduleType.RescheduledIntoBus;
                reschedulerefcode.RescheduleDate = Clock.Now;

                //for cancelled tickets whose bookingsatus by now would be SUSPENDED
                reschedulerefcode.BookingStatus = BookingStatus.Approved;

                // reschedulerefcode.RouteId = reschedule.RouteId;
                _unitOfWork.SaveChanges();


                //  Get reschedule refcode from seatmanagement table

                var rescheduleRefcodes = _seatManagemntSvc.FirstOrDefault(a => a.RescheduleReferenceCode == reschedulerefcode.RescheduleReferenceCode);

                return rescheduleRefcodes.RescheduleReferenceCode;
            }

            else
            {

                var refcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reschedule.BookingReferenceCode);

                if (refcode is null)
                    return null;

                refcode.VehicleTripRegistrationId = reschedule.VehicleTripRegistrationId;
                refcode.SeatNumber = seatnumber;
                refcode.RescheduleReferenceCode = reschedule.BookingReferenceCode + "-RS";
                refcode.RescheduleStatus = RescheduleStatus.Pending;
                refcode.RescheduleMode = reschedule.RescheduleMode;
                refcode.VehicleModelId = reschedule.VehicleModel;

                //for cancelled tickets whose bookingsatus by now would be SUSPENDED
                refcode.BookingStatus = BookingStatus.Approved;

                //For DatatSync app not to overwrite tickets that have been rescheduled online
                refcode.RescheduleType = RescheduleType.RescheduledIntoBus;
                refcode.RescheduleDate = Clock.Now;

                //refcode.RouteId = reschedule.RouteId;
                _unitOfWork.SaveChanges();

                //  Get reschedule refcode from seatmanagement table

                var rescheduleRefcode = _seatManagemntSvc.FirstOrDefault(a => a.RescheduleReferenceCode == refcode.RescheduleReferenceCode);

                return rescheduleRefcode.RescheduleReferenceCode;
            }
        }


        public async Task<string> allocateSeat(RescheduleDTO reschedule)
        {
            var vehicletrip = await _vehicleTripRegSvc.GetAsync(reschedule.VehicleTripRegistrationId.GetValueOrDefault());

            reschedule.VehicleModel = vehicletrip.VehicleModelId;
            string reschedulesubmit = await allocateSeat(reschedule);

            if (reschedulesubmit != "" && reschedule.RescheduleMode == RescheduleMode.Admin)
            {
                string customEmail = "noemail@libmot.com"; //this is the email used in case user dont supply email
                var tripcode = await _seatManagemntSvc.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); // _uow.SeatManagements.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); 
                var mainbooker = await GetBookingByRefcodeAsync(tripcode?.MainBookerReferenceCode);
                string smsMessage = "";
                smsMessage =
                    $"Seat Allocation was successful for BRN : {reschedule.BookingReferenceCode}. New Seat :  { tripcode.SeatNumber}. Your Departure Date is:" + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                            ". Check-in closes 30mins before departure";
                try
                {
                    //_smsSvc.SendSMSNow(smsMessage, new[] { tripcode.PhoneNumber.NigerianMobile() });
                    _smsSvc.SendSMSNow(smsMessage, recipient: tripcode.PhoneNumber.ToNigeriaMobile());
                    if (!mainbooker.Email.Equals(customEmail) && mainbooker.Email != null)
                    {

                        var mail = new Mail(appConfig.AppEmail, "Seat Allocation Notification!", customEmail)
                        {
                            IsBodyHtml = true,
                            BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, "New Seat Allocation with reference code: " + reschedule.BookingReferenceCode +
                             ". New Seat: " + tripcode.SeatNumber + ". and Trip Code: " + tripcode.TripCode +
                             " was successful.\r\nYour Departure Date is:" + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                             ".\r\n\r\nCheck-in closes 30mins before departure. Note: only one medium sized box is allowed" +
                             ".\r\n\r\nSent from libmot.com")
                        };
                        await _mailSvc.SendMailAsync(mail);
                    }

                }
                catch (Exception)
                {
                    //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
                }
            }
            return reschedulesubmit;
        }



        public async Task<string> Reroutebookingupdate(RescheduleDTO reroute)
        {

            var seatnumber = (byte)reroute.SeatNumber;

            if (reroute.RerouteMode == RerouteMode.Admin)
            {
                var reschedulerefcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reroute.BookingReferenceCode);

                if (reschedulerefcode == null)
                    return null;

                reschedulerefcode.VehicleTripRegistrationId = reroute.VehicleTripRegistrationId;
                reschedulerefcode.SeatNumber = seatnumber;
                reschedulerefcode.RerouteReferenceCode = reroute.BookingReferenceCode + "-RR";
                reschedulerefcode.RerouteStatus = RerouteStatus.PayAtTerminal;
                reschedulerefcode.IsRerouted = true;
                reschedulerefcode.VehicleModelId = reschedulerefcode.VehicleModelId;
                reschedulerefcode.RouteId = reroute.RouteId;

                // For DatatSync app not to overwrite tickets that have been rescheduled online
                reschedulerefcode.RescheduleType = RescheduleType.RescheduledIntoBus;
                reschedulerefcode.RescheduleDate = DateTime.Now;

                //for cancelled tickets whose bookingsatus by now would be SUSPENDED
                reschedulerefcode.BookingStatus = BookingStatus.Approved;

                var farerefcode = _fareSvc.FirstOrDefault(a => a.RouteId == reroute.RouteId && a.VehicleModelId == reschedulerefcode.VehicleModelId);
                var vtriprefcode = _vehicleTripRegSvc.FirstOrDefault(a => a.Id == reroute.VehicleTripRegistrationId);
                var triprefcode = _tripSvc.FirstOrDefault(a => a.RouteId == reroute.RouteId && a.DepartureTime == reroute.DepartureTime);
                vtriprefcode.TripId = triprefcode.Id;
                reschedulerefcode.Amount = farerefcode.Amount;
                //if (reroute.NewRouteAmount.GetValueOrDefault() > reroute.PreviousRouteAmount.GetValueOrDefault())
                if (farerefcode.Amount > reroute.PreviousRouteAmount)
                {
                    reschedulerefcode.RerouteFeeDiff = Math.Round(farerefcode.Amount - reroute.PreviousRouteAmount.GetValueOrDefault());
                }
                else
                {
                    reschedulerefcode.RerouteFeeDiff = 0;
                }
                await _unitOfWork.SaveChangesAsync();


                //  Get reschedule refcode from seatmanagement table

                var rescheduleRefcodes = _seatManagemntSvc.FirstOrDefault(a => a.RerouteReferenceCode == reschedulerefcode.RerouteReferenceCode);

                return rescheduleRefcodes.RerouteReferenceCode;
            }

            else
            {

                var refcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reroute.BookingReferenceCode);

                if (refcode == null)
                    return null;

                refcode.VehicleTripRegistrationId = reroute.VehicleTripRegistrationId;
                refcode.SeatNumber = seatnumber;
                refcode.RerouteReferenceCode = reroute.BookingReferenceCode + "-RR";
                refcode.RerouteStatus = RerouteStatus.Pending;
                refcode.RerouteMode = reroute.RerouteMode;
                refcode.RouteId = reroute.RouteId;
                refcode.VehicleModelId = reroute.VehicleModel;
                refcode.RerouteFeeDiff = reroute.NewRouteAmount - reroute.Amount;
                //for cancelled tickets whose bookingsatus by now would be SUSPENDED
                refcode.BookingStatus = BookingStatus.Approved;

                // For DatatSync app not to overwrite tickets that have been rescheduled online
                refcode.RescheduleType = RescheduleType.RescheduledIntoBus;
                refcode.RescheduleDate = DateTime.Now;

                _unitOfWork.SaveChanges();
                //  Get reschedule refcode from seatmanagement table
                var rescheduleRefcode = _seatManagemntSvc.FirstOrDefault(a => a.RerouteReferenceCode == refcode.RerouteReferenceCode);

                return rescheduleRefcode.RerouteReferenceCode;
            }

        }

        public async Task<string> Reroutebooking(RescheduleDTO reroute)
        {
            var seat = await _seatManagemntSvc.GetSeatAndVehicleModelByRefcodeAsync(reroute.BookingReferenceCode);
            var reschedulerefcode = _seatManagemntSvc.FirstOrDefault(a => a.BookingReferenceCode == reroute.BookingReferenceCode);
            if (seat != null)
            {
                reroute.Amount = seat.Amount;

                var vehicletrip = await _vehicleTripRegSvc.GetVehicleTripRegistrationDTO(reroute.VehicleTripRegistrationId.GetValueOrDefault());
                reroute.VehicleModel = vehicletrip.VehicleModelId;
                var previousRouteFare = await _fareSvc.GetFareByVehicleTrip(reroute.PreviousRouteId.GetValueOrDefault(), seat.VehicleModelId.GetValueOrDefault());
                reroute.PreviousRouteAmount = previousRouteFare?.Amount;
                var newRouteFare = await _fareSvc.GetFareByVehicleTrip(reroute.RouteId.GetValueOrDefault(), vehicletrip.VehicleModelId.GetValueOrDefault());
                reroute.NewRouteAmount = newRouteFare?.Amount;
            }

            string reroutesubmit = await Reroutebookingupdate(reroute);     //Note this for change

            if (reroutesubmit != "" && reroute.RerouteMode == RerouteMode.Admin)
            {
                string customEmail = "support@libmot.com"; //this is the email used in case user dont supply email
                var tripcode = await _seatManagemntSvc.GetSeatByRefcodeAsync(reroute.BookingReferenceCode); // _uow.SeatManagements.GetSeatByRefcodeAsync(reschedule.BookingReferenceCode); 
                var mainbooker = await GetBookingByRefcodeAsync(tripcode?.MainBookerReferenceCode);
                string smsMessage = "";
                smsMessage =
                    $"Your libmot Reroute was successful with BRN: {reroute.BookingReferenceCode}. Your new Route is: " + tripcode.RouteName + " and Departure date is: " + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                            ". Check-in closes 30mins before departure";

                var farerefcode = _fareSvc.FirstOrDefault(a => a.RouteId == reroute.RouteId && a.VehicleModelId == reschedulerefcode.VehicleModelId);
                var vtriprefcode = _vehicleTripRegSvc.FirstOrDefault(a => a.Id == reroute.VehicleTripRegistrationId);
                var triprefcode = _tripSvc.FirstOrDefault(a => a.RouteId == reroute.RouteId && a.DepartureTime == reroute.DepartureTime);

                if (farerefcode.Amount > reroute.PreviousRouteAmount)
                {
                    var farediff =  Math.Round(farerefcode.Amount - reroute.PreviousRouteAmount.GetValueOrDefault());
                    smsMessage =
               $"Your libmot Reroute was successful with BRN: {reroute.BookingReferenceCode}. Your new Route is: " + tripcode.RouteName + " and Departure date is: " + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                       ". You would be required to pay additional N" + farediff + " , Check -in closes 30mins before departure";
                }
                try
                {
                    _smsSvc.SendSMSNow(smsMessage, recipient: tripcode.PhoneNumber.ToNigeriaMobile());
                    if (!mainbooker.Email.Equals(customEmail) && mainbooker.Email != null)
                    {

                        if (reroute.NewRouteAmount > reroute.Amount)
                        {
                            var farediff = reroute.NewRouteAmount - reroute.Amount;

                            var mail = new Mail(appConfig.AppEmail, "Reroute Notification!", customEmail)
                            {
                                IsBodyHtml = true,
                                BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Your libmot rerouting with reference code: " + reroute.BookingReferenceCode +
                                " and Trip Code: " + tripcode.TripCode +
                                " was successful.\r\nYour new Route is: " + tripcode.RouteName + " and Departure date is: " + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                                ".\r\n\r\nYou would be required to pay additional N" + farediff + " ,Check-in closes 30mins before departure. Note: only one medium sized box is allowed" +
                                ".\r\n\r\nSent from libmot.com")
                            };
                            await _mailSvc.SendMailAsync(mail);
                        }
                        else
                        {
                            var mail = new Mail(appConfig.AppEmail, "Reroute Notification!", customEmail)
                            {
                                IsBodyHtml = true,
                                BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Your libmot rerouting with reference code: " + reroute.BookingReferenceCode +
                                                     " and Trip Code: " + tripcode.TripCode +
                                                     " was successful.\r\nYour new Route is: " + tripcode.RouteName + " and Departure date is: " + tripcode.DepartureDate.GetValueOrDefault().ToString("dd MMMM, yyyy") + " by " + tripcode.DepartureTime +
                                                     ".\r\n\r\nCheck-in closes 30mins before departure. Note: only one medium sized box is allowed" +
                                                     ".\r\n\r\nSent from libmot.com")
                            };
                            await _mailSvc.SendMailAsync(mail);

                        }
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
                }
            }
            return reroutesubmit;
        }

        public  Task<IPagedList<SeatManagementDTO>> GetTraveledCustomersAsync(DateTime startdate, DateTime enddate, int pageNumber, int pageSize, string query = null)
        {
            //var traveledCust = from seatmanagement in _seatManagemntSvc.GetAll()

            //                   join vehicletripregistration in _vehicleTripRegSvc on seatmanagement.VehicleTripRegistration equals vehicletripregistration.Id 
            //                   where seatmanagement.DepartureDate >= startdate  
            //                   select new SeatManagementDTO
            //                   {
            //                       FullName = seatmanagement.FullName,
            //                       PhoneNumber = seatmanagement.PhoneNumber,
            //                       DepartureDate = seatmanagement.DepartureDate,
            //                       VehicleName = vehicle.PhysicalBusRegistrationNumber
            //                   };

            var traveledCust = (from a in _seatManagemntSvc.GetAll()
                               join b in _vehicleTripRegSvc.GetAll() on a.VehicleTripRegistrationId equals b.Id
                               where b.DepartureDate >= startdate && b.DepartureDate <= enddate 
                               && a.TravelStatus == TravelStatus.Travelled
                               select new SeatManagementDTO
                               {
                                   FullName = a.FullName,
                                   PhoneNumber = a.PhoneNumber,
                                   DepartureDate = b.DepartureDate,
                                   VehicleName = b.PhysicalBusRegistrationNumber,
                                   RouteName = a.Route.Name
                                   
                               }).GroupBy(c => new { c.FullName, c.PhoneNumber})
                                .Select(k => k.FirstOrDefault());


            return traveledCust.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        public Task<List<SeatManagementDTO>> GetTraveledCustomers(DateTime startdate)
        {
            var traveledCust = (from a in _seatManagemntSvc.GetAll()
                                join b in _vehicleTripRegSvc.GetAll() on a.VehicleTripRegistrationId equals b.Id
                                where b.DepartureDate == startdate && a.TravelStatus == TravelStatus.Travelled
                                select new SeatManagementDTO
                                {
                                    FullName = a.FullName,
                                    PhoneNumber = a.PhoneNumber,
                                    DepartureDate = b.DepartureDate,
                                    VehicleName = b.PhysicalBusRegistrationNumber,
                                    RouteName = a.Route.Name

                                }).GroupBy(c => new { c.FullName, c.PhoneNumber })
                                .Select(k => k.FirstOrDefault());


            return traveledCust.ToListAsync();
        }
    }
}