using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
//using Travel.Core.Domain.Entities;
//using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
namespace Travel.Business.Services
{
    public interface IDriverService
    {
        Task<DriverDTO> GetActiveDriverByCodeAsync(string DriverCode);
        Task<DriverDTO> GetActiveDriverByVehicleAsync(string regnum);
        Task<List<DriverDTO>> GetAllAvailableDriversAsync();
        Task<List<DriverDTO>> GetAvailableDriversAsync();
        Task AddDriver(DriverDTO driver);
        Task<List<DriverDTO>> GetAvailableDriversForTripCountAsync();
        Task<List<JourneyManagementDTO>> GetAvailableJourneyMangementCount(string DriverCode, int StartDay, int EndDay);
        Task<DriverDTO> GetDriverByBusNumberAsync(string busNumber);
        Task<DriverDTO> GetDriverByCodeAsync(string DriverCode);
        Task<DriverDTO> GetDriverByVehicleAsync(string regnum);
        Task<IPagedList<DriverDTO>> GetDriversAsync(int pageNumber, int pageSize, string searchTerm);
        Task<List<DriverDTO>> GetVirtualDriversAsync();
        void RemoveAssignedVehicle(string DriverCode);
        Task<Driver> FirstOrDefaultAsync(Expression<Func<Driver, bool>> filter);
        Task<bool> ExistAsync(Expression<Func<Driver, bool>> filter);
        Task<Driver> GetAsync(int id);
        Task<DriverDTO> GetDriverById(int? id);
        Task UpdateDriver(int id, DriverDTO model);
        Task RemoveDriver(int id);
        IQueryable<Driver> GetAll();
        Task<DriverDTO> GetDriverByVehicleRegNum(string regnum);
        Task<DriverDTO> GetDriverByVehicleId(string id);
    }

    public class DriverService : IDriverService
    {
        private readonly IRepository<WalletNumber> _walletNumberRepo;
        private readonly IRepository<Driver> _driverRepo;
        private readonly IWalletService _walletSvc;
        private readonly IRepository<VehicleTripRegistration, Guid> _vehicleTripRegRepo;
        private readonly IRepository<JourneyManagement, Guid> _journeyMgtRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DriverService(IRepository<Driver> driverRepo, IWalletService walletSvc,
            IRepository<VehicleTripRegistration, Guid> vehicleTripRegRepo, IRepository<WalletNumber> walletNumberRepo,
            IRepository<JourneyManagement, Guid> journeyMgtRepo,
            IServiceHelper serviceHelper, IUnitOfWork unitOfWork,
            IRepository<Vehicle> vehicleRepo)
        {
            _driverRepo = driverRepo;
            _walletSvc = walletSvc;
            _vehicleTripRegRepo = vehicleTripRegRepo;
            _journeyMgtRepo = journeyMgtRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
            _walletNumberRepo = walletNumberRepo;
            _vehicleRepo = vehicleRepo;
        }

        public IQueryable<Driver> GetAll()
        {
            return _driverRepo.GetAll();
        }


        public Task<IPagedList<DriverDTO>> GetDriversAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var drivers =
                from driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on driver.WalletId equals wallet.Id

                where string.IsNullOrWhiteSpace(searchTerm) ||
                (driver.Name.Contains(searchTerm) || driver.Code.Contains(searchTerm))
                orderby driver.CreationTime descending

                select new DriverDTO
                {
                    DriverDetails = driver.Name + " - " + driver.Code + " ( " + driver.DriverStatus + " )",
                    Details = driver.Code + " - " + driver.Name,
                    Id = driver.Id,
                    Code = driver.Code,
                    HandoverCode = driver.HandoverCode,
                    DriverStatus = driver.DriverStatus,
                    DriverType = driver.DriverType,
                    Name = driver.Name,
                    Phone1 = driver.Phone1,
                    Phone2 = driver.Phone2,
                    Designation = driver.Designation,
                    DateOfEmployment = driver.DateOfEmployment,
                    AssignedDate = driver.AssignedDate,
                    ResidentialAddress = driver.ResidentialAddress,
                    NextOfKin = driver.NextOfKin,
                    DateCreated = driver.CreationTime,
                    Picture = driver.Picture,
                    Active = driver.Active,
                    NextOfKinNumber = driver.NextOfKinNumber,
                    BankName = driver.BankName,
                    BankAccount = driver.BankAccount,
                    DeactivationReason = driver.DeactivationReason,
                    ActivationStatusChangedByEmail = driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    NoOfTrips = driver.NoOfTrips,
                    VehicleNumber = driver.VehicleRegistrationNumber
                };

