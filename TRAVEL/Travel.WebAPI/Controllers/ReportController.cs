using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    public class ReportController :  BaseController
    {
        private readonly IBookingReportService _bookingReportService;

        public ReportController(IBookingReportService bookingReportService)
        {
            _bookingReportService = bookingReportService;
        }
       
        [HttpPost]
        [Route("search")]
        public async  Task<IServiceResponse<List<BookingReportDto>>> BookingReports(BookingReportQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.GetBookingReport(queryDto);
                return new ServiceResponse<List<BookingReportDto>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("bookedtrips")]
        public async Task<IServiceResponse<List<BookedTripsDto>>> AllBookedTrips(BookedTripsQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.BookedTrips(queryDto);
                return new ServiceResponse<List<BookedTripsDto>>
                {
                    Object = reports.OrderByDescending(p => p.DepartureDate).ToList()
                };
            });
        }

        [HttpPost]
        [Route("bookedsales")]
        public async Task<IServiceResponse<List<BookingSalesReport>>> BookingSales(BookingSalesReportQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.BookingSalesReport(queryDto);
                return new ServiceResponse<List<BookingSalesReport>>
                {
                    Object = reports.OrderByDescending(p => p.Amount).ToList()
                };
            });
        }

        [HttpPost]
        [Route("passengerReport")]
        public async Task<IServiceResponse<List<PassengerReportDto>>> PassengerReport(PassengerReportQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.PassengerReport(queryDto);
                return new ServiceResponse<List<PassengerReportDto>>
                {
                    Object = reports.ToList()
                };
            });
        }

        [HttpPost]
        [Route("salesbybus")]
        public async Task<IServiceResponse<List<SalesPerBusDTO>>> SalesByBus(SalesByBusQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.SalesByBus(queryDto);
                return new ServiceResponse<List<SalesPerBusDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("driversalary")]
        public async Task<IServiceResponse<List<DriverSalaryReportModel>>> DriverSalary(SalaryReportQuery queryDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var reports = await _bookingReportService.DriverSalaryReport(queryDto);
                return new ServiceResponse<List<DriverSalaryReportModel>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("hiretrips")]

        public async Task<IServiceResponse<List<HireRequestDTO>>> HiredTrips(HiredTripRequestDTO requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.HiredTripReportAsync(requestDto);
                return new ServiceResponse<List<HireRequestDTO>>
                {
                    Object = reports
                };
            });
        }


        [HttpPost]
        [Route("journeyCharts")]

        public async Task<IServiceResponse<List<JourneyChartDto>>> JourneyCharts(JourneyChartQueryDto requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.JourneyChartReport(requestDto);
                return new ServiceResponse<List<JourneyChartDto>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("journeyChartsForStateTerminals")]

        public async Task<IServiceResponse<List<JourneyChartDto>>> JourneyChartsForStateTerminals(JourneyChartQueryDto requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.JourneyChartForStateTerminals(requestDto);
                return new ServiceResponse<List<JourneyChartDto>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("journeyChartsForTerminals")]

        public async Task<IServiceResponse<List<JourneyDetailDisplayDTO>>> JourneyChartsForTerminal(JourneyChartQueryDto requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.JourneyChartForTerminals(requestDto);
                return new ServiceResponse<List<JourneyDetailDisplayDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("totalSalesReportPerState")]

        public async Task<IServiceResponse<List<SalesReportDTO>>> TotalSalesReportByState(DateModel requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.TotalSalesPerState(requestDto);
                return new ServiceResponse<List<SalesReportDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("totalTerminalSalesInState")]

        public async Task<IServiceResponse<List<SalesReportDTO>>> TotalTerminalSalesInState(DateModel requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.TotalTerminalSalesInState(requestDto);
                return new ServiceResponse<List<SalesReportDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpPost]
        [Route("terminalSalesSummary")]

        public async Task<IServiceResponse<List<SalesReportDTO>>> TerminalSalesSummary(DateModel requestDto)
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.TotalTerminalSalesSummary(requestDto);
                return new ServiceResponse<List<SalesReportDTO>>
                {
                    Object = reports
                };
            });
        }

        [HttpGet]
        [Route("getsalessummary")]

        public async Task<IServiceResponse<SalesSummaryDTO>> SalesSummary()
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.GetSalesSummary();
                return new ServiceResponse<SalesSummaryDTO>
                {
                    Object = reports
                };
            });
        }
        [HttpGet]
        [Route("getbookingsummary")]

        public async Task<IServiceResponse<BookingSummaryDto>> BookingSummary()
        {
            return await HandleApiOperationAsync(async () => {
                var reports = await _bookingReportService.GetBookingSummary();
                return new ServiceResponse<BookingSummaryDto>
                {
                    Object = reports
                };
            });
        }
    }
}