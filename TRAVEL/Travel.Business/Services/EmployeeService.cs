using IPagedList;
using Travel.Core;
using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Email;
using Travel.Core.Messaging.Sms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface IEmployeeService
    {
        Employee FirstOrDefault(Expression<Func<Employee, bool>> filter);
        IQueryable<Employee> GetAll();
        Task<List<EmployeeDTO>> GetTerminalEmployees(int terminalId);
        Task<EmployeeDTO> GetEmployeesByemailAsync(string email);
        Task AddEmployee(EmployeeDTO employee);
        Task<IPagedList<EmployeeDTO>> GetEmployees(int pageNumber, int pageSize, string query);
        Task<int?> GetAssignedTerminal(string email);
        Task<EmployeeDTO> GetOperationManager(string email);
        Task UpdateEmployeeOtp(int employeeId, string otp);
        Task<bool> Verifyotp(string otp);
        Task<EmployeeDTO> GetEmployee(int id);
        Task UpdateEmployee(int id, EmployeeDTO model);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        private readonly IRepository<Employee> _repo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IUserService _userSvc;
        private readonly IRoleService _roleSvc;
        private readonly IReferralService _referralSvc;
        private readonly ISMSService _smsSvc;
        private readonly IMailService _mailSvc;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IGuidGenerator _guidGenerator;
        private readonly AppConfig appConfig;

        public EmployeeService(IUnitOfWork unitOfWork,
            IRepository<Employee> employeeRepo,
            IRepository<Terminal> terminalRepo,
            IRepository<Wallet> walletRepo,
            IRepository<Department> departmentRepo,
            IServiceHelper serviceHelper,
            IUserService userSvc,
            IRoleService roleSvc,
            IReferralService referralSvc,
            ISMSService smsSvc,
            IMailService mailSvc,
            IHostingEnvironment hostingEnvironment,
            IGuidGenerator guidGenerator,
            IOptions<AppConfig> _appConfig)
        {
            _unitOfWork = unitOfWork;
            _repo = employeeRepo;
            _terminalRepo = terminalRepo;
            _walletRepo = walletRepo;
            _departmentRepo = departmentRepo;
            _serviceHelper = serviceHelper;
            _userSvc = userSvc;
            _referralSvc = referralSvc;
            _smsSvc = smsSvc;
            _mailSvc = mailSvc;
            appConfig = _appConfig.Value;
            _hostingEnvironment = hostingEnvironment;
            _guidGenerator = guidGenerator;
            _roleSvc = roleSvc;
        }

        private async Task<bool> IsValidTerminal(int? id)
        {
            return await _terminalRepo.ExistAsync(x => x.Id == id);
        }

        private async Task<bool> IsValidDepartment(int? id)
        {
            return await _departmentRepo.ExistAsync(x => x.Id == id);
        }

        private async Task<bool> EmployeeExist(string code)
        {
            return await _repo.ExistAsync(x => x.EmployeeCode == code);
        }

        public async Task AddEmployee(EmployeeDTO employee)
        {
            if (employee == null) {
                throw await _serviceHelper.GetExceptionAsync("invalid parameter");
            } 

            if (employee.TerminalId != null && !await IsValidTerminal(employee.TerminalId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TERMINAL_NOT_EXIST);
            }

            if (employee.DepartmentId != null && !await IsValidDepartment(employee.DepartmentId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DEPARTMENT_NOT_EXIST);
            }

            employee.EmployeeCode = employee.EmployeeCode.Trim();

            if (await _repo.ExistAsync(v => v.EmployeeCode == employee.EmployeeCode)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.EMPLOYEE_EXIST);
            }

            try {
                _unitOfWork.BeginTransaction();

                var user = new User
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    MiddleName = employee.MiddleName,
                    Gender = employee.Gender,
                    Email = employee.Email,
                    PhoneNumber = employee.PhoneNumber,
                    Address = employee.Address,
                    NextOfKinName = employee.NextOfKin,
                    NextOfKinPhone = employee.NextOfKinPhone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    UserName = employee.Email,
                    ReferralCode = CommonHelper.GenereateRandonAlphaNumeric()
                };

                var creationStatus = await _userSvc.CreateAsync(user, "123456");

                if (creationStatus.Succeeded) {

                    var dbRole = await _roleSvc.FindByIdAsync(employee.RoleId);

                    if (dbRole != null) {
                        await _userSvc.AddToRoleAsync(user, dbRole.Name);
                    }

                    _repo.Insert(new Employee
                    {
                        UserId = user.Id,
                        EmployeeCode = employee.EmployeeCode,
                        DateOfEmployment = employee.DateOfEmployment,
                        DepartmentId = employee.DepartmentId,
                        TerminalId = employee.TerminalId,
                        CreatorUserId = _serviceHelper.GetCurrentUserId()
                    });


                    await SendAccountEmail(user);

                }
                else {
                    _unitOfWork.Rollback();

                    throw await _serviceHelper
                        .GetExceptionAsync(creationStatus.Errors.FirstOrDefault()?.Description);
                }
                _unitOfWork.Commit();
            }
            catch (Exception) {

                _unitOfWork.Rollback();
                throw;
            }
        }

        private async Task SendAccountEmail(User user)
        {
            try {

                var replacement = new StringDictionary
                {
                    ["FirstName"] = user.FirstName,
                    ["UserName"] = user.UserName,
                    ["DefaultPassword"] = "123456"
                };

                var mail = new Mail(appConfig.AppEmail, "Libmot.com: New staff account information", user.Email)
                {
                    BodyIsFile = true,
                    BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.AccountActivationEmail)
                };

                await _mailSvc.SendMailAsync(mail, replacement);
            }
            catch (Exception) {
            }
        }

        public Employee FirstOrDefault(Expression<Func<Employee, bool>> filter)
        {
            return _repo.FirstOrDefault(filter);
        }

        public IQueryable<Employee> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<EmployeeDTO> GetEmployeesByemailAsync(string email)
        {
            var employees =
                from employee in _repo.GetAllIncluding(x => x.User)

                join department in _departmentRepo.GetAll() on employee.DepartmentId equals department.Id
                into departments
                from department in departments.DefaultIfEmpty()

                join wallet in _walletRepo.GetAll() on employee.User.WalletId equals wallet.Id
                into wallets
                from wallet in wallets.DefaultIfEmpty()

                join terminal in _terminalRepo.GetAll() on employee.TerminalId equals terminal.Id
                into terminals
                from terminal in terminals.DefaultIfEmpty()

                where employee.User.Email == email

                select new EmployeeDTO
                {
                    Id = employee.Id,
                    DateOfEmployment = employee.DateOfEmployment,
                    FirstName = employee.User.FirstName,
                    LastName = employee.User.LastName,
                    MiddleName = employee.User.MiddleName,
                    Gender = employee.User.Gender,
                    Email = employee.User.Email,
                    PhoneNumber = employee.User.PhoneNumber,
                    Address = employee.User.Address,
                    EmployeePhoto = employee.User.Photo,
                    NextOfKin = employee.User.NextOfKinName,
                    NextOfKinPhone = employee.User.NextOfKinPhone,
                    DepartmentName = department.Name,
                    DepartmentId = department.Id,
                    TerminalId = terminal.Id,
                    TerminalName = terminal.Name,
                    Otp = employee.Otp,
                    OtpIsUsed = employee.OtpIsUsed,
                    EmployeeCode = employee.EmployeeCode,
                };

            return employees.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<List<EmployeeDTO>> GetTerminalEmployees(int terminalId)
        {
            var employees =
                from employee in _repo.GetAllIncluding(x => x.User)
                join terminal in _terminalRepo.GetAll() on employee.TerminalId equals terminal.Id
                where terminal.Id == terminalId

                select new EmployeeDTO
                {
                    Id = employee.Id,
                    FirstName = employee.User.FirstName,
                    LastName = employee.User.LastName,
                    Email = employee.User.Email,
                    Otp = employee.Otp,
                    OtpIsUsed = employee.OtpIsUsed,
                    PhoneNumber = employee.User.PhoneNumber
                };

            return employees.AsNoTracking().ToListAsync();
        }
        public async Task<EmployeeDTO> GetOperationManager(string email)
        {


            var terminal = await GetAssignedTerminal(email);
            if (terminal != null) {
                var terminalEmployees = await GetAllEmployeeByTerminalId(terminal.GetValueOrDefault());
                foreach (var employee in terminalEmployees) {
                    IList<string> userRoles = new List<string>();

                    try {
                        userRoles = await _userSvc.GetUserRolesAsync(employee.Email);
                    }
                    catch {

                    }


                    foreach (var role in userRoles) {
                        if (role == "Operations Manager") {
                            return employee;
                        }
                    }
                }
            }

            return new EmployeeDTO();



        }

        public async Task<bool> Verifyotp(string otp)
        {


            var email = await _userSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
            var operationManager = await GetOperationManager(email.Email);
            if (operationManager.Otp == otp && !operationManager.OtpIsUsed) {
                await UpdateUsedEmployeeOtp(operationManager.Id);
                return true;
            }
            return false;


        }
        public async Task<List<EmployeeDTO>> GetAllEmployeeByTerminalId(int terminalId)
        {

            var employee = await GetTerminalEmployees(terminalId);

            if (employee == null) {
                //throw await _helper.GetExceptionAsync(ErrorConstants.EMPLOYEE_NOT_EXIST);
                return new List<EmployeeDTO>();
            }

            return employee;

        }

        public async Task UpdateEmployeeOtp(int employeeId, string otp)
        {
            var employees = await _repo.GetAsync(employeeId);

            if (employees == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.EMPLOYEE_NOT_EXIST);
            }

            employees.Otp = otp;
            employees.OtpIsUsed = false;
            DateTime today = DateTime.Now.Date;
            var otpMaxperday = appConfig.OTPMaxperday != null ? int.Parse(appConfig.OTPMaxperday) : 2;


            if (employees.OTPLastUsedDate == today && employees.OtpNoOfTimeUsed == otpMaxperday) {
                throw await _serviceHelper.GetExceptionAsync("You have exceeded the maximum number of OTP for a day");
            }
            else {
                //if (employees.OTPLastUsedDate == null)
                //{
                //    employees.OTPLastUsedDate = today;
                //    employees.OtpNoOfTimeUsed = 1;
                //}
                //else if (employees.OTPLastUsedDate != today)
                //{
                //    employees.OTPLastUsedDate = today;
                //    employees.OtpNoOfTimeUsed = 1;
                //}
                //else if (employees.OTPLastUsedDate == today)
                //{
                //    employees.OtpNoOfTimeUsed = employees.OtpNoOfTimeUsed.GetValueOrDefault() + 1;
                //}


            }



            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUsedEmployeeOtp(int employeeId)
        {
            var employees = await _repo.GetAsync(employeeId);

            if (employees == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.EMPLOYEE_NOT_EXIST);
            }

            employees.OtpIsUsed = true;
            DateTime today = DateTime.Now.Date;
            var otpMaxperday = appConfig.OTPMaxperday != null ? int.Parse(appConfig.OTPMaxperday) : 2;

            if (employees.OTPLastUsedDate == today && employees.OtpNoOfTimeUsed == otpMaxperday) {
                throw await _serviceHelper.GetExceptionAsync("You have exceeded the maximum number of OTP for a day");
            }
            else {
                if (employees.OTPLastUsedDate == null) {
                    employees.OTPLastUsedDate = today;
                    employees.OtpNoOfTimeUsed = 1;
                }
                else if (employees.OTPLastUsedDate != today) {
                    employees.OTPLastUsedDate = today;
                    employees.OtpNoOfTimeUsed = 1;
                }
                else if (employees.OTPLastUsedDate == today) {
                    employees.OtpNoOfTimeUsed = employees.OtpNoOfTimeUsed.GetValueOrDefault() + 1;
                }


            }

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<IPagedList<EmployeeDTO>> GetEmployees(int pageNumber, int pageSize, string query)
        {
            var employees =
                from employee in _repo.GetAllIncluding(x => x.User)

                join department in _departmentRepo.GetAll() on employee.DepartmentId equals department.Id
                into departments
                from department in departments.DefaultIfEmpty()

                join terminal in _terminalRepo.GetAll() on employee.TerminalId equals terminal.Id
                into terminals
                from terminal in terminals.DefaultIfEmpty()

                let userRole = employee.User == null ? Enumerable.Empty<string>() : _userSvc.GetUserRoles(employee.User).Result

                where string.IsNullOrWhiteSpace(query) ||
                (employee.User.FirstName.Contains(query) ||
                    employee.User.LastName.Contains(query) ||
                    employee.User.Email.Contains(query) ||
                    employee.User.PhoneNumber.Contains(query) ||
                    employee.EmployeeCode.Contains(query))
                select new EmployeeDTO
                {
                    Id = employee.Id,
                    DateOfEmployment = employee.DateOfEmployment,
                    FirstName = employee.User.FirstName,
                    LastName = employee.User.LastName,
                    MiddleName = employee.User.MiddleName,
                    Gender = employee.User.Gender,
                    Email = employee.User.Email,
                    PhoneNumber = employee.User.PhoneNumber,
                    Address = employee.User.Address,
                    EmployeePhoto = employee.User.Photo,
                    NextOfKin = employee.User.NextOfKinName,
                    NextOfKinPhone = employee.User.NextOfKinPhone,
                    DepartmentName = department.Name,
                    DepartmentId = department.Id,
                    TerminalId = terminal.Id,
                    TerminalName = terminal.Name,
                    EmployeeCode = employee.EmployeeCode,
                    RoleName = string.Join(",", userRole)
                };

            return employees.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<EmployeeDTO> GetEmployee(int id)
        {
            var employee = _repo.GetAllIncluding(x => x.User).FirstOrDefault(x => x.Id == id);

            var employeeDto = new EmployeeDTO
            {
                Id = employee.Id,
                DateOfEmployment = employee.DateOfEmployment,
                FirstName = employee.User.FirstName,
                LastName = employee.User.LastName,
                MiddleName = employee.User.MiddleName,
                Gender = employee.User.Gender,
                Email = employee.User.Email,
                PhoneNumber = employee.User.PhoneNumber,
                Address = employee.User.Address,
                EmployeePhoto = employee.User.Photo,
                NextOfKin = employee.User.NextOfKinName,
                NextOfKinPhone = employee.User.NextOfKinPhone,
                DepartmentId = employee.DepartmentId,
                TerminalId = employee.TerminalId,
                EmployeeCode = employee.EmployeeCode,
            };

            var userRole = _userSvc.GetUserRoles(employee.User).Result.FirstOrDefault();

            if (!string.IsNullOrEmpty(userRole)) {
                var role = await _roleSvc.FindByName(userRole);
                employeeDto.RoleId = role.Id;
            }

            return employeeDto;
        }


        public async Task<int?> GetAssignedTerminal(string email)
        {
            var employee = await GetEmployeesByemailAsync(email);
            return employee?.TerminalId;
        }

        public async Task UpdateEmployee(int id, EmployeeDTO model)
        {
            var employee = _repo.Get(id);

            var user = await _userSvc.FindFirstAsync(x => x.Id == employee.UserId);

            if (user is null) {
                throw new EntityNotFoundException(typeof(User), id);
            }

            employee.EmployeeCode = model.EmployeeCode;
            employee.DepartmentId = model.DepartmentId;
            employee.TerminalId = model.TerminalId;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.Gender = model.Gender;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.NextOfKinName = model.NextOfKin;
            user.NextOfKinPhone = model.NextOfKinPhone;


            if (model.RoleId != 0) {
                var newRole = await _roleSvc.FindByIdAsync(model.RoleId);
                if (newRole != null && !await _userSvc.IsInRoleAsync(user, newRole.Name)) {
                    await _userSvc.RemoveFromRolesAsync(user, await _userSvc.GetUserRoles(user));
                    await _userSvc.AddToRoleAsync(user, newRole.Name);
                }
            }

            await _userSvc.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}