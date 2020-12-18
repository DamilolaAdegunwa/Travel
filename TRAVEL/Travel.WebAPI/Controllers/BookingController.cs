using IPagedList;
using Travel.Business.Services;
using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Core.Messaging.Email;
using Travel.Core.Messaging.Sms;
using Travel.WebAPI.Utils;
using Travel.WebAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class BookingController : BaseController
    {
        private readonly IUserService _userManagerSvc;
        private readonly IMailService _mailSvc;
        private readonly IBookingService _bookingSvc;
        private readonly IEmployeeService _empSvc;
        private readonly IServiceHelper _serviceHelper;
        private readonly ISMSService _smsSvc;
        private readonly AppConfig appConfig;
        public BookingController(IUserService userManagerSvc,
            IMailService mailSvc,
            IBookingService bookingSvc,
            ISMSService smsSvc,
            IServiceHelper serviceHelper,
            IOptions<AppConfig> _appConfig,
            IEmployeeService empSvc)
        {
            appConfig = _appConfig.Value;
            _userManagerSvc = userManagerSvc;
            _mailSvc = mailSvc;
            _bookingSvc = bookingSvc;
            _serviceHelper = serviceHelper;
            _empSvc = empSvc;
            _smsSvc = smsSvc;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IServiceResponse<GroupedTripsDetailDTO>> GetAvailableTripDetails(VehicleTripRouteSearchDTO tripBookingSearch)
        {
            return await HandleApiOperationAsync(async () => {
                var availableTrips = await _bookingSvc.GetAvailableTripDetails(tripBookingSearch);
                return new ServiceResponse<GroupedTripsDetailDTO>(availableTrips);
            });
        }

        [HttpPost]
        [Route("TerminalSearch")]
        public async Task<IServiceResponse<GroupedTripsDetailDTO>> GetAvailableTripForTerminalDetails(VehicleTripRouteSearchDTO tripBookingSearch)
        {
            return await HandleApiOperationAsync(async () => {
                var availableTrips = await _bookingSvc.GetAvailableTripTerminalDetails(tripBookingSearch);
                return new ServiceResponse<GroupedTripsDetailDTO>(availableTrips);
            });
        }

        [HttpPost]
        [Route("PostBooking")]
        public async Task<IServiceResponse<BookingResponseDTO>> PostBooking(BookingDetailDTO bookingdetail)
        {
            return await HandleApiOperationAsync(async () => {
                var availableTrips = await _bookingSvc.PostBooking(bookingdetail);
                return new ServiceResponse<BookingResponseDTO>(availableTrips);
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Details/{refCode}")]
        public async Task<IServiceResponse<SeatManagementDTO>> GetBookingDetails(string refCode)
        {
            return await HandleApiOperationAsync(async () => {
                var seat = await _bookingSvc.GetBookingDetails(refCode);

                if (seat is null)
                    throw new LMEGenericException("Invalid refCode", $"{(int) HttpStatusCode.BadRequest}");

                return new ServiceResponse<SeatManagementDTO>(seat);
            });
        }

        [HttpGet]
        [Route("getcustomer/{phone}")]
        public async Task<IServiceResponse<BookingDTO>> GetCustomerByPhone(string phone)
        {
            return await HandleApiOperationAsync(async () => {
                var customer = await _bookingSvc.GetCustomerByPhone(phone);

                return new ServiceResponse<BookingDTO>
                {
                    Object = customer
                };
            });
        }

        [HttpGet]
        [Route("GetTripHistory/{PhoneNo}")]
        public async Task<IServiceResponse<IEnumerable<SeatManagementDTO>>> GetTripHistory(string PhoneNo)
        {
            return await HandleApiOperationAsync(async () => {
                var seat = await _bookingSvc.GetBookingHistory(PhoneNo);

                return new ServiceResponse<IEnumerable<SeatManagementDTO>>(seat);
            });
        }

        [HttpGet]
        [Route("GetTripHistory")]
        public async Task<IServiceResponse<IEnumerable<SeatManagementDTO>>> GetTripHistory()
        {
            return await HandleApiOperationAsync(async () => {
                var seat = await _bookingSvc.GetBookingHistory();

                return new ServiceResponse<IEnumerable<SeatManagementDTO>>(seat);
            });
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("AllDetails/{refCode}")]
        public async Task<IServiceResponse<List<SeatManagementDTO>>> GetAllBookingDetails(string refCode)
        {
            return await HandleApiOperationAsync(async () => {
                var seat = await _bookingSvc.GetAllBookingDetails(refCode);
                return new ServiceResponse<List<SeatManagementDTO>>(seat);
            });
        }

        [HttpGet]
        [Route("RevalidatePaystack/{RefCode}")]
        public async Task<IServiceResponse<BookingResponseDTO>> RevalidatePaystackWithRefcode(string RefCode)
        {
            var Refcode = RefCode;
            return await HandleApiOperationAsync(async () => {
                var BookingDetails = await _bookingSvc.ProcessPaystackPayment(Refcode);
                return new ServiceResponse<BookingResponseDTO>(BookingDetails);
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("RevalidateUnpaidBookings")]
        public async Task<IServiceResponse<bool>> RevalidateAllPending(int txnTime = -1)
        {
            var time = txnTime;
            return await HandleApiOperationAsync(async () => 
            {
                await _bookingSvc.RevalidateAllPending(time);
                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("UpdateBooking")]
        public async Task<IServiceResponse<bool>> UpdateBooking(BookingDetailDTO bookingdetail)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.UpdateBooking(bookingdetail);
                return new ServiceResponse<bool>();
            });
        }

        [HttpGet]
        [Route("GetTripFare/{subrouteId}/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<decimal?>> GetTripFare(int subrouteId, Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _bookingSvc.GetTripFare(subrouteId, vehicleTripRegistrationId);
                return new ServiceResponse<decimal?>(amount);
            });
        }
     

        [HttpGet]
        [Route("GetNonIdAmount/{routeId}/{modelId}")]
        public async Task<IServiceResponse<decimal?>> GetNonIdAmount(int routeId, int modelId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _bookingSvc.GetNonIdAmount(routeId, modelId);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetNewRoutefare/{routeId}/{modelId}")]
        public async Task<IServiceResponse<decimal?>> GetNewRouteFare(int routeId, int modelId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _bookingSvc.GetNewRouteFare(routeId, modelId);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetPassengerFare/{passengerInfo}")]
        public async Task<IServiceResponse<decimal?>> GetPassengerFare(string passengerInfo)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _bookingSvc.GetPassengerFare(passengerInfo);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetMainTripFare/{vehicleTripRegistrationId}")]
        public async Task<IServiceResponse<decimal?>> GetMainTripFare(Guid vehicleTripRegistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var amount = await _bookingSvc.GetMainTripFare(vehicleTripRegistrationId);
                return new ServiceResponse<decimal?>(amount);
            });
        }

        [HttpGet]
        [Route("GetAvailableSeats/{vehicletripregistrationId}")]
        public async Task<IServiceResponse<RemainingSeatDTO>> GetAvailableseats(Guid vehicletripregistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var getavailableseats = await _bookingSvc.GetAvailableseats(vehicletripregistrationId);

                return new ServiceResponse<RemainingSeatDTO>
                {
                    Object = getavailableseats
                };
            });
        }

        [HttpGet]
        [Route("Getseatsavailable/{vehicletripregistrationId}")]
        public async Task<IServiceResponse<RemainingSeatDTO>> GetseatsAvailable(Guid vehicletripregistrationId)
        {
            return await HandleApiOperationAsync(async () => {
                var getavailableseats = await _bookingSvc.GetseatsAvailable(vehicletripregistrationId);

                return new ServiceResponse<RemainingSeatDTO>
                {
                    Object = getavailableseats
                };
            });
        }

        [HttpPut]
        [Route("Upgradedowngradeticket")]
        public async Task<IServiceResponse<bool>> UpgradeDowngradeTicket(ManifestExt ticketDetail)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.UpgradeDowngradeTicket(ticketDetail);
                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("AddRefCodeTobooking")]
        public async Task<IServiceResponse<bool>> AddRefCodeToBooking(ManifestExt refCodeDetail)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.AddRefCodeToBooking(refCodeDetail);
                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("CancelBooking/{Refcode}")]
        public async Task<IServiceResponse<bool>> CancelBooking(string refcode)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.CancelBooking(refcode);
                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("Approveticket/{seatManagementId}")]
        public async Task<IServiceResponse<bool>> ApproveTicket(long seatManagementId)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.ApproveBooking(seatManagementId);
                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("Suspendbooking/{BookingReferenceCode}")]
        public async Task<IServiceResponse<bool>> Suspendbooking(string bookingReferenceCode)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.SuspendBooking(bookingReferenceCode);
                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("SwapVehicle")]
        public async Task<IServiceResponse<bool>> SwapVehicle(SwapVehicleDTO swapvehicle)
        {
            return await HandleApiOperationAsync(async () => {
                await _bookingSvc.SwapVehicle(swapvehicle);
                return new ServiceResponse<bool>();
            });
        }

        [HttpGet]
        [Route("sendotp")]
        public async Task<IServiceResponse<bool>> SendOtp()
        {
            var otp = Guid.NewGuid().ToString().Remove(8).ToUpper();
            return await HandleApiOperationAsync(async () => {
                var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
                var operationManager = await _empSvc.GetOperationManager(email.Email);
                if (operationManager.Email != null) {
                    await _empSvc.UpdateEmployeeOtp(operationManager.Id, otp);

                    string smsMessage = "";
                    smsMessage =
                        $"A ticket discount has been requested from your terminal by " + _serviceHelper.GetCurrentUserEmail() + ". Kindly use " + otp + " as your One Time Password";
                    try {


                        _smsSvc.SendSMSNow(smsMessage, recipient: operationManager.PhoneNumber.ToNigeriaMobile());
                        var mail = new Mail(appConfig.AppEmail, "Urgent: Ticket Discount!", operationManager.Email)
                        {
                            Body = "A ticket discount has been requested from your terminal by " + _serviceHelper.GetCurrentUserEmail() + ". Kindly use " + otp + " as your One Time Password\r\n\r\nSent from Transport"
                        };
                        _mailSvc.SendMail(mail);
                    }
                    catch {
                        //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
                    }

                }

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("sendmanifestotp")]
        public async Task<IServiceResponse<bool>> SendManifestOtp()
        {
            var otp = Guid.NewGuid().ToString().Remove(8).ToUpper();
            return await HandleApiOperationAsync(async () => {
                var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
                var operationManager = await _empSvc.GetOperationManager(email.Email);
                if (operationManager.Email != null) {
                    await _empSvc.UpdateEmployeeOtp(operationManager.Id, otp);

                    string smsMessage = "";
                    smsMessage =
                        $"A request to open manifest has been made from your terminal by " + _serviceHelper.GetCurrentUserEmail() + ". Kindly use " + otp + " as your One Time Password";
                    try {

                        _smsSvc.SendSMSNow(smsMessage, recipient: operationManager.PhoneNumber.ToNigeriaMobile());
                        var mail = new Mail(appConfig.AppEmail, "Urgent: Open Manifest!", operationManager.Email)
                        {
                            Body = "A request to open manifest has been made from your terminal by " + _serviceHelper.GetCurrentUserEmail() + ". Kindly use " + otp + " as your One Time Password\r\n\r\nSent from Transport"
                        };
                        _mailSvc.SendMail(mail);
                    }
                    catch {
                        //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
                    }

                }

                return new ServiceResponse<bool>();
            });
        }

        [HttpGet]
        [Route("verifyotp/{otp}")]
        public async Task<IServiceResponse<bool>> VerifyOtp(string otp)
        {

            return await HandleApiOperationAsync(async () => {
                var status = await _empSvc.Verifyotp(otp);
                return new ServiceResponse<bool>(status);
            });
        }

        [HttpPost]
        [Route("ProcessPaystackPayment")]
        public async Task<IServiceResponse<BookingResponseDTO>> ProcessPayStackPayment(PayStackResponseModel model)
        {
            return await HandleApiOperationAsync(async () => {
                var BookingDetails = await _bookingSvc.ProcessPaystackPayment(model.RefCode);
                return new ServiceResponse<BookingResponseDTO>(BookingDetails);
            });
        }

        [HttpPost]
        [Route("RescheduleTicketsearch")]
        public async Task<IServiceResponse<List<TicketRescheduleDTO>>> RescheduleTicketSearch(TicketRescheduleDTO ticketReschedule)
        {
            return await HandleApiOperationAsync(async () => {
                var availableTrips = await _bookingSvc.RescheduleTicketSearch(ticketReschedule);
                return new ServiceResponse<List<TicketRescheduleDTO>>(availableTrips);
            });
        }

        [HttpPost]
        [Route("RescheduleBooking")]
        public async Task<IServiceResponse<string>> Reschedulebooking(RescheduleDTO reschedule)
        {
            return await HandleApiOperationAsync(async () => {
                var status = await _bookingSvc.RescheduleBooking(reschedule);
                return new ServiceResponse<string>(status);
            });
        }

        [HttpPost]
        [Route("reroutebooking")]
        public async Task<IServiceResponse<string>> Reroutebooking(RescheduleDTO reroute)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var status = await _bookingSvc.Reroutebooking(reroute);
                return new ServiceResponse<string>(status);
            });
        }


        [HttpPost]
        [Route("RevalidatePaystack")]
        public async Task<IServiceResponse<BookingResponseDTO>> RevalidatePaystack(string RefCode)
        {
            var Refcode = RefCode;
            return await HandleApiOperationAsync(async () =>
            {
                var BookingDetails = await _bookingSvc.ProcessPaystackPayment(Refcode);
                return new ServiceResponse<BookingResponseDTO>(BookingDetails);
            });

        }

        [HttpPost]
        [Route("GetTraveledCustomers")]
        [Route("GetTraveledCustomers/{pageNumber}/{pageSize}")]
        [Route("GetTraveledCustomers/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<SeatManagementDTO>>> getTraveledCustomers(DateModel dateModel, int pageNumber = 1,
          int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var Discounts = await _bookingSvc.GetTraveledCustomersAsync(dateModel.StartDate, dateModel.EndDate, pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<SeatManagementDTO>>
                {
                    Object = Discounts
                };
            });
        }

        [HttpGet]
        [Route("GetAllTraveledCustomers/{startdate}")]
        public async Task<IServiceResponse<List<SeatManagementDTO>>> getAllTraveledCustomers(string startdate)
        {
            var date = Convert.ToDateTime(startdate);
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingSvc.GetTraveledCustomers(date.Date);
                return new ServiceResponse<List<SeatManagementDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("ProcessPayStackWebhook")]
        public async Task<IServiceResponse<BookingResponseDTO>> ProcessPayStackWebhook(PayStackResponseModel model)
        {
            var Refcode = model.RefCode;
            return await HandleApiOperationAsync(async () =>
            {
                var BookingDetails = await _bookingSvc.ProcessPaystackWebhook(Refcode);
                return new ServiceResponse<BookingResponseDTO>(BookingDetails);
            });

        }


        //[HttpGet]
        //[Route("verifymoveticketotp/{otp}")]
        //public async Task<IServiceResponse<bool>> VerifyMoveTicketOtp(string otp)
        //{

        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var status = await _service.VerifyMoveTicketOtp(otp);
        //        return new ServiceResponse<bool>(status);
        //    });
        //}


        //private static int GetRandomNumber(int min = 0100000, int max = 9999999)
        //{
        //    lock (getrandom)
        //    {
        //        return getrandom.Next(min, max);
        //    }
        //}

        //[HttpGet]
        //[Route("sendmoveticketotp")]
        //public async Task<IServiceResponse<bool>> SendMoveTicketOtp()
        //{

        //    var otp = GetRandomNumber().ToString();
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var operationManager = await _employeehelper.GetOperationManager(User.Identity.Name);
        //        var terminalaccountant = await _employeehelper.GetTerminalAccountant(User.Identity.Name);

        //        if (operationManager.Email != null && terminalaccountant.Email != null)
        //        {
        //            await _employeehelper.UpdateEmployeesRemoveTicketOtp(operationManager.EmployeeId, terminalaccountant.EmployeeId, otp);
        //            var otpreceipientMails = ConfigurationManager.AppSettings["SeatRemovalOTPreceipientEmails"];
        //            var otpreceipientNumbers = ConfigurationManager.AppSettings["SeatRemovalOTPreceipientnumbers"];

        //            var mailList = new List<string>();

        //            foreach (var mail in otpreceipientMails.Split(','))
        //            {
        //                mailList.Add(mail);
        //            }




        //            string smsMessage = "";
        //            smsMessage =
        //                $"A booking is about to be removed from a bus in your terminal by " + User.Identity.Name + ". Kindly use " + otp + " as your One Time Password";
        //            try
        //            {
        //                foreach (var number in otpreceipientNumbers.Split(','))
        //                {
        //                    _smsNotification.SendSmsNowAsync(smsMessage, new[] { number.NigerianMobile() }, "GIGM OTP");

        //                }

        //                if (mailList.Count() > 1)
        //                    _messagingFactory.SendMailMultiple(mailList, "Urgent: Booking Removal!", "A booking is about to be removed from a bus in your terminal by " + User.Identity.Name + ". Kindly use " + otp + " as your One Time Password\r\n\r\nSent from Transport");
        //                else
        //                    _messagingFactory.SendMail(otpreceipientMails, "Urgent: Booking Removal!", "A booking is about to be removed from a bus in your terminal by " + User.Identity.Name + ". Kindly use " + otp + " as your One Time Password\r\n\r\nSent from Libmot");


        //            }
        //            catch (Exception ex)
        //            {
        //                //Console.WriteLine("Error reading from {0}. Message = {1}", path, e.Message);
        //            }

        //        }



        //        return new ServiceResponse<bool>();
        //    });
        //}

        //[HttpGet]
        //[Route("todayticketbooking")]
        //public async Task<IServiceResponse<List<TicketBookingDto>>> TodayTicketBooking()
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.TodayTicketBooking();
        //        return new ServiceResponse<List<TicketBookingDto>>(availableTrips);
        //    });
        //}
        //[HttpGet]
        //[Route("pagedtodayticketbooking/{pageNumber}/{pageSize}")]
        //public async Task<IServiceResponse<PagedList<TicketBookingDto>>> PagedTodayTicketBooking(int pageNumber = 1, int pageSize = 0)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.PagedTodayTicketBooking(pageNumber, pageSize);
        //        //return new ServiceResponse<PagedList<TicketBookingDto>>(availableTrips);

        //        return new ServiceResponse<PagedList<TicketBookingDto>>
        //        {
        //            Object = availableTrips.QueryBuilder.OrderByDescendending(e => e.SeatManagementId).Execute()
        //        };
        //    });
        //}
        //[HttpGet]
        //[Route("pagedtomorrowticketbooking/{pageNumber}/{pageSize}")]//dn
        //public async Task<IServiceResponse<PagedList<TicketBookingDto>>> PagedTomorrowTicketBooking(int pageNumber = 1, int pageSize = 0)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.PagedTomorrowTicketBooking(pageNumber, pageSize);
        //        //return new ServiceResponse<PagedList<TicketBookingDto>>(availableTrips);

        //        return new ServiceResponse<PagedList<TicketBookingDto>>
        //        {
        //            Object = availableTrips.QueryBuilder.OrderByDescendending(e => e.SeatManagementId).Execute()
        //        };
        //    });
        //}
        //[HttpGet]
        //[Route("pagedtodayticketmainbookerbooking/{pageNumber}/{pageSize}")]
        //public async Task<IServiceResponse<PagedList<TicketBookingDto>>> PagedTodayTicketMainBookerBooking(int pageNumber = 1, int pageSize = 0)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.PagedTodayTicketMainBookerBooking(pageNumber, pageSize);
        //        //return new ServiceResponse<PagedList<TicketBookingDto>>(availableTrips);

        //        return new ServiceResponse<PagedList<TicketBookingDto>>
        //        {
        //            Object = availableTrips.QueryBuilder.OrderByDescendending(e => e.SeatManagementId).Execute()
        //        };
        //    });
        //}

        //[HttpGet]
        //[Route("PagedYesterdayTravelledTicketMainBookerBooking/{pageNumber}/{pageSize}")]//dn
        //public async Task<IServiceResponse<PagedList<TicketBookingDto>>> PagedYesterdayTravelledTicketMainBookerBooking(int pageNumber = 1, int pageSize = 0)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.PagedYesterdayTravelledTicketMainBookerBooking(pageNumber, pageSize);
        //        //return new ServiceResponse<PagedList<TicketBookingDto>>(availableTrips);

        //        return new ServiceResponse<PagedList<TicketBookingDto>>
        //        {
        //            Object = availableTrips.QueryBuilder.OrderByDescendending(e => e.SeatManagementId).Execute()
        //        };
        //    });
        //}

        //[HttpPost]
        //[Route("ticketbookingsearch")]
        //public async Task<IServiceResponse<List<TicketBookingDto>>> TicketBookingSearch(TicketBookingDto ticketbooking)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.TicketBookingSearch(ticketbooking);
        //        return new ServiceResponse<List<TicketBookingDto>>
        //        {
        //            Object = availableTrips.OrderByDescending(e => e.SeatManagementId).ToList()
        //        };
        //    });
        //}



        //[HttpPost]
        //[Route("nextdaybookingsearch")]
        //public async Task<IServiceResponse<List<TicketBookingDto>>> NextDayBookingSearch(TicketBookingDto ticketbooking)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.NextDayBookingSearch(ticketbooking);
        //        return new ServiceResponse<List<TicketBookingDto>>
        //        {
        //            Object = availableTrips.OrderByDescending(e => e.SeatManagementId).ToList()
        //        };
        //    });
        //}



        //[HttpPost]
        //[Route("mainbookerticketbookingsearch")]
        //public async Task<IServiceResponse<List<TicketBookingDto>>> MainBookerTicketBookingSearch(TicketBookingDto ticketbooking)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.MainBookerTicketBookingSearch(ticketbooking);
        //        //return new ServiceResponse<List<TicketBookingDto>>(availableTrips);
        //        return new ServiceResponse<List<TicketBookingDto>>
        //        {
        //            Object = availableTrips.OrderByDescending(e => e.SeatManagementId).ToList()
        //        };
        //    });
        //}
        //[HttpPost]
        //[Route("Vehiclebookingsearch")]
        //public async Task<IServiceResponse<List<Rescheduling>>> VehicleBookingSearch(Rescheduling bookedvehicle)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var availableTrips = await _service.VehicleBookingSearch(bookedvehicle);
        //        return new ServiceResponse<List<Rescheduling>>(availableTrips);
        //    });
        //}


        //[HttpPost]
        //[Route("CreateRescheduleBuses")]
        //public async Task<IServiceResponse<bool>> CreateRescheduleBuses(SeatManagementDto rescheduleinfo)
        //{

        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var status = await _service.CreateRescheduleBuses(rescheduleinfo);
        //        return new ServiceResponse<bool>(status);
        //    });
        //}

        //[HttpPost]
        //[Route("RescheduleBooking")]
        //public async Task<IServiceResponse<string>> reschedulebooking(Reschedule reschedule)
        //{

        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var status = await _service.reschedulebooking(reschedule);
        //        return new ServiceResponse<string>(status);
        //    });
        //}

        //[HttpPost]
        //[Route("allocateseat")]
        //public async Task<IServiceResponse<string>> AllocateSeat(Reschedule reschedule)
        //{

        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var status = await _service.allocateSeat(reschedule);
        //        return new ServiceResponse<string>(status);
        //    });
        //}




        //[HttpGet]
        //[Route("rescheduleinfosearch")]
        //public async Task<IServiceResponse<List<AvailabileTripDetail>>> RescheduleInfoSearch(SeatManagementDto reschedulerecord)
        //{


        //    {

        //        return await HandleApiOperationAsync(async () =>
        //        {
        //            var ticketerRoutes = await _service.RescheduleInfoSearch(reschedulerecord);
        //            return new ServiceResponse<List<AvailabileTripDetail>>
        //            {
        //                Object = ticketerRoutes
        //            };
        //        });

        //    }
        //}


        //[HttpPut]
        //[Route("Approveticket/{seatManagementId}")]
        //public async Task<IServiceResponse<bool>> ApproveTicket(long seatManagementId)
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        await _bookingSvc.ApproveBooking(seatManagementId);
        //        return new ServiceResponse<bool>();
        //    });
        //}
        //[HttpPost]
        //[Route("ProcessThirdPartyPayment")]
        //public async Task<IServiceResponse<BookingResponse>> ProcessThirdPartyPayment(ThirdPartyResponseModel thirdPartyResponseModel)
        //{

        //    //   var Refcode = model.RefCode;
        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.ProcessThirdPartyPayment(thirdPartyResponseModel);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}

        //[HttpPost]
        //[Route("ProcessPaystackPaymentForReschedule")]
        //public async Task<IServiceResponse<BookingResponse>> ProcessPaystackPaymentForReschedule(PayStackResponseModel model)
        //{
        //    var Refcode = model.RefCode;
        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.ProcessPaystackPaymentForReschedule(Refcode);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}

        //[HttpGet]
        //[Route("ProcessPaystackPaymentForReschedule")]
        //public async Task<IServiceResponse<BookingResponse>> ProcessPaystackPaymentForReschedule(string RefCode)
        //{
        //    var Refcode = RefCode;
        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.ProcessPaystackPaymentForReschedule(Refcode);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}



        //[HttpGet]
        //[Route("RevalidatePaystack/{RefCode}")]
        //public async Task<IServiceResponse<BookingResponse>> RevalidatePaystackWithRefcode(string RefCode)
        //{
        //    var Refcode = RefCode;
        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.ProcessPaystackPayment(Refcode);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}

        //[HttpPost]
        //public async Task<IServiceResponse<BookingResponse>> RevalidateGlobalPayByRefCode(string RefCode)
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var bookingDetails = await _service.ProcessGlobalPay(RefCode);

        //        return new ServiceResponse<BookingResponse>(bookingDetails);

        //    });
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("RevalidateAllPending")]
        //public async Task<IServiceResponse<BookingResponse>> RevalidateAllPending(int txnTime = -1)
        //{
        //    var time = txnTime;
        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.RevalidateAllPending(time);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}


        //[HttpGet]
        //[Route("RevalidateBooking/{RefCode}")]
        //public async Task<IServiceResponse<BookingResponse>> RevalidateBooking(string RefCode)
        //{

        //    return await HandleApiOperationAsync(async () => {
        //        var BookingDetails = await _service.RevalidateBooking(RefCode);
        //        return new ServiceResponse<BookingResponse>(BookingDetails);
        //    });

        //}



        //[HttpGet]
        //[Route("GetWebToken")]
        //public async Task<IServiceResponse<TokenResponse>> GetWebToken(string username, string password)
        //{
        //    // var time = txnTime;
        //    return await HandleApiOperationAsync(async () => {
        //        var TokenDetails = await _service.GetWebToken(username, password);
        //        return new ServiceResponse<TokenResponse>(TokenDetails);
        //    });

        //}

        //[HttpGet]
        //[Route("ActivePaymentGateway")]
        //public async Task<IServiceResponse<List<PaymentGatewayStatusDto>>> ActivePaymentGateway()
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var activePaymentGateways = await _service.ActivePaymentGateway();
        //        return new ServiceResponse<List<PaymentGatewayStatusDto>>(activePaymentGateways);
        //    });

        //}

        //[HttpPost]
        //[Route("getavailabletrip")]
        //public async Task<IServiceResponse<List<Reschedule>>> GetAvailableTrip(Reschedule reschedule)
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var activePaymentGateways = await _service.GetAvailableTrip(reschedule);
        //        return new ServiceResponse<List<Reschedule>>(activePaymentGateways);
        //    });

        //}


        //[HttpPost]
        //[Route("getavailabletripreschedule")]
        //public async Task<IServiceResponse<List<Reschedule>>> GetAvailableTripReschedule(Reschedule reschedule)
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var activePaymentGateways = await _service.GetAvailableTripReschedule(reschedule);
        //        return new ServiceResponse<List<Reschedule>>(activePaymentGateways);
        //    });

        //}
        ////AutoReleaseService
        //[HttpGet]
        //[Route("AutoReleaseSeat")]
        //public async Task<IServiceResponse<string>> AutoReleaseSeat(string RefCode)
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var seatRemoveResponse = await _service.AutoReleaseSeat(RefCode);
        //        return new ServiceResponse<string>(seatRemoveResponse);
        //    });

        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("TravelledGuestCountForDayBefore")]
        //public async Task<IServiceResponse<BookingCount>> TravelledGuestCountForDayBefore()
        //{
        //    return await HandleApiOperationAsync(async () => {
        //        var bookingCountResponse = await _service.TravelledGuestCountForDayBefore();
        //        return new ServiceResponse<BookingCount>(bookingCountResponse);
        //    });

        //}



    }
}