using Travel.Core.Domain.Entities;
using Travel.Data.Repository;
using System;
using System.Threading.Tasks;
using System.Linq;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Travel.Core.Domain.DataTransferObjects;
using IPagedList;
using Travel.Core.Exceptions;
using Travel.Core.Domain.Entities.Enums;
using System.Collections.Generic;

namespace Travel.Business.Services
{
    public interface IFareCalendarService
    {
        Task<FareCalendarDTO> GetFareCalendaByRoutesAsync(int routeId, DateTime departureDate);
        Task<List<FareCalendarDTO>> GetFareCalendaListByRoutesAsync(int routeId, DateTime departureDate);
        Task<FareCalendarDTO> GetFareCalendaByTerminalsAsync(int terminalId, DateTime departureDate);
        Task<List<FareCalendarDTO>> GetFareCalendaListByTerminalsAsync(int terminalId, DateTime departureDate);

        Task<IPagedList<FareCalendarDTO>> GetFareCalendarsAsync(int page, int size, string q = null);
        Task<FareCalendarDTO> GetFareCalendarByIdAsync(int Id);
        Task AddFareCalendar(FareCalendarDTO calendar);
        Task UpdateFareCalendar(int Id, FareCalendarDTO fareCalendarDto);
        Task DeleteFareCalendar(int Id);
    }

    public class FareCalendarService : IFareCalendarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<FareCalendar> _repo;
        private readonly IServiceHelper _serviceHelper;

        public FareCalendarService(IUnitOfWork unitOfWork,
            IRepository<Route> routeRepo,
            IRepository<Terminal> terminalRepo,
            IRepository<FareCalendar> repo, IServiceHelper serviceHelper)
        {
            _unitOfWork = unitOfWork;
            _routeRepo = routeRepo;
            _terminalRepo = terminalRepo;
            _repo = repo;
            _serviceHelper = serviceHelper;
        }

        public Task<FareCalendarDTO> GetFareCalendaByRoutesAsync(int routeId, DateTime departureDate)
        {
            var date = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);


            var calendarManagements =
                from calendar in _repo.GetAllIncluding(x=>x.VehicleModel)
                join route in _routeRepo.GetAll() on calendar.RouteId equals route.Id

                where date >= calendar.StartDate.Date && calendar.EndDate.Date >= date && route.Id == routeId

                select new FareCalendarDTO
                {
                    Id = calendar.Id,
                    Name = calendar.Name,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate,
                    VehicleModelId = calendar.VehicleModelId,
                    VehicleModelName = calendar.VehicleModelId != null ? calendar.VehicleModel.Name : "",
                    RouteId = calendar.RouteId,
                    RouteName = route.Name,
                    FareAdjustmentType = calendar.FareAdjustmentType,
                    FareParameterType = calendar.FareParameterType,
                    FareType = calendar.FareType,
                    FareTypeName =calendar.FareType.ToString(),
                    FareValue = calendar.FareValue
                };

