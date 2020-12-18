using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class MtuReportController : BaseController
    {
        private readonly IVehicleService _vehicleService;
        private readonly IMtuReports _mtuReportService;
        public MtuReportController(IVehicleService vehicleService, IMtuReports mtuReportService)
        {
            _vehicleService = vehicleService;
            _mtuReportService = mtuReportService;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ServiceResponse<bool>> AddVehicle(MtuReportModelDTO mtuReport)
        {
            mtuReport.Email = CurrentUser.UserName;
            //mtuReport.FullName = 
            return await HandleApiOperationAsync(async () => {
                await _mtuReportService.AddReport(mtuReport);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPost]
        [Route("GetAllReport")]
        [Route("GetAllReport/{pageNumber}/{pageSize}")]
        [Route("GetAllReport/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<MtuReportModelDTO>>> GetAllReport(DateModel search, 
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, 
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                IPagedList<MtuReportModelDTO> vehicles;

                vehicles =  await _mtuReportService.GetAllReport(search, pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<MtuReportModelDTO>>
                {
                    Object = vehicles
                };
            });
        }
        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<MtuReportModelDTO>> GetReportById(int id)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var vehicles = await _mtuReportService.GetReportById(id);

                return new ServiceResponse<MtuReportModelDTO>
                {
                    Object = vehicles
                };
            });
        }

    }
}