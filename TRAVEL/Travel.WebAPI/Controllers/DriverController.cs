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
    public class DriverController : BaseController
    {
        readonly IDriverService _driverSvc;

        public DriverController(IDriverService driverSvc)
        {
            _driverSvc = driverSvc;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddDriver(DriverDTO driver)
        {
            return await HandleApiOperationAsync(async () => {
                await _driverSvc.AddDriver(driver);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<DriverDTO>>> GetDrivers(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string query = "")
        {
            return await HandleApiOperationAsync(async () => {

                var captains = await _driverSvc.GetDriversAsync(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<DriverDTO>>
                {
                    Object = captains
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<DriverDTO>> GetDriverById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var driver = await _driverSvc.GetDriverById(id);

                return new ServiceResponse<DriverDTO>
                {
                    Object = driver
                };
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateCaptain(int id, DriverDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                await _driverSvc.UpdateDriver(id, model);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteCaptain(int id)
        {
            return await HandleApiOperationAsync(async () => {
                await _driverSvc.RemoveDriver(id);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public async Task<IServiceResponse<DriverDTO>> GetDriverByCode(string code)
        {
            return await HandleApiOperationAsync(async () => {
                var captain = await _driverSvc.GetDriverByCodeAsync(code);

                return new ServiceResponse<DriverDTO>
                {
                    Object = captain
                };
            });
        }

        [HttpGet]
        [Route("GetDriverbyVehicleRegistrationNumber/{registrationNumber}")]
        public async Task<IServiceResponse<DriverDTO>> GetActiveCaptainByVehicle(string registrationNumber)
        {
            return await HandleApiOperationAsync(async () => {
                var captain = await _driverSvc.GetDriverByVehicleRegNum(registrationNumber);

                return new ServiceResponse<DriverDTO>
                {
                    Object = captain
                };
            });
        }

        [HttpGet]
        [Route("GetAvailableDriverAsync")]
        public async Task<IServiceResponse<List<DriverDTO>>> GetAvailableDriverAsync()
        {
            return await HandleApiOperationAsync(async () => {
                var drivers = await _driverSvc.GetAllAvailableDriversAsync();

                return new ServiceResponse<List<DriverDTO>>
                {
                    Object = drivers
                };
            });
        }

        [HttpGet]
        [Route("GetADriverByVehicleId/{id}")]
        public async Task<ServiceResponse<DriverDTO>> GetDriverByVehicleId(string id)
        {
            return await HandleApiOperationAsync(async () => {
                var driver = await _driverSvc.GetDriverByVehicleId(id);

                return new ServiceResponse<DriverDTO>
                {
                    Object = driver
                };
            });
        }
    }
}