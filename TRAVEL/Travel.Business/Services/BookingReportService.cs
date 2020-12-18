using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Timing;
using Travel.Data.efCore.Context;
using Travel.Data.UnitOfWork;
using Travel.Data.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Business.Services
{
    public interface IBookingReportService
    {
        Task<List<BookingReportDto>> GetBookingReport(BookingReportQueryDto queryDto);
        Task<List<BookedTripsDto>> BookedTrips(BookedTripsQueryDto queryDto);
        Task<List<BookingSalesReport>> BookingSalesReport(BookingSalesReportQueryDto queryDto);
        Task<List<PassengerReportDto>> PassengerReport(PassengerReportQueryDto queryDto);
        Task<List<SalesPerBusDTO>> SalesByBus(SalesByBusQueryDto queryDto);
        Task<List<DriverSalaryReportModel>> DriverSalaryReport(SalaryReportQuery queryDto);
        Task<List<HireRequestDTO>> HiredTripReportAsync(HiredTripRequestDTO queryDto);
        Task<List<JourneyChartDto>> JourneyChartReport(JourneyChartQueryDto queryDto);
        Task<List<JourneyChartDto>> JourneyChartForStateTerminals(JourneyChartQueryDto queryDto);
        Task<SalesSummaryDTO> GetSalesSummary();
        Task<BookingSummaryDto> GetBookingSummary();
        Task<List<JourneyDetailDisplayDTO>> JourneyChartForTerminals(JourneyChartQueryDto details);
        Task<List<SalesReportDTO>> TotalSalesPerState(DateModel date);
        Task<List<SalesReportDTO>> TotalTerminalSalesInState(DateModel date);
        Task<List<SalesReportDTO>> TotalTerminalSalesSummary(DateModel date);
    }
    public class BookingReportService : IBookingReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISeatManagementService _seatManagementService;


        public BookingReportService(IUnitOfWork unitOfWork, ISeatManagementService seatManagementService) 
        {
            _unitOfWork = unitOfWork;
            _seatManagementService = seatManagementService;
        }
        public async Task<List<BookingReportDto>> GetBookingReport(BookingReportQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<BookingReportDto>(@"Exec Sp_BookingReport",
                                                                queryDto.TerminalId, queryDto.Keyword, queryDto.BookingType, queryDto.BookingStatus,  queryDto.CreatedBy , queryDto.ReferenceCode, 
                                                                queryDto.StartDate, queryDto.EndDate, queryDto.PageIndex, queryDto.PageSize);

            return reports.ToList();
        }
        public async Task<List<BookedTripsDto>> BookedTrips(BookedTripsQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }
            queryDto.PhysicalBusRegisterationNumber = string.IsNullOrEmpty(queryDto.PhysicalBusRegisterationNumber) ? null : queryDto.PhysicalBusRegisterationNumber;
            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<BookedTripsDto>(@"Exec Sp_BookedBusesReports", queryDto.BookingType, 
                                                             queryDto.PhysicalBusRegisterationNumber, queryDto.StartDate, queryDto.EndDate);

            return reports.ToList();
        }

        public async Task<List<BookingSalesReport>> BookingSalesReport(BookingSalesReportQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }
            queryDto.TerminalId = queryDto.TerminalId == 0 ? null : queryDto.TerminalId;
            queryDto.PaymentMethod = queryDto.PaymentMethod == 0 ? null : queryDto.PaymentMethod;
            queryDto.StateId = queryDto.StateId == 0 ? null : queryDto.StateId;
            queryDto.RouteId = queryDto.RouteId == 0 ? null : queryDto.RouteId;

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<BookingSalesReport>(@"Exec Sp_Salesreport", 
                queryDto.RouteId, queryDto.TerminalId,queryDto.StateId, queryDto.PaymentMethod, queryDto.CreatedBy, queryDto.StartDate, queryDto.EndDate);

            return reports.ToList();
        }

        public async Task<List<PassengerReportDto>> PassengerReport(PassengerReportQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<PassengerReportDto>(@"Exec Sp_PassengerReport",
                queryDto.Keyword, queryDto.StartDate, queryDto.EndDate);

            return reports.ToList();
        }
        public async Task<List<SalesPerBusDTO>> SalesByBus(SalesByBusQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<SalesPerBusDTO>(@"Exec Sp_SalesByBus",
               queryDto.StartDate, queryDto.EndDate);

            return reports.ToList();
        }
        public async Task<List<DriverSalaryReportModel>> DriverSalaryReport(SalaryReportQuery queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;
            }

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<DriverSalaryReportModel>(@"Exec Sp_DriverSalary", queryDto.DriverCode,
               queryDto.StartDate, queryDto.EndDate);

            return reports.ToList();
        }
        public async Task<List<HireRequestDTO>> HiredTripReportAsync(HiredTripRequestDTO queryDto)
        {
            if (queryDto.StartDate == null) {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null) {
                queryDto.EndDate = Clock.Now;
            }

            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<HireRequestDTO>(@"Exec Sp_HiredRequests", queryDto.Keyword, queryDto.StartDate, queryDto.EndDate
                ,queryDto.PageIndex, queryDto.PageSize
             );

            return reports.ToList();

        }
        public async Task<List<JourneyChartDto>> JourneyChartReport(JourneyChartQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;

            }

            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<JourneyChartDto>(@"Exec Sp_JourneyCharts",  queryDto.StartDate, queryDto.EndDate
             );

            return reports.ToList();
        }

        public async Task<List<JourneyChartDto>> JourneyChartForStateTerminals(JourneyChartQueryDto queryDto)
        {
            if (queryDto.StartDate == null)
            {
                queryDto.StartDate = Clock.Now.Date;
            }
            if (queryDto.EndDate == null)
            {
                queryDto.EndDate = Clock.Now;

            }

            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<JourneyChartDto>(@"Exec Sp_JourneyChartsByState", queryDto.StartDate, queryDto.EndDate, queryDto.StateId
             );

            return reports.ToList();
        }

        public async Task<List<JourneyDetailDisplayDTO>> JourneyChartForTerminals(JourneyChartQueryDto details)
        {
            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<JourneyDetailDisplayDTO>(@"Exec Sp_JourneyChartsByTerminalDetails", details.StartDate,
             details.EndDate, details.DepartureTerminalId, details.DestinationTerminalId, 
             details.JourneyStatus, details.JourneyType);

            return reports.ToList();
        }

        public async Task<List<SalesReportDTO>> TotalSalesPerState(DateModel date)
        {
            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<SalesReportDTO>(@"Exec sp_SalesInStates", date.StartDate,
             date.EndDate, date.Id);

            return reports.ToList();
        }


        public  Task<SalesSummaryDTO> GetSalesSummary()
        {
            var today = DateTime.Now.Date;
            var currentDate = DateTime.Now;

            var todaysSales = _seatManagementService.GetAll().Where(p => p.BookingStatus == BookingStatus.Approved
                    && (p.CreationTime > today && p.CreationTime < currentDate))?.Sum(p => p.Amount);
            var yesterdaysSales = _seatManagementService.GetAll().Where(p => p.BookingStatus == BookingStatus.Approved
                    && (p.CreationTime > today.AddDays(-1) && p.CreationTime < currentDate.AddDays(-1)))?.Sum(p => p.Amount);
            var todaysBookings = _seatManagementService.GetAll().Where(p => p.BookingStatus == BookingStatus.Approved
                     && (p.CreationTime > today && p.CreationTime < currentDate))?.Count();
            var summary =  new SalesSummaryDTO
            {
                TodaysSales = todaysSales,
                LastSales = yesterdaysSales,
                TodaysBookings = todaysBookings
            };
           return Task.FromResult(summary);
        }
        public Task<BookingSummaryDto> GetBookingSummary()
        {
            var today = DateTime.Now.Date;
            var currentDate = DateTime.Now;

            var onlineBookings = _seatManagementService.GetAll().Where(p => p.BookingType == BookingTypes.Online
                    && (p.CreationTime > today && p.CreationTime < currentDate))?.Count();
            var terminalBookings = _seatManagementService.GetAll().Where(p => p.BookingType == BookingTypes.Terminal
                    && (p.CreationTime > today && p.CreationTime < currentDate))?.Count();
            var advancedBooking = _seatManagementService.GetAll().Where(p => p.BookingType == BookingTypes.Advanced
                 && (p.CreationTime > today && p.CreationTime < currentDate))?.Count();

            var summary = new BookingSummaryDto
            {
                OnlineChannelCount = onlineBookings,
                TerminalBookingCount = terminalBookings,
                AdvancedBookingCount = advancedBooking
            };
            return Task.FromResult(summary);
        }

        public async Task<List<SalesReportDTO>> TotalTerminalSalesInState(DateModel date)
        {
            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<SalesReportDTO>(@"Exec sp_SalesByTerminalInState", date.StartDate, date.EndDate, date.Id);

            return reports.ToList();
        }

        public async Task<List<SalesReportDTO>> TotalTerminalSalesSummary(DateModel date)
        {
            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<SalesReportDTO>(@"Exec sp_SalesInTerminal", date.StartDate, date.EndDate, date.BookingType, date.Id);

            return reports.ToList();
        }
    } 
}
