using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Core.Timing;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using IPagedList;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;

namespace Travel.Business.Services
{
    public interface ITerminalService
    {
        Task<IPagedList<TerminalDTO>> GetTerminals(int page, int size, string search = null);
        Task<TerminalDTO> GetTerminalById(int terminalId);
        Task UpdateTerminal(int terminalId, TerminalDTO terminal);
        Task RemoveTerminal(int terminalId);
        Task<List<TerminalDTO>> GetEmployeeTerminal(int userId);
        Task<TerminalDTO> GetLoginEmployeeTerminal(string username);
        Task AddTerminal(TerminalDTO terminal);
        Task<List<EmployeeDTO>> GetTerminalTicketers(int terminalId);
        Task<List<EmployeeDTO>> GetTerminalAccountants(int terminalId);
        Task<List<RouteDTO>> GetTerminalRoutes(int terminalId);
    }

    public class TerminalService : ITerminalService
    {
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IEmployeeService _employeeSvc;
        private readonly IRepository<State> _stateRepo;
        private readonly IRepository<Route> _routeRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userSvc;

        public TerminalService(IRepository<Terminal> terminalRepo,
            IEmployeeService employeeSvc, IRepository<State> stateRepo,
            IServiceHelper serviceHelper, IUnitOfWork unitOfWork,
            IUserService userSvc, IRepository<Route> routeRepo)
        {
            _terminalRepo = terminalRepo;
            _employeeSvc = employeeSvc;
            _stateRepo = stateRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
            _userSvc = userSvc;
            _routeRepo = routeRepo;
        }

        private async Task<bool> IsValidState(int stateId)
        {
            return stateId > 0 &&
                 await _stateRepo.ExistAsync(m => m.Id == stateId);
        }

        public async Task AddTerminal(TerminalDTO terminalDto)
        {
            if (!await IsValidState(terminalDto.StateId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            terminalDto.Name = terminalDto.Name.Trim();


            if (await _terminalRepo.ExistAsync(v => v.Name.Equals(terminalDto.Name))) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TERMINAL_EXIST);
            }

            if (terminalDto.StartDate is null) {
                terminalDto.StartDate = Clock.Now;
            }

            _terminalRepo.Insert(new Terminal
            {
                Id = terminalDto.Id,
                Name = terminalDto.Name,
                Code = terminalDto.Code,
                Image = terminalDto.Image,
                Address = terminalDto.Address,
                ContactPerson = terminalDto.ContactPerson,
                ContactPersonNo = terminalDto.ContactPersonNo,
                Latitude = terminalDto.Latitude,
                Longitude = terminalDto.Longitude,
                StateId = terminalDto.StateId,
                TerminalType = terminalDto.TerminalType,
                TerminalStartDate = terminalDto.StartDate.GetValueOrDefault()
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<List<TerminalDTO>> GetEmployeeTerminal(int userId)
        {
            var employeeterminalId = _employeeSvc.FirstOrDefault(x => x.UserId == userId);

            var employeeTerminals =
                from employeeterminal in _terminalRepo.GetAll()
                where employeeterminal.Id == employeeterminalId.TerminalId

                select new TerminalDTO
                {
                    Id = employeeterminal.Id,
                    Name = employeeterminal.Name,
                    StateId = employeeterminal.StateId,
                    RouteId = employeeterminal.Id,
                    TerminalType = employeeterminal.TerminalType
                };

            return employeeTerminals.AsNoTracking().ToListAsync();
        }

        public Task<TerminalDTO> GetLoginEmployeeTerminal(string username)
        {
            var employeeTerminals =
             from employee in _employeeSvc.GetAll()
             join terminal in _terminalRepo.GetAll() on employee.TerminalId equals terminal.Id
             where employee.User.Email == username

             select new TerminalDTO
             {
                 Id = (int) employee.TerminalId,
                 Name = terminal.Name,
                 TerminalType = terminal.TerminalType,
                 Latitude = terminal.Latitude,
                 Longitude = terminal.Longitude
             };

            return employeeTerminals.FirstOrDefaultAsync();
        }

        public async Task<List<EmployeeDTO>> GetTerminalAccountants(int terminalId)
        {
            var terminal = await GetTerminalById(terminalId);

            var terminalEmployees = await _employeeSvc.GetTerminalEmployees(terminalId);

            string accountant = "TerminalAccountant";
            var usersInRole = await _userSvc.GetUsersInRoleAsync(accountant);

            var accountants = new List<EmployeeDTO>();

            foreach (var employee in terminalEmployees) {
                foreach (var user in usersInRole) {
                    if (employee.Email.Equals(user.Email)) {
                        accountants.Add(employee);
                    }
                }
            }
            return accountants;
        }

        public async Task<TerminalDTO> GetTerminalById(int terminalId)
        {
            var terminal = await _terminalRepo.GetAsync(terminalId);

            if (terminal == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TERMINAL_EXIST);
            }

            return new TerminalDTO
            {
                Id = terminal.Id,
                TerminalType = terminal.TerminalType,
                IsNew = terminal.IsNew,
                StartDate = terminal.TerminalStartDate,
                BookingType = BookingTypes.Terminal,
                Code = terminal.Code,
                Image = terminal.Image,
                Latitude = terminal.Latitude,
                Longitude = terminal.Longitude,
                Name = terminal.Name,
                Address = terminal.Address,
                ContactPerson = terminal.ContactPerson,
                ContactPersonNo = terminal.ContactPersonNo,
                StateId = terminal.StateId,
            };
        }

        public Task<IPagedList<TerminalDTO>> GetTerminals(int page, int size, string search = null)
        {
            var terminals =
                from terminal in _terminalRepo.GetAll()
                join state in _stateRepo.GetAll() on terminal.StateId equals state.Id
                where (string.IsNullOrWhiteSpace(search) || terminal.Name.Contains(search))
                 && terminal.TerminalType == TerminalType.Physical
                orderby state.Name
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
                    Address = terminal.Address
                };

            return terminals.AsNoTracking().ToPagedListAsync(page, size);
        }

        public Task<List<EmployeeDTO>> GetTerminalTicketers(int terminalId)
        {
            throw new System.NotImplementedException();
        }

        public async Task RemoveTerminal(int terminalId)
        {
            var terminal = await _terminalRepo.GetAsync(terminalId);

            if (terminal == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TERMINAL_NOT_EXIST);
            }

            _terminalRepo.Delete(terminal);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateTerminal(int terminalId, TerminalDTO terminal)
        {
            var terminalresult = await _terminalRepo.GetAsync(terminalId);

            if (terminalresult == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TERMINAL_NOT_EXIST);
            }

            terminalresult.Name = terminal.Name;
            terminalresult.Code = terminal.Code;
            terminalresult.Address = terminal.Address;
            terminalresult.ContactPerson = terminal.ContactPerson;
            terminalresult.ContactPersonNo = terminal.ContactPersonNo;
            terminalresult.Latitude = terminal.Latitude;
            terminalresult.Longitude = terminal.Longitude;
            terminalresult.TerminalType = terminal.TerminalType;

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<List<RouteDTO>> GetTerminalRoutes(int terminalId)
        {
            var routes =
                from route in _routeRepo.GetAll()
                where route.DepartureTerminalId == terminalId && route.AvailableAtTerminal

                select new RouteDTO
                {
                    Id = route.Id,
                    Name = route.Name,
                    RouteType = route.Type,
                    AvailableOnline = route.AvailableOnline,
                    AvailableAtTerminal = route.AvailableAtTerminal
                };

            return routes.AsNoTracking().ToListAsync();
        }
    }
}