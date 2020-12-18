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
    public class VehicleController : BaseController
    {
        private readonly IVehicleService _vehicleService;
        private readonly ITerminalService _terminalSvc;

        public VehicleController(IVehicleService vehicleService, ITerminalService terminalSvc)
        {
            _vehicleService = vehicleService;
            _terminalSvc = terminalSvc;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ServiceResponse<bool>> AddVehicle(VehicleDTO vehicle)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleService.AddVehicle(vehicle);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<ServiceResponse<IPagedList<VehicleDTO>>> GetVehicles(
            int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize,
            string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                IPagedList<VehicleDTO> vehicles;

                vehicles = await _vehicleService.GetVehicles(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<VehicleDTO>>
                {
                    Object = vehicles
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<VehicleDTO>> GetVehicleById(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var vehicle = await _vehicleService.GetVehicleById(id);

                return new ServiceResponse<VehicleDTO>
                {
                    Object = vehicle
                };
            });
        }

        [HttpGet]
        [Route("GetVehicleByIdWithDriverName/{id}")]
        public async Task<ServiceResponse<VehicleDTO>> GetVehicleByIdWithDriverName(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var vehicle = await _vehicleService.GetVehicleByIdWithDriverName(id);

                return new ServiceResponse<VehicleDTO>
                {
                    Object = vehicle
                };
            });
        }

        [HttpDelete]
        [Route("Delete/{vehicleId}")]
        public async Task<IServiceResponse<bool>> DeleteVehicle(int vehicleId)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleService.RemoveVehicle(vehicleId);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateVehicle(int id, VehicleDTO vehicle)
        {
            return await HandleApiOperationAsync(async () => {
                await _vehicleService.UpdateVehicle(id, vehicle);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("UpdateVehicleAssignments/{id}")]
        public async Task<ServiceResponse<bool>> UpdateVehicleAssignments(int id, VehicleAssignmentDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _vehicleService.UpdateVehicleAssignments(id, model);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("GetAvailableVehicles")]
        public async Task<ServiceResponse<List<VehicleDTO>>> GetAvailableVehicles()
        {
            return await HandleApiOperationAsync(async () => {
                var tripvehicles = await _vehicleService.GetAvailableVehicles();

                return new ServiceResponse<List<VehicleDTO>>
                {
                    Object = tripvehicles
                };
            });
        }

        [HttpGet]
        [Route("GetAvailableVehiclesByTerminal/{id}")]
        public async Task<ServiceResponse<List<VehicleDTO>>> GetAvailableVehiclesByTerminal(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var tripvehicles = await _vehicleService.GetAvailableVehiclesByTerminal(id);

                return new ServiceResponse<List<VehicleDTO>>
                {
                    Object = tripvehicles
                };
            });
        }

        [HttpGet]
        [Route("GetByRegNumber/{regNum}")]
        public async Task<ServiceResponse<VehicleDTO>> GetVehiclesByRegNum(string regNum)
        {
            return await HandleApiOperationAsync(async () => {
                var vehicle = await _vehicleService.GetVehiclesByRegNum(regNum);

                return new ServiceResponse<VehicleDTO>
                {
                    Object = vehicle
                };
            });
        }

        [HttpGet]
        [Route("GetAvailableVehiclesInTerminal")]
        public async Task<IServiceResponse<List<VehicleDTO>>> GetAvailableVehiclesInTerminal()
        {
            return await HandleApiOperationAsync(async () => {
                var vehicles = await _vehicleService.GetAvailableVehiclesInTerminal();

                return new ServiceResponse<List<VehicleDTO>>
                {
                    Object = vehicles
                };
            });
        }

        //Create Header/Detail for vehicle allocation - working
        //[HttpGet]
        //[Route("GetVehiclesByTerminalHeader/{LocationId}")]
        //public async Task<ServiceResponse<List<VehicleDTO>>> GetVehiclesByTerminalHeader(int LocationId)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        List<VehicleDTO> vehicles;
        //        vehicles = await _vehicleService.GetVehiclesByTerminalHeader(LocationId);

        //        return new ServiceResponse<List<VehicleDTO>>
        //        {
        //            Object = vehicles
        //        };
        //    });
        //}

        [HttpGet]
        [Route("GetVehiclesByTerminalHeader/{LocationId}")]
        public async Task<ServiceResponse<IPagedList<VehicleDTO>>> GetVehiclesByTerminalHeader(int LocationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                IPagedList<VehicleDTO> vehicle;

                vehicle = await _vehicleService.GetVehiclesByTerminalHeaderb(LocationId);

                return new ServiceResponse<IPagedList<VehicleDTO>>
                {
                    Object = vehicle
                };
            });
        }


        [HttpGet]
        [Route("GetTerminalHeader")]
        [Route("GetTerminalHeader/{pageNumber}/{pageSize}")]
        [Route("GetTerminalHeader/{pageNumber}/{pageSize}/{query}")]
       public async Task<ServiceResponse<IPagedList<TerminalDTO>>> GetTerminalHeader(
       int pageNumber = 1,
       int pageSize = WebConstants.DefaultPageSize,
       string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                IPagedList<TerminalDTO> terminals;

                //terminals = await _terminalSvc.GetTerminals(pageNumber, pageSize, query);
                terminals = await _vehicleService.GetTerminalHeader(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<TerminalDTO>>
                {
                    Object = terminals
                };
            });
        }


        //Create drop down list for vehicle
        [HttpGet]
        [Route("GetVehicleslist")]
        public async Task<ServiceResponse<List<VehicleDTO>>> GetVehicleslist()
        {
            return await HandleApiOperationAsync(async () => {
                List<VehicleDTO> vehicles;
                vehicles = await _vehicleService.GetVehicleslist();

                return new ServiceResponse<List<VehicleDTO>>
                {
                    Object = vehicles
                };
            });
        }


        //Create function that will allocate the buses
        //[HttpPost]
        //[Route("allocatebuses")]
        //public async Task<IServiceResponse<List<VehicleDTO>>> AllocateBuses(VehicleAllocationDTO queryDto)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var buses = await _vehicleService.AllocateBuses(queryDto);
        //        return new ServiceResponse<List<VehicleDTO>>
        //        {
        //            Object = buses
        //        };
        //    });
        //}


        [HttpPut]
        [Route("allocatebuses")]
        public async Task<ServiceResponse<bool>> AllocateBuses(VehicleAllocationDTO queryDto)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _vehicleService.AllocateBuses(queryDto);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("GetTerminalRemainingVehicle/{terminalId}")]
        public async Task<IServiceResponse<List<VehicleDTO>>> GetAllRemainingVehicleInTerminal(int terminalId)
        {
            return await HandleApiOperationAsync(async () => {
                var vehicles = await _vehicleService.VehiclesRemainingInTerminal(terminalId);

                return new ServiceResponse<List<VehicleDTO>>
                {
                    Object = vehicles
                };
            });
        }

        [HttpGet]
        [Route("confirm/{vehicleId}")]
        public async Task<ServiceResponse<bool>> VehicleConfirmation(int vehicleId)
        {
            return await HandleApiOperationAsync(async () => {
                var result = _vehicleService.AllocateVehicleConfirmation(vehicleId);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpDelete]
        [Route("deletefromvehicleallocationtable")]
        public async Task<IServiceResponse<bool>> deletefromvehicleallocationtable()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _vehicleService.DeleteFromVehicleAllocationTable();

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }


    }
}