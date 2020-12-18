using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Data.efCore.Context;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Travel.Data.Utils.Extensions;
using Travel.Core.Messaging.Sms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Travel.Business.Services
{
    public interface IVehicleService
    {
        Task AddVehicle(VehicleDTO vehicle);
        Task<IPagedList<VehicleDTO>> GetVehicles(int pageNumber, int pageSize, string searchTerm);
        Task<VehicleDTO> GetVehicleById(int vehicleId);
        Task<bool> UpdateVehicleAssignments(int vehicleId, VehicleAssignmentDTO model);
        Task<List<VehicleDTO>> GetAvailableVehicles();
        Task<List<VehicleDTO>> GetAvailableVehiclesByTerminal(int terminalId);
        Task<VehicleDTO> GetVehiclesByRegNum(string regNum);
        Task RemoveVehicle(int vehicleId);
        Task UpdateVehicle(int id, VehicleDTO vehicle);
        Task<List<VehicleDTO>> GetAvailableVehiclesInTerminal();

        Task<List<VehicleDTO>> GetVehiclesByTerminalHeader(int LocationId);
        Task<IPagedList<VehicleDTO>> GetVehiclesByTerminalHeaderb(int LocationId);
        Task<IPagedList<TerminalDTO>> GetTerminalHeader(int pageNumber, int pageSize, string searchTerm);
        Task<List<VehicleDTO>> GetVehicleslist();
        Task<bool> AllocateBuses(VehicleAllocationDTO queryDto);
        Task<VehicleDTO> GetVehicleByIdWithDriverName(int id);
        Task<List<VehicleDTO>> VehiclesRemainingInTerminal(int terminalId);

        bool AllocateVehicleConfirmation(int vehicleId);
        Task<bool> DeleteFromVehicleAllocationTable();
    }

    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IRepository<VehicleTripRegistration, Guid> _vhclTrip;
        private readonly IDriverService _driversvc;
        private readonly IRepository<VehiclePart> _vehiclepartRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        private readonly IRepository<State> _stateRepo;
        private readonly IFranchizeService _franchizeService;
        private readonly ISMSService _smsSvc;
        private readonly IUserService _userSvc;
        private readonly IRepository<Manifest, Guid> _manifestRepo;
        private readonly IRepository<VehicleAllocationDetailModel> _vehicleAlloRepo;
        private readonly IEmployeeService _employeeSvc;
        private readonly IRepository<VehicleAllocationDetailModel> _vehAllocDet;

        public VehicleService(IRepository<Vehicle> vehicleRepo, IDriverService driversvc, IRepository<VehicleAllocationDetailModel> vehicleAlloRepo, IFranchizeService franchizeService,
            IRepository<VehiclePart> vehiclepartRepo, IRepository<VehicleModel> vehicleModelRepo,
            IRepository<Terminal> terminalRepo, IRepository<Employee> employeeRepo,
            IUnitOfWork unitOfWork, IServiceHelper serviceHelper, IRepository<State> stateRepo, ISMSService smsSvc, IRepository<Manifest, Guid> manifestRepo,
            IRepository<VehicleTripRegistration, Guid> vhclTrip, IUserService userSvc, IEmployeeService employeeSvc,
            IRepository<VehicleAllocationDetailModel> vehAllocDet)
        {
            _vehicleRepo = vehicleRepo;
            _driversvc = driversvc;
            _vehiclepartRepo = vehiclepartRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _terminalRepo = terminalRepo;
            _employeeRepo = employeeRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
            _stateRepo = stateRepo;
            _franchizeService = franchizeService;
            _smsSvc = smsSvc;
            _manifestRepo = manifestRepo;
            _vhclTrip = vhclTrip;
            _employeeSvc = employeeSvc;
            _vehicleAlloRepo = vehicleAlloRepo;
            _userSvc = userSvc;
            _vehAllocDet = vehAllocDet;
        }

        private IQueryable<VehicleDTO> _QueryVehicles()
        {
            var vehicles =
                                from vehicle in _vehicleRepo.GetAll()

                                join vehiclemodel in _vehicleModelRepo.GetAll() on vehicle.VehicleModelId equals vehiclemodel.Id

                                join vehicleLocation in _terminalRepo.GetAll() on vehicle.LocationId equals vehicleLocation.Id
                                into vehilceLocations
                                from vehilceLocation in vehilceLocations.DefaultIfEmpty()


                                join driver in _driversvc.GetAll() on vehicle.DriverId equals driver.Id                              
                                into drivers
                                from driver in drivers.DefaultIfEmpty()

                                join franchise in _franchizeService.GetAll() on vehicle.FranchizeId equals franchise.Id
                                into franchises
                                from franchise in franchises.DefaultIfEmpty()

                                orderby vehicle.CreationTime descending
                                select new VehicleDTO
                                {
                                    Details = vehicle.RegistrationNumber + " ( " + vehicle.Status + (vehicle.Status == VehicleStatus.Working ? " " : " IN " + vehilceLocation.Name) + " )",
                                    Id = vehicle.Id,
                                    RegistrationNumber = vehicle.RegistrationNumber,
                                    ChasisNumber = vehicle.ChasisNumber,
                                    EngineNumber = vehicle.EngineNumber,
                                    IMEINumber = vehicle.IMEINumber,
                                    Type = vehicle.Type,
                                    VehicleStatus = vehicle.Status,
                                    DateCreated = vehicle.CreationTime,
                                    LocationId = vehilceLocation.Id,
                                    LocationName = vehilceLocation.Name,
                                    VehicleModelId = vehiclemodel.Id,
                                    VehicleModelName = vehiclemodel.Name,
                                    IsOperational = vehicle.IsOperational,
                                    //DriverId = driver.Id,
                                    //DriverName = driver.Name,
                                    FranchizeId = franchise.Id,
                                    FranchiseName = franchise.FirstName
                                };
            return vehicles;
        }

        public Task<IPagedList<VehicleDTO>> GetVehicles(int pageNumber, int pageSize, string searchTerm)
        {
            var vehicles = _QueryVehicles();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                vehicles = vehicles.Where(
                                v => v.RegistrationNumber.Contains(searchTerm)
                                || v.Type.Contains(searchTerm)
                                || v.EngineNumber.Contains(searchTerm)
                                || v.ChasisNumber.Contains(searchTerm)
                                || v.LocationName.Contains(searchTerm));
            }

            return vehicles.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        private async Task<bool> IsValidVehicleModel(int vehicleModelId)
        {
            return vehicleModelId > 0 &&
            await _vehicleModelRepo.ExistAsync(m => m.Id == vehicleModelId);
        }

        private async Task<bool> IsValidDriver(int driverId)
        {
            return driverId > 0 &&
            await _driversvc.ExistAsync(m => m.Id == driverId);
        }

        public async Task AddVehicle(VehicleDTO vehicleDto)
        {

            if (!await IsValidVehicleModel(vehicleDto.VehicleModelId))
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            if (vehicleDto.DriverId != null && !await IsValidDriver(vehicleDto.DriverId.Value))
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }

            // new vehicles are operational
            vehicleDto.IsOperational = true;

            vehicleDto.RegistrationNumber = vehicleDto.RegistrationNumber.Trim();

            var vehicleRegNumber = vehicleDto.RegistrationNumber.ToLower();

            if (await _vehicleRepo.ExistAsync(v => v.RegistrationNumber.ToLower() == vehicleRegNumber))
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_EXIST);
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = vehicleDto.RegistrationNumber,
                ChasisNumber = vehicleDto.ChasisNumber,
                EngineNumber = vehicleDto.EngineNumber,
                IMEINumber = vehicleDto.IMEINumber,
                Type = vehicleDto.Type,
                Status = vehicleDto.VehicleStatus,
                VehicleModelId = vehicleDto.VehicleModelId,
                LocationId = vehicleDto.LocationId,
                IsOperational = vehicleDto.IsOperational,
                DriverId = vehicleDto.DriverId,
                FranchizeId = vehicleDto.FranchizeId
            };

            _vehicleRepo.Insert(vehicle);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<VehicleDTO> GetVehicleById(int id)
        {
            var vehicle = await _vehicleRepo.GetAsync(id);

            if (vehicle == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_EXIST);
            }

            return new VehicleDTO
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                ChasisNumber = vehicle.ChasisNumber,
                EngineNumber = vehicle.EngineNumber,
                IMEINumber = vehicle.IMEINumber,
                Type = vehicle.Type,
                VehicleStatus = vehicle.Status,
                DateCreated = vehicle.CreationTime,
                IsOperational = vehicle.IsOperational,
                DriverId = vehicle.DriverId,
                VehicleModelId = vehicle.VehicleModelId,
                LocationId = vehicle.LocationId,
                FranchizeId = vehicle.FranchizeId
            };
        }

        public Task<VehicleDTO> GetVehicleByIdWithDriverName(int id)
        {
            var vehicles = from vehicle in _vehicleRepo.GetAll()
                           join driver in _driversvc.GetAll() on vehicle.DriverId equals driver.Id
                           where vehicle.Id == id

                           select new VehicleDTO
                           {
                               Id = vehicle.Id,
                               RegistrationNumber = vehicle.RegistrationNumber,
                               ChasisNumber = vehicle.ChasisNumber,
                               EngineNumber = vehicle.EngineNumber,
                               IMEINumber = vehicle.IMEINumber,
                               Type = vehicle.Type,
                               VehicleStatus = vehicle.Status,
                               DateCreated = vehicle.CreationTime,
                               IsOperational = vehicle.IsOperational,
                               DriverId = vehicle.DriverId,
                               VehicleModelId = vehicle.VehicleModelId,
                               LocationId = vehicle.LocationId,
                               FranchizeId = vehicle.FranchizeId,
                               DriverName = driver.Name,
                               DriverCode = driver.Code
                           };

            return vehicles.AsNoTracking().FirstOrDefaultAsync();

        }


        public async Task<bool> UpdateVehicleAssignments(int id, VehicleAssignmentDTO model)
        {
            if (model is null)
                return false;

            var vehicle = await _vehicleRepo.GetAsync(id);

            if (vehicle == null)
                return false;

            var driver = (await _driversvc.FirstOrDefaultAsync(c => c.Code == model.PryDriver));


            if (driver == null)
            {
                throw new LMEGenericException($"Driver ({model.PryDriver}) not found");
            }

            Driver handOverDriver = null;

            if (!string.IsNullOrEmpty(model.NewHandOverDriverCode))
            {
                handOverDriver = (await _driversvc.FirstOrDefaultAsync(c => c.Code == model.NewHandOverDriverCode));
                if (handOverDriver == null)
                {
                    throw new LMEGenericException($"Hand over driver ({model.NewHandOverDriverCode}) not found");
                }
            }

            // Set primary captain  vehicle/ but first take away current captain from this vehicle
            if (driver.VehicleRegistrationNumber != vehicle.RegistrationNumber)
            {
                var currAssignedCaptains = await _driversvc.GetDriverByBusNumberAsync(vehicle.RegistrationNumber);

                if (currAssignedCaptains != null && currAssignedCaptains.Code != model.PryDriver)
                {
                    var curPry = await _driversvc.GetAsync(currAssignedCaptains.Id);
                    if (curPry == null)
                        throw new LMEGenericException($"Unable to load previous owner of vehicle({vehicle.RegistrationNumber}) for deassignement");

                    curPry.VehicleRegistrationNumber = "";
                }
            }

            driver.VehicleRegistrationNumber = vehicle.RegistrationNumber;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public Task<List<VehicleDTO>> GetAvailableVehicles()
        {
            var vehicles =
               from vehicle in _vehicleRepo.GetAll()

               join vehiclemodel in _vehicleModelRepo.GetAll() on vehicle.VehicleModelId equals vehiclemodel.Id

               join vehicleLocation in _terminalRepo.GetAll() on vehicle.LocationId equals vehicleLocation.Id
               into vehilceLocations
               from vehilceLocation in vehilceLocations.DefaultIfEmpty()

               where vehicle.Status == VehicleStatus.Idle
               select new VehicleDTO
               {
                   Details = vehicle.RegistrationNumber + " ( " + vehicle.Status.ToString() + (vehicle.Status == VehicleStatus.Working ? " " : " IN " + vehilceLocation.Name) + " )",
                   Id = vehicle.Id,
                   RegistrationNumber = vehicle.RegistrationNumber,
                   ChasisNumber = vehicle.ChasisNumber,
                   EngineNumber = vehicle.EngineNumber,
                   IMEINumber = vehicle.IMEINumber,
                   Type = vehicle.Type,
                   LocationId = vehilceLocation.Id,
                   LocationName = vehilceLocation.Name,
                   VehicleModelId = vehiclemodel.Id,
                   VehicleModelName = vehiclemodel.Name,
                   IsOperational = vehicle.IsOperational

               };

            return vehicles.AsNoTracking().ToListAsync();
        }


        public Task<List<VehicleDTO>> GetAvailableVehiclesByTerminal(int terminalId)
        {
            var vehicles =
                from vehicle in _vehicleRepo.GetAll()
                join driver in _driversvc.GetAll() on vehicle.DriverId equals driver.Id
                join vehiclemodel in _vehicleModelRepo.GetAll() on vehicle.VehicleModelId equals vehiclemodel.Id

                join vehicleLocation in _terminalRepo.GetAll() on vehicle.LocationId equals vehicleLocation.Id
                into vehilceLocations  from vehilceLocation in vehilceLocations.DefaultIfEmpty() 
                

                where vehicle.LocationId == terminalId && (vehicle.Status == VehicleStatus.Idle || vehicle.Status == VehicleStatus.TerminalUse)
                select new VehicleDTO
                {
                    Details = vehicle.RegistrationNumber + " ( " + vehicle.Status + (vehicle.Status == VehicleStatus.Working ? " " : " IN " + vehilceLocation.Name) + " )",
                    Id = vehicle.Id,
                    RegistrationNumber = vehicle.RegistrationNumber,
                    ChasisNumber = vehicle.ChasisNumber,
                    EngineNumber = vehicle.EngineNumber,
                    IMEINumber = vehicle.IMEINumber,
                    Type = vehicle.Type,
                    LocationId = vehilceLocation.Id,
                    LocationName = vehilceLocation.Name,
                    VehicleModelId = vehiclemodel.Id,
                    VehicleModelName = vehiclemodel.Name,
                    IsOperational = vehicle.IsOperational,
                    DriverName = driver.Name,
                    DriverNo = driver.Phone1
                };

            return vehicles.AsNoTracking().ToListAsync();
        }


        public Task<List<VehicleDTO>> GetAvailableVehiclesInTerminal(int terminalId)
        {
            var vehicles =
                from vehicle in _vehicleRepo.GetAll()

                join driver in _driversvc.GetAll() on vehicle.RegistrationNumber equals driver.VehicleRegistrationNumber
                into drivers
                from driver in drivers.DefaultIfEmpty()

                join vehiclemodel in _vehicleModelRepo.GetAll() on vehicle.VehicleModelId equals vehiclemodel.Id

                join vehicleLocation in _terminalRepo.GetAll() on vehicle.LocationId equals vehicleLocation.Id
               into vehilceLocations

                from vehilceLocation in vehilceLocations.DefaultIfEmpty()

                where vehicle.LocationId == terminalId && (vehicle.Status == VehicleStatus.Idle || vehicle.Status == VehicleStatus.TerminalUse)
                select new VehicleDTO
                {
                    Details = vehicle.RegistrationNumber + " ( " + vehicle.Status.ToString() + (vehicle.Status == VehicleStatus.Working ? " " : " IN " + vehilceLocation.Name) + " )",
                    Id = vehicle.Id,
                    RegistrationNumber = vehicle.RegistrationNumber,
                    ChasisNumber = vehicle.ChasisNumber,
                    EngineNumber = vehicle.EngineNumber,
                    IMEINumber = vehicle.IMEINumber,
                    Type = vehicle.Type,
                    LocationId = vehilceLocation.Id,
                    LocationName = vehilceLocation.Name,
                    VehicleModelId = vehiclemodel.Id,
                    VehicleModelName = vehiclemodel.Name,
                    IsOperational = vehicle.IsOperational,
                    DriverName = driver.Name,
                    DriverCode = driver.Code
                };

            return vehicles.AsNoTracking().ToListAsync();
        }


        public Task<VehicleDTO> GetVehiclesByRegNum(string regNum)
        {
            var vehicles =
                 from vehicle in _vehicleRepo.GetAll()
                 join vehicleLocation in _terminalRepo.GetAll() on vehicle.LocationId equals vehicleLocation.Id

            into vehilceLocations
                 from vehilceLocation in vehilceLocations.DefaultIfEmpty()
                 where vehicle.RegistrationNumber == regNum

                 select new VehicleDTO
                 {
                     Details = vehicle.RegistrationNumber + " ( " + vehicle.Status + (vehicle.Status == VehicleStatus.Working ? " " : " IN " + vehilceLocation.Name) + " )",
                     Id = vehicle.Id,
                     RegistrationNumber = vehicle.RegistrationNumber,
                     VehicleModelId = vehicle.VehicleModelId,
                     IMEINumber = vehicle.IMEINumber,
                     Type = vehicle.Type,
                     LocationId = vehilceLocation.Id,
                     LocationName = vehilceLocation.Name,
                     DateCreated = vehicle.CreationTime,
                     VehicleStatus = vehicle.Status,
                     IsOperational = vehicle.IsOperational,
                 };

            return vehicles.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task RemoveVehicle(int vehicleId)
        {
            var vehicle = await _vehicleRepo.GetAsync(vehicleId);

            if (vehicle == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_NOT_EXIST);
            }

            _vehicleRepo.Delete(vehicle);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateVehicle(int id, VehicleDTO vehicleDto)
        {
            var vehicle = await _vehicleRepo.GetAsync(id);

            if (vehicle == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_NOT_EXIST);
            }

            if (vehicleDto.DriverId != null && !await IsValidDriver(vehicleDto.DriverId.Value))
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }
                       
            vehicle.EngineNumber = vehicleDto.EngineNumber;
            vehicle.RegistrationNumber = vehicleDto.RegistrationNumber.Trim();
            vehicle.ChasisNumber = vehicleDto.ChasisNumber;
            vehicle.VehicleModelId = vehicleDto.VehicleModelId;
            vehicle.Type = vehicleDto.Type;
            vehicle.Status = vehicleDto.VehicleStatus;
            vehicle.DriverId = vehicleDto.DriverId;
            //vehicle.Driver.Name = vehicleDto.DriverName;
            vehicle.LocationId = vehicleDto.LocationId;
            vehicle.FranchizeId = vehicleDto.FranchizeId;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<VehicleDTO>> GetAvailableVehiclesInTerminal()
        {
            var employeeTerminal = await (from employee in _employeeRepo.GetAllIncluding(x => x.User)
                                          join terminal in _terminalRepo.GetAll() on employee.TerminalId equals terminal.Id
                                          where employee.User.UserName == _serviceHelper.GetCurrentUserEmail()
                                          select terminal).FirstOrDefaultAsync();

            return employeeTerminal is null ? new List<VehicleDTO>() :
                await GetAvailableVehiclesInTerminal(employeeTerminal.Id);
        }


        // Create Header/Detail for vehicle allocation
        public Task<List<VehicleDTO>> GetVehiclesByTerminalHeader(int LocationId)
        {
            var vehicles = from vehicle in _vehicleRepo.GetAll()
                           where (vehicle.LocationId == LocationId)
                           orderby vehicle.CreationTime descending

                           select new  VehicleDTO
                           {
                               Id = vehicle.Id,
                               RegistrationNumber = vehicle.RegistrationNumber,
                               ChasisNumber = vehicle.ChasisNumber,
                               EngineNumber = vehicle.EngineNumber,
                               IMEINumber = vehicle.IMEINumber,
                               Type = vehicle.Type,
                               VehicleStatus = vehicle.Status,
                               DateCreated = vehicle.CreationTime,
                               IsOperational = vehicle.IsOperational,
                               DriverId = vehicle.DriverId,
                               VehicleModelId = vehicle.VehicleModelId,
                               LocationId = vehicle.LocationId,

                           };

            return vehicles.AsNoTracking().ToListAsync() ;

        }

        Task<IPagedList<TerminalDTO>> IVehicleService.GetTerminalHeader(int pageNumber, int pageSize, string searchTerm)
        {
            var terminals =
               from terminal in _terminalRepo.GetAll()
               join state in _stateRepo.GetAll() on terminal.StateId equals state.Id
               where (terminal.TerminalType == TerminalType.Physical && string.IsNullOrWhiteSpace(searchTerm) || terminal.Name.Contains(searchTerm))

               orderby terminal.CreationTime descending
               select new TerminalDTO
               {
                   Id = terminal.Id,
                   Name = state.Name + " (" + terminal.Name + ")",
                   Code = terminal.Code,
                   Image = terminal.Image,
                   Latitude = terminal.Latitude,
                   Longitude = terminal.Longitude,
                   StateId = state.Id,
                   StateName = state.Name,
                   BookingType = BookingTypes.Terminal,
                   StartDate = terminal.TerminalStartDate,
                   IsNew = terminal.IsNew,
                   TerminalType = terminal.TerminalType,
                   ContactPerson = terminal.ContactPerson,
                   ContactPersonNo = terminal.ContactPersonNo,
                   Address = terminal.Address,
                   
               };

            return terminals.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        public Task<List<VehicleDTO>> GetVehicleslist()
        {
            var vehicles = from vehicle in _vehicleRepo.GetAll()                   
                           orderby vehicle.CreationTime descending

                           select new VehicleDTO
                           {
                               Id = vehicle.Id,
                               RegistrationNumber = vehicle.RegistrationNumber,
                               ChasisNumber = vehicle.ChasisNumber,
                               EngineNumber = vehicle.EngineNumber,
                               IMEINumber = vehicle.IMEINumber,
                               Type = vehicle.Type,
                               VehicleStatus = vehicle.Status,
                               DateCreated = vehicle.CreationTime,
                               IsOperational = vehicle.IsOperational,
                               DriverId = vehicle.DriverId,
                               VehicleModelId = vehicle.VehicleModelId,
                               LocationId = vehicle.LocationId,

                           };

            return vehicles.AsNoTracking().ToListAsync();
        }

        //getting list of vechicle per terminal
        public Task<IPagedList<VehicleDTO>> GetVehiclesByTerminalHeaderb(int LocationId)
        {
            var vehicles = from vehicle in _vehicleRepo.GetAll() join driver in _driversvc.GetAll() on vehicle.DriverId equals driver.Id
                           where (vehicle.LocationId == LocationId
                           //&& vehicle.Status == VehicleStatus.Idle
                           )
                           orderby vehicle.CreationTime descending

                           select new VehicleDTO
                           {
                               Id = vehicle.Id,
                               RegistrationNumber = vehicle.RegistrationNumber,
                               ChasisNumber = vehicle.ChasisNumber,
                               EngineNumber = vehicle.EngineNumber,
                               IMEINumber = vehicle.IMEINumber,
                               Type = vehicle.Type,
                               VehicleStatus = vehicle.Status,
                               DateCreated = vehicle.CreationTime,
                               IsOperational = vehicle.IsOperational,
                               DriverId = vehicle.DriverId,
                               VehicleModelId = vehicle.VehicleModelId,
                               LocationId = vehicle.LocationId,
                               DriverName = driver.Name
                           };

            return vehicles.AsNoTracking().ToPagedListAsync(1, int.MaxValue);
        }

        public async Task<bool> AllocateBuses(VehicleAllocationDTO queryDto)
        {
            if (!string.IsNullOrEmpty(queryDto.Type.ToString()) && queryDto.Type.ToString() != "0")
            {
                await SendAllocateMessage(queryDto);
            }
            else
            {
                string AllRegs = queryDto.RegistrationNumber;
                string[] AllRegsvalues = AllRegs.Split(',');
                dynamic test = AllRegsvalues;
                foreach (var item in AllRegsvalues)
                {
                    if (item == "")
                    {
                        continue;
                    }
                    else if (item != null)
                    {
                        int id = Convert.ToInt32(item.ToString());
                        var vehicle = await _vehicleRepo.GetAsync(id);
                        vehicle.VehicleModel = await _vehicleModelRepo.GetAsync(vehicle.VehicleModelId);
                        var email = await _userSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
                        queryDto.Email = email.Email;
                        vehicle.LocationId = queryDto.LocationId;
                        vehicle.Status = VehicleStatus.Idle; //new

                        var check = _vehicleAlloRepo.GetAll().Where(s => s.VehicleId == vehicle.Id && s.TransactionDate.Date == DateTime.UtcNow.Date);

                        if (check.Count() != 0)
                        {
                            check.FirstOrDefault().TransactionDate = DateTime.Now;
                            check.FirstOrDefault().DestinationTerminal = vehicle.LocationId;

                            await _unitOfWork.SaveChangesAsync();
                        }

                        else if (check.Count() == 0)
                        {
                            _vehicleAlloRepo.Insert(new VehicleAllocationDetailModel
                            {
                                VehicleId = vehicle.Id,
                                DriverId = vehicle.DriverId,
                                VehicleName = vehicle.VehicleModel.Name,
                                DestinationTerminal = vehicle.LocationId,
                                UserEmail = queryDto.Email,
                                TransactionDate = DateTime.Now,
                                CreatorUserId = _serviceHelper.GetCurrentUserId(),

                            });
                        }

                        await _unitOfWork.SaveChangesAsync();
                    };
                }
            }
            return true;
        }


        public async Task<bool> SendAllocateMessage(VehicleAllocationDTO queryDto)
        {
            string ComposedDetail ="";
            string joined = "";
            string DRegNumber = "";
            string NewL = HttpUtility.HtmlEncode("<br />");

            IQueryable<VehicleAllocationDTO> Terminal = null;

            // Reset All assigned Vehicle location to Benin
            if (queryDto.Type == 3)
            {
                var vehicleinfo = from vvehicle in _vehicleRepo.GetAll()
                                  select new VehicleAllocationDTO
                                  {
                                  RegistrationNumber = vvehicle.RegistrationNumber,
                                  LocationId = vvehicle.LocationId,
                                  DriverId = vvehicle.DriverId,
                                  Id = vvehicle.Id
                                  };
                foreach (var detail in vehicleinfo)
                {
                    int id = Convert.ToInt32("25");
                    var vehicle = await _vehicleRepo.GetAsync(detail.Id);
                    vehicle.LocationId = id;
                    vehicle.Status = VehicleStatus.Idle; 
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else
            {
                //send sms to a specified terminal after assigning of vehicles
                if (queryDto.Type == 2)
                {
                    Terminal = from terminal in _terminalRepo.GetAll().Where(x => x.Id == queryDto.LocationId)
                               select new VehicleAllocationDTO
                               {
                                   TerminalId = terminal.Id,
                                   ContactPersonNo = terminal.ContactPersonNo,
                                   TerminalName = terminal.Name
                               };
                }
                //send sms to all terminals after assigning of vehicles
                else if (queryDto.Type == 1)
                {   //var newInClause = new string[] { "26", "16", "30" }.Where(x => x.LocationId == 26 || x.LocationId == 16 || x.LocationId == 30 );
                    Terminal = from terminal in _terminalRepo.GetAll().Where(x => x.TerminalType == TerminalType.Physical)
                               select new VehicleAllocationDTO
                               {
                                   TerminalId = terminal.Id,
                                   ContactPersonNo = terminal.ContactPersonNo,
                                   TerminalName = terminal.Name
                               };
                }
                foreach (var item in Terminal)
                {
                    if (!string.IsNullOrEmpty(item.ContactPersonNo))
                    {
                        var vehicleinfo = from vehicle in _vehicleRepo.GetAll()
                                              //join driverinfo  in _driversvc.GetAll() on vehicle.DriverId equals driverinfo.Id
                                          where (vehicle.LocationId == item.TerminalId)
                                          select new VehicleAllocationDTO
                                          {
                                              RegistrationNumber = vehicle.RegistrationNumber,
                                              //Details = vehicle.RegistrationNumber + " - " + driverinfo.Name,
                                              DriverId = vehicle.DriverId

                                          };

                        StringBuilder AllDetails = new StringBuilder();
                        foreach (var detail in vehicleinfo)
                        {
                            DRegNumber = detail.RegistrationNumber.ToString();
                            var Dname = await _driversvc.GetDriverById(detail.DriverId);
                            string ShortDname = "";
                            if(detail.DriverId == 497)
                            {
                                ShortDname = "N/A";
                            }
                            else
                            {
                                ShortDname = Dname.Name.ToString();
                            }
                            ComposedDetail = "  " + DRegNumber + " - " + ShortDname;
                            AllDetails.Append(string.Format("{0};", ComposedDetail));
                            AllDetails.Append("\r\n").Replace(System.Environment.NewLine, "\r\n");
                            //joined = string.Join(",", ComposedDetail);
                        }
                        string smsMessageb = $"Vehicles allocated are \r\n" + AllDetails.ToString();
                        await SendSMS(item.ContactPersonNo, smsMessageb);

                    }
                }
            }

            return true;
        }

        private async Task SendSMS(string phoneNumber, string smsMessageb)
        {
            string smsMessage = smsMessageb;
            try
            {
                await Task.Factory.StartNew(() => _smsSvc.SendSMSNow(smsMessage, recipient: phoneNumber.ToNigeriaMobile()));
            }

            catch (Exception)
            {
            }
        }

        public async Task<List<VehicleDTO>> VehiclesRemainingInTerminal(int terminalId)
        {
            var AllocDate = DateTime.Now.Date.AddDays(-1);
            var EAllocdate = AllocDate.AddHours(23);
            var travelDate = DateTime.Now.Date;
            var etravelDate = DateTime.Now;

            var reports = await _unitOfWork
                .GetDbContext<ApplicationDbContext>()
                .Database.ExecuteSqlToObject<VehicleDTO>(@"Exec sp_LeftOver",
                                                                AllocDate, EAllocdate, travelDate, etravelDate, terminalId);

            return reports.ToList();

        }
        public bool AllocateVehicleConfirmation(int vehicleId)
        {
            var exists = from confirm in _vehicleAlloRepo.GetAll()
                         where confirm.VehicleId == vehicleId && confirm.TransactionDate.Date == DateTime.UtcNow.Date

                         select confirm;

            return exists.FirstOrDefault() != null ? true : false;
        }

        public async Task<bool> DeleteFromVehicleAllocationTable()
        {
            DateTime startDate = DateTime.UtcNow.Date;

            DateTime endDate = DateTime.Now;

            var reports = await _unitOfWork
             .GetDbContext<ApplicationDbContext>()
             .Database.ExecuteSqlToObject<bool>(@"Exec spDeleteFromVehicleAllocationTable", startDate, endDate);


            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}