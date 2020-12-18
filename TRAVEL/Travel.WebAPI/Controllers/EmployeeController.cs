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
    public class EmployeeController : BaseController
    {
        private readonly IUserService _userManagerSvc;
        private readonly IEmployeeService _employeeSvc;
        private readonly IServiceHelper _serviceHelper;

        public EmployeeController(IEmployeeService employeeSvc, IServiceHelper serviceHelper, IUserService userManagerSvc)
        {
            _employeeSvc = employeeSvc;
            _userManagerSvc = userManagerSvc;
            _serviceHelper = serviceHelper;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddEmployee(EmployeeDTO employee)
        {
            return await HandleApiOperationAsync(async () => {
                await _employeeSvc.AddEmployee(employee);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<EmployeeDTO>> GetEmployee(int id)
        {
            return await HandleApiOperationAsync(async () => {
                var employee = await _employeeSvc.GetEmployee(id);

                return new ServiceResponse<EmployeeDTO>(employee);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateEmployee(int id, EmployeeDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                await _employeeSvc.UpdateEmployee(id, model);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("GetEmployeebyMail")]
        public async Task<IServiceResponse<EmployeeDTO>> GetEmployeeByMail()
        {
            return await HandleApiOperationAsync(async () => {
                var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
                var employee = await _employeeSvc.GetEmployeesByemailAsync(email.Email);

                return new ServiceResponse<EmployeeDTO>
                {
                    Object = employee
                };
            });
        }

        [HttpGet]
        [Route("GetEmployeeTerminal/{email}")]
        public async Task<ServiceResponse<int?>> GetEmployeeTerminal(string email)
        {
            return await HandleApiOperationAsync(async () => {
                var employeeTerminalId = await _employeeSvc.GetAssignedTerminal(email);

                return new ServiceResponse<int?>(employeeTerminalId);
            });
        }

        [HttpGet]
        [Route("loginemployee")]
        public async Task<IServiceResponse<List<EmployeeDTO>>> GetLoginEmployee()
        {
            var username = User.Identity.Name;
            return await HandleApiOperationAsync(async () => {
                var email = await _userManagerSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());
                var terminalid = await _employeeSvc.GetAssignedTerminal(email.Email);
                var loginEmployees = await _employeeSvc.GetTerminalEmployees(terminalid.GetValueOrDefault());

                return new ServiceResponse<List<EmployeeDTO>>
                {
                    Object = loginEmployees
                };
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{search}")]
        public async Task<IServiceResponse<IPagedList<EmployeeDTO>>> GetEmployees(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string search = null)
        {
            return await HandleApiOperationAsync(async () => {
                var employees = await _employeeSvc.GetEmployees(pageNumber, pageSize, search);

                return new ServiceResponse<IPagedList<EmployeeDTO>>
                {
                    Object = employees
                };
            });
        }
    }
}