            return drivers.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }


        public Task<List<DriverDTO>> GetAvailableDriversAsync()
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where
                    (Driver.DriverStatus == DriverStatus.Idle && Driver.Active && Driver.DriverType == DriverType.Handover)
                    || Driver.DriverType == DriverType.Virtual

                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber,
                    DateOfEmployment = Driver.DateOfEmployment
                };

            return Drivers.AsNoTracking().ToListAsync();
        }

        public Task<List<DriverDTO>> GetAllAvailableDriversAsync()
        {
            var drivers =
                from driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on driver.WalletId equals wallet.Id
                where

                    (driver.DriverStatus == DriverStatus.Idle && driver.Active)

                select new DriverDTO
                {
                    DriverDetails = driver.Name + " - " + driver.Code + " ( " + driver.DriverStatus + " )",
                    Id = driver.Id,
                    Code = driver.Code,
                    HandoverCode = driver.HandoverCode,
                    DriverStatus = driver.DriverStatus,
                    DriverType = driver.DriverType,
                    Name = driver.Name,
                    Phone1 = driver.Phone1,
                    Phone2 = driver.Phone2,
                    Designation = driver.Designation,
                    AssignedDate = driver.AssignedDate,
                    ResidentialAddress = driver.ResidentialAddress,
                    NextOfKin = driver.NextOfKin,
                    DateCreated = driver.CreationTime,
                    Picture = driver.Picture,
                    Active = driver.Active,
                    NextOfKinNumber = driver.NextOfKinNumber,
                    BankName = driver.BankName,
                    BankAccount = driver.BankAccount,
                    DeactivationReason = driver.DeactivationReason,
                    ActivationStatusChangedByEmail = driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = driver.MaintenanceWalletId,
                    NoOfTrips = driver.NoOfTrips,
                    VehicleNumber = driver.VehicleRegistrationNumber,
                    DateOfEmployment = driver.DateOfEmployment
                };

            return drivers.AsNoTracking().ToListAsync();
        }

        public Task<List<DriverDTO>> GetAvailableDriversForTripCountAsync()
        {
            var drivers =
              from driver in _driverRepo.GetAll()
              join wallet in _walletSvc.GetAll() on driver.WalletId equals wallet.Id

              select new DriverDTO
              {
                  DriverDetails = driver.Name + " - " + driver.Code + " ( " + driver.DriverStatus + " )",
                  Id = driver.Id,
                  Code = driver.Code,
                  HandoverCode = driver.HandoverCode,
                  DriverStatus = driver.DriverStatus,
                  DriverType = driver.DriverType,
                  Name = driver.Name,
                  Phone1 = driver.Phone1,
                  Phone2 = driver.Phone2,
                  Designation = driver.Designation,
                  AssignedDate = driver.AssignedDate,
                  ResidentialAddress = driver.ResidentialAddress,
                  NextOfKin = driver.NextOfKin,
                  DateCreated = driver.CreationTime,
                  Picture = driver.Picture,
                  Active = driver.Active,
                  NextOfKinNumber = driver.NextOfKinNumber,
                  BankName = driver.BankName,
                  BankAccount = driver.BankAccount,
                  DeactivationReason = driver.DeactivationReason,
                  ActivationStatusChangedByEmail = driver.ActivationStatusChangedByEmail,
                  WalletId = wallet.Id,
                  WalletBalance = wallet.Balance,
                  WalletNumber = wallet.WalletNumber,
                  MaintenanceWalletId = driver.MaintenanceWalletId,
                  NoOfTrips = driver.NoOfTrips,
                  VehicleNumber = driver.VehicleRegistrationNumber,
                  DateOfEmployment = driver.DateOfEmployment
              };

            return drivers.AsNoTracking().ToListAsync();
        }

        public Task<List<JourneyManagementDTO>> GetAvailableJourneyMangementCount(string DriverCode, int StartDay, int EndDay)
        {

            var theDay = DateTime.Now.Day;
            var theMonth = DateTime.Now.Month;
            var theYear = DateTime.Now.Year;
            int newYear = DateTime.Now.Year + 1;
            int lastYear = DateTime.Now.Year - 1;
            DateTime theStartDate;
            DateTime theEndDate;
            int startMonth;
            DateTime fullEndMonth;
            DateTime fullStartMonth;
            int endMonth;

            if (theDay > EndDay) {
                startMonth = theMonth;
                fullEndMonth = DateTime.Now.AddMonths(1);
                endMonth = fullEndMonth.Month;
                if (theMonth == 12) {
                    theStartDate = new DateTime(theYear, theMonth, StartDay);
                    theEndDate = new DateTime(newYear, endMonth, EndDay);
                }
                else {
                    theStartDate = new DateTime(theYear, theMonth, StartDay);
                    theEndDate = new DateTime(theYear, endMonth, EndDay);

                }


            }
            else {

                startMonth = theMonth;
                fullStartMonth = DateTime.Now.AddMonths(-1);
                startMonth = fullStartMonth.Month;
                endMonth = theMonth;

                if (theMonth == 1) {
                    theStartDate = new DateTime(lastYear, startMonth, StartDay);
                    theEndDate = new DateTime(theYear, theMonth, EndDay);
                }
                else {
                    theStartDate = new DateTime(theYear, startMonth, StartDay);
                    theEndDate = new DateTime(theYear, theMonth, EndDay);
                }

            }

            var Drivers =
                from DriverVehicleReg in _vehicleTripRegRepo.GetAll()
                join DriverJourneyManagmnt in _journeyMgtRepo.GetAll() on DriverVehicleReg.Id equals DriverJourneyManagmnt.VehicleTripRegistrationId
                where DriverVehicleReg.DriverCode == DriverCode
                && DriverJourneyManagmnt.TripStartTime > theStartDate
                && DriverJourneyManagmnt.TripStartTime < theEndDate
                && DriverJourneyManagmnt.JourneyStatus == JourneyStatus.Received


                select new JourneyManagementDTO
                {
                    Id = DriverJourneyManagmnt.Id,
                    VehicleTripRegistrationId = DriverJourneyManagmnt.VehicleTripRegistrationId,
                    JourneyStatus = DriverJourneyManagmnt.JourneyStatus,
                    TripStartTime = DriverJourneyManagmnt.TripStartTime,
                    TripEndTime = DriverJourneyManagmnt.TripEndTime,
                    DateCreated = DriverJourneyManagmnt.CreationTime
                };


            return Drivers.AsNoTracking().ToListAsync();
        }

        public Task<DriverDTO> GetDriverByCodeAsync(string DriverCode)
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where Driver.Code == DriverCode

                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber
                };

            return Drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<DriverDTO> GetActiveDriverByCodeAsync(string DriverCode)
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where Driver.Code == DriverCode && Driver.Active


                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber
                };

            return Drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<DriverDTO> GetDriverByBusNumberAsync(string busNumber)
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where Driver.VehicleRegistrationNumber == busNumber


                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber

                };

            return Drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<List<DriverDTO>> GetVirtualDriversAsync()
        {
            var Drivers =
                    from Driver in _driverRepo.GetAll()
                    join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                    where Driver.DriverType == DriverType.Virtual

                    select new DriverDTO
                    {
                        DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                        Id = Driver.Id,
                        Code = Driver.Code,
                        HandoverCode = Driver.HandoverCode,
                        DriverStatus = Driver.DriverStatus,
                        DriverType = Driver.DriverType,
                        Name = Driver.Name,
                        Phone1 = Driver.Phone1,
                        Phone2 = Driver.Phone2,
                        Designation = Driver.Designation,
                        AssignedDate = Driver.AssignedDate,
                        ResidentialAddress = Driver.ResidentialAddress,
                        NextOfKin = Driver.NextOfKin,
                        DateCreated = Driver.CreationTime,
                        Picture = Driver.Picture,
                        Active = Driver.Active,
                        NextOfKinNumber = Driver.NextOfKinNumber,
                        BankName = Driver.BankName,
                        BankAccount = Driver.BankAccount,
                        DeactivationReason = Driver.DeactivationReason,
                        ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                        WalletId = wallet.Id,
                        WalletBalance = wallet.Balance,
                        WalletNumber = wallet.WalletNumber,
                        MaintenanceWalletId = Driver.MaintenanceWalletId,
                        NoOfTrips = Driver.NoOfTrips,
                        VehicleNumber = Driver.VehicleRegistrationNumber
                    };

            return Task.FromResult(Drivers.AsNoTracking().ToList());
        }

        public void RemoveAssignedVehicle(string DriverCode)
        {
            var Driver = _driverRepo.FirstOrDefault(c => c.Code == DriverCode);

            if (Driver == null)
                return;

            Driver.VehicleRegistrationNumber = "";
        }

        public Task<DriverDTO> GetDriverByVehicleAsync(string regnum)
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where Driver.VehicleRegistrationNumber == regnum && (Driver.DriverType == DriverType.Permanent || Driver.DriverType == DriverType.Owner)


                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber
                };

            return Drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<DriverDTO> GetActiveDriverByVehicleAsync(string regnum)
        {
            var Drivers =
                from Driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                where Driver.VehicleRegistrationNumber == regnum && (Driver.DriverType == DriverType.Permanent || Driver.DriverType == DriverType.Owner) && Driver.Active

                select new DriverDTO
                {
                    DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                    Id = Driver.Id,
                    Code = Driver.Code,
                    HandoverCode = Driver.HandoverCode,
                    DriverStatus = Driver.DriverStatus,
                    DriverType = Driver.DriverType,
                    Name = Driver.Name,
                    Phone1 = Driver.Phone1,
                    Phone2 = Driver.Phone2,
                    Designation = Driver.Designation,
                    AssignedDate = Driver.AssignedDate,
                    ResidentialAddress = Driver.ResidentialAddress,
                    NextOfKin = Driver.NextOfKin,
                    DateCreated = Driver.CreationTime,
                    Picture = Driver.Picture,
                    Active = Driver.Active,
                    NextOfKinNumber = Driver.NextOfKinNumber,
                    BankName = Driver.BankName,
                    BankAccount = Driver.BankAccount,
                    DeactivationReason = Driver.DeactivationReason,
                    ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = Driver.MaintenanceWalletId,
                    NoOfTrips = Driver.NoOfTrips,
                    VehicleNumber = Driver.VehicleRegistrationNumber
                };

            return Drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<Driver> FirstOrDefaultAsync(Expression<Func<Driver, bool>> filter)
        {
            return _driverRepo.FirstOrDefaultAsync(filter);
        }

        public Task<Driver> GetAsync(int id)
        {
            return _driverRepo.GetAsync(id);
        }

        public async Task<DriverDTO> GetDriverById(int? id)
        {
            var driver =
                await (from Driver in _driverRepo.GetAll()
                       join wallet in _walletSvc.GetAll() on Driver.WalletId equals wallet.Id
                       where Driver.Id == id

                       select new DriverDTO
                       {
                           DriverDetails = Driver.Name + " - " + Driver.Code + " ( " + Driver.DriverStatus + " )",
                           Id = Driver.Id,
                           Code = Driver.Code,
                           HandoverCode = Driver.HandoverCode,
                           DriverStatus = Driver.DriverStatus,
                           DriverType = Driver.DriverType,
                           Name = Driver.Name,
                           Phone1 = Driver.Phone1,
                           Phone2 = Driver.Phone2,
                           Designation = Driver.Designation,
                           AssignedDate = Driver.AssignedDate,
                           ResidentialAddress = Driver.ResidentialAddress,
                           NextOfKin = Driver.NextOfKin,
                           DateCreated = Driver.CreationTime,
                           Picture = Driver.Picture,
                           Active = Driver.Active,
                           NextOfKinNumber = Driver.NextOfKinNumber,
                           BankName = Driver.BankName,
                           BankAccount = Driver.BankAccount,
                           DeactivationReason = Driver.DeactivationReason,
                           ActivationStatusChangedByEmail = Driver.ActivationStatusChangedByEmail,
                           WalletId = wallet.Id,
                           WalletBalance = wallet.Balance,
                           WalletNumber = wallet.WalletNumber,
                           MaintenanceWalletId = Driver.MaintenanceWalletId,
                           NoOfTrips = Driver.NoOfTrips,
                           VehicleNumber = Driver.VehicleRegistrationNumber
                       }).FirstOrDefaultAsync();

            if (driver is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }

            return driver;
        }

        public async Task UpdateDriver(int driverId, DriverDTO driverDto)
        {
            var driver = await GetAsync(driverId);

            if (driver == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }

            driver.Code = driverDto.Code;
            driver.DriverType = driverDto.DriverType;
            driver.HandoverCode = driverDto.HandoverCode;
            driver.Name = driverDto.Name;
            driver.Phone1 = driverDto.Phone1;
            driver.Phone2 = driverDto.Phone2;
            driver.ResidentialAddress = driverDto.ResidentialAddress;
            driver.NextOfKin = driverDto.NextOfKin;
            driver.NextOfKinNumber = driverDto.NextOfKinNumber;
            driver.BankName = driverDto.BankName;
            driver.BankAccount = driverDto.BankAccount;
            driver.DateOfEmployment = driverDto.DateOfEmployment;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveDriver(int id)
        {
            var driver = await GetAsync(id);

            if (driver is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_NOT_EXIST);
            }

            driver.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddDriver(DriverDTO driver)
        {
            driver.Code = driver.Code.Trim();

            if (await _driverRepo.ExistAsync(v => v.Code == driver.Code)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DRIVER_EXIST);
            }

            var walletNumber = await _walletSvc.GenerateNextValidWalletNumber();

            _walletNumberRepo.Insert(walletNumber);

            var wallet = new Wallet
            {
                WalletNumber = walletNumber.WalletPan,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                Balance = 0.00M,
                UserType = UserType.Captain.ToString()
            };

            _walletSvc.Add(wallet);

            _driverRepo.Insert(new Driver
            {
                Code = driver.Code,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                DriverStatus = driver.DriverStatus,
                DriverType = driver.DriverType,
                HandoverCode = driver.HandoverCode,
                Name = driver.Name,
                Phone1 = driver.Phone1,
                Phone2 = driver.Phone2,
                Designation = driver.Designation,
                AssignedDate = driver.AssignedDate,
                ResidentialAddress = driver.ResidentialAddress,
                NextOfKin = driver.NextOfKin,
                Picture = driver.Picture,
                Active = true,
                NextOfKinNumber = driver.NextOfKinNumber,
                BankName = driver.BankName,
                BankAccount = driver.BankAccount,
                DeactivationReason = driver.DeactivationReason,
                WalletId = wallet.Id,
                NoOfTrips = 0,
                DateOfEmployment = driver.DateOfEmployment
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<DriverDTO> GetDriverByVehicleRegNum(string regnum)
        {
            var drivers =
                from driver in _driverRepo.GetAll()
                join wallet in _walletSvc.GetAll() on driver.WalletId equals wallet.Id
                where driver.VehicleRegistrationNumber == regnum && (driver.DriverType == DriverType.Permanent || driver.DriverType == DriverType.Owner) && driver.Active

                select new DriverDTO
                {
                    DriverDetails = driver.Name + " - " + driver.Code + " ( " + driver.DriverStatus + " )",
                    Id = driver.Id,
                    Code = driver.Code,
                    HandoverCode = driver.HandoverCode,
                    DriverStatus = driver.DriverStatus,
                    DriverType = driver.DriverType,
                    Name = driver.Name,
                    Phone1 = driver.Phone1,
                    Phone2 = driver.Phone2,
                    Designation = driver.Designation,
                    AssignedDate = driver.AssignedDate,
                    ResidentialAddress = driver.ResidentialAddress,
                    NextOfKin = driver.NextOfKin,
                    DateCreated = driver.CreationTime,
                    Active = driver.Active,
                    NextOfKinNumber = driver.NextOfKinNumber,
                    BankName = driver.BankName,
                    BankAccount = driver.BankAccount,
                    DeactivationReason = driver.DeactivationReason,
                    ActivationStatusChangedByEmail = driver.ActivationStatusChangedByEmail,
                    WalletId = wallet.Id,
                    WalletBalance = wallet.Balance,
                    WalletNumber = wallet.WalletNumber,
                    MaintenanceWalletId = driver.MaintenanceWalletId,
                    NoOfTrips = driver.NoOfTrips,
                    VehicleNumber = driver.VehicleRegistrationNumber
                };

            return drivers.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<Driver, bool>> filter)
        {
            return await _driverRepo.ExistAsync(filter);
        }

        public Task<DriverDTO> GetDriverByVehicleId(string id)
        {
            var vehicles = from vehicle in _vehicleRepo.GetAll()
                           join driver in _driverRepo.GetAll() on vehicle.DriverId equals driver.Id
                           where vehicle.RegistrationNumber == id

                           select new DriverDTO
                           {
                               Id = vehicle.Id,
                               Code = driver.Code,
                               Name = driver.Name,
                               Picture = driver.Picture
                           };

            return vehicles.AsNoTracking().FirstOrDefaultAsync();
        }
    }
}