            return calendarManagements.AsNoTracking().FirstOrDefaultAsync();
        }
        public Task<List<FareCalendarDTO>> GetFareCalendaListByRoutesAsync(int routeId, DateTime departureDate)
        {
            var date = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);


            var calendarManagements =
                from calendar in _repo.GetAllIncluding(x => x.VehicleModel)
                join route in _routeRepo.GetAll() on calendar.RouteId equals route.Id

                where date >= calendar.StartDate.Date && calendar.EndDate.Date >= date && route.Id == routeId

                select new FareCalendarDTO
                {
                    Id = calendar.Id,
                    Name = calendar.Name,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate,
                    VehicleModelId = calendar.VehicleModelId,
                    VehicleModelName = calendar.VehicleModelId != null ? calendar.VehicleModel.Name : "",
                    RouteId = calendar.RouteId,
                    RouteName = route.Name,
                    FareAdjustmentType = calendar.FareAdjustmentType,
                    FareParameterType = calendar.FareParameterType,
                    FareType = calendar.FareType,
                    FareTypeName = calendar.FareType.ToString(),
                    FareValue = calendar.FareValue
                };

            return calendarManagements.AsNoTracking().ToListAsync();
        }

        public Task<FareCalendarDTO> GetFareCalendaByTerminalsAsync(int terminalId, DateTime departureDate)
        {
            var date = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);


            var calendarManagements =
                from calendar in _repo.GetAllIncluding(x=>x.VehicleModel)
                join terminal in _terminalRepo.GetAll() on calendar.TerminalId equals terminal.Id

                where date >= calendar.StartDate.Date && calendar.EndDate.Date >= date && terminal.Id == terminalId

                select new FareCalendarDTO
                {
                    Id = calendar.Id,
                    Name = calendar.Name,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate,
                    VehicleModelId = calendar.VehicleModelId,
                    VehicleModelName = calendar.VehicleModelId != null ? calendar.VehicleModel.Name : "",
                    TerminalId = calendar.TerminalId,
                    TerminalName = terminal.Name,
                    FareAdjustmentType=calendar.FareAdjustmentType,
                    FareParameterType=calendar.FareParameterType,
                    FareType = calendar.FareType,
                    FareTypeName = calendar.FareType.ToString(),
                    FareValue = calendar.FareValue
                };

            return calendarManagements.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<List<FareCalendarDTO>> GetFareCalendaListByTerminalsAsync(int terminalId, DateTime departureDate)
        {
            var date = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);


            var calendarManagements =
                from calendar in _repo.GetAllIncluding(x => x.VehicleModel)
                join terminal in _terminalRepo.GetAll() on calendar.TerminalId equals terminal.Id

                where date >= calendar.StartDate.Date && calendar.EndDate.Date >= date && terminal.Id == terminalId

                select new FareCalendarDTO
                {
                    Id = calendar.Id,
                    Name = calendar.Name,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate,
                    VehicleModelId = calendar.VehicleModelId,
                    VehicleModelName = calendar.VehicleModelId != null ? calendar.VehicleModel.Name : "",
                    TerminalId = calendar.TerminalId,
                    TerminalName = terminal.Name,
                    FareAdjustmentType = calendar.FareAdjustmentType,
                    FareParameterType = calendar.FareParameterType,
                    FareType = calendar.FareType,
                    FareTypeName = calendar.FareType.ToString(),
                    FareValue = calendar.FareValue
                };

            return calendarManagements.AsNoTracking().ToListAsync();
        }

        public Task<IPagedList<FareCalendarDTO>> GetFareCalendarsAsync(int page, int size, string q = null)
        {

            var calendarManagements =
                from calendar in _repo.GetAllIncluding(x => x.Route, y => y.Terminal)

             
                select new FareCalendarDTO
                {
                    Id = calendar.Id,
                    Name = calendar.Name,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate,
                    FareTypeName = calendar.FareType.ToString(),
                    FareAdjustmentTypeName = calendar.FareAdjustmentType.ToString(),
                    FareParameterTypeName= calendar.FareParameterType.ToString(),
                    VehicleModelId = calendar.VehicleModelId,
                    VehicleModelName = calendar.VehicleModelId != null ? calendar.VehicleModel.Name : "",
                    RouteId = calendar.RouteId,
                    RouteName = calendar.FareParameterType==FareParameterType.Route? calendar.Route.Name: "",
                    TerminalId=calendar.TerminalId,
                    TerminalName= calendar.FareParameterType == FareParameterType.Terminal ? calendar.Terminal.Name : "",
                    FareAdjustmentType = calendar.FareAdjustmentType,
                    FareParameterType = calendar.FareParameterType,
                    FareType = calendar.FareType,
                    FareValue = calendar.FareValue
                };


            return calendarManagements.AsNoTracking().ToPagedListAsync(page, size); ;
        }

        public Task<FareCalendarDTO> GetFareCalendarByIdAsync(int Id)
        {
            var calendarManagements =
               from calendar in _repo.GetAllIncluding(x => x.Route, y => y.Terminal,z=>z.VehicleModel)
               where calendar.Id==Id

               select new FareCalendarDTO
               {
                   Id = calendar.Id,
                   Name = calendar.Name,
                   StartDate = calendar.StartDate,
                   EndDate = calendar.EndDate,
                   FareTypeName = calendar.FareType.ToString(),
                   FareAdjustmentTypeName = calendar.FareAdjustmentType.ToString(),
                   FareParameterTypeName = calendar.FareParameterType.ToString(),
                   VehicleModelId=calendar.VehicleModelId,
                   VehicleModelName=calendar.VehicleModelId != null?calendar.VehicleModel.Name:"",
                   RouteId = calendar.RouteId,
                   RouteName = calendar.FareParameterType == FareParameterType.Route ? calendar.Route.Name : "",
                   TerminalId = calendar.TerminalId,
                   TerminalName = calendar.FareParameterType == FareParameterType.Terminal ? calendar.Terminal.Name : "",
                   FareAdjustmentType = calendar.FareAdjustmentType,
                   FareParameterType = calendar.FareParameterType,
                   FareType = calendar.FareType,
                   FareValue = calendar.FareValue
               };

            return calendarManagements.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task AddFareCalendar(FareCalendarDTO calendar)
        {
          


            _repo.Insert(new FareCalendar
            {
                Name = calendar.Name,
                StartDate = calendar.StartDate,
                EndDate = calendar.EndDate,
                RouteId = calendar.RouteId,
                VehicleModelId=calendar.VehicleModelId,
                TerminalId=calendar.TerminalId,
                FareAdjustmentType=calendar.FareAdjustmentType,
                FareParameterType=calendar.FareParameterType,
                FareType = calendar.FareType,
                FareValue = calendar.FareValue
            });

            await _unitOfWork.SaveChangesAsync();
        }




        public async Task UpdateFareCalendar(int Id, FareCalendarDTO fareCalendarDto)
        {
            var calendar = await _repo.GetAsync(Id);

            if (calendar == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FareCalendar_NOT_EXIST);
            }



            calendar.Name = fareCalendarDto.Name.Trim();
            calendar.StartDate = fareCalendarDto.StartDate;
            calendar.EndDate = fareCalendarDto.EndDate;
            calendar.RouteId = fareCalendarDto.RouteId;
            calendar.TerminalId = fareCalendarDto.TerminalId;
            calendar.FareParameterType = fareCalendarDto.FareParameterType;
            calendar.FareAdjustmentType = fareCalendarDto.FareAdjustmentType;
            calendar.VehicleModelId = fareCalendarDto.VehicleModelId;
            calendar.FareType = fareCalendarDto.FareType;
            calendar.FareValue = fareCalendarDto.FareValue;

            await _unitOfWork.SaveChangesAsync();
        }


        public async Task DeleteFareCalendar(int Id)
        {
            var farecalendar = await _repo.GetAsync(Id);

            if (farecalendar == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FareCalendar_NOT_EXIST);
            }

            _repo.Delete(farecalendar);

            await _unitOfWork.SaveChangesAsync();
        }

    }
}