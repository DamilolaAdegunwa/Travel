using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Core.Timing;
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
    public interface IFareService
    {
        Task<Fare> GetAsync(int id);
        IQueryable<Fare> GetAll();
        Task<IPagedList<FareDTO>> GetFares(int page, int size, string q = null);
        Task<FareDTO> GetFareByVehicleTrip(int subRouteNameId, int vehicleModel);
        Fare FirstOrDefault(Expression<Func<Fare, bool>> predicate);
        Task<FareDTO> GetFareById(int fareId);
        Task AddFare(FareDTO fare);
        Task UpdateFare(int fareId, FareDTO fare);
        Task DeleteFare(int fareId);
        decimal GetFareByRouteId(int routeId, int modelId);
    }

    public class FareService : IFareService
    {
        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<Fare> _repo;
        private readonly IRepository<FareCalendar> _fareCalendarRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        private readonly IFareCalendarService _fareCalendarSvc;
        private readonly IRouteService _routeSvc;

        public FareService(IRepository<Route> routeRepo,
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<Fare> repo,
            IRepository<FareCalendar> fareCalendarRepo,
            IUnitOfWork unitOfWork,
            IServiceHelper serviceHelper, IFareCalendarService fareCalendarSvc, IRouteService routeSvc)
        {
            _routeRepo = routeRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _repo = repo;
            _routeSvc = routeSvc;
            _fareCalendarRepo = fareCalendarRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
            _fareCalendarSvc = fareCalendarSvc;
        }

        public Fare FirstOrDefault(Expression<Func<Fare, bool>> predicate)
        {
            return _repo.FirstOrDefault(predicate);
        }

        public IQueryable<Fare> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<Fare> GetAsync(int id)
        {
            return _repo.GetAsync(id);
        }


        public async Task<FareDTO> GetFareByVehicleTrip(int routeId, int vehicleModel)
        {
            var departureDate = Clock.Now.Date;

            //var fareCalendar = _fareCalendarRepo.FirstOrDefault(c => departureDate >= c.StartDate && c.EndDate >= departureDate && routeId == c.RouteId);

            //var fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault();

            var fares =
                from fare in GetAll()
                join route in _routeRepo.GetAll() on fare.RouteId equals route.Id
                join vehiclemodel in _vehicleModelRepo.GetAll() on fare.VehicleModelId equals vehiclemodel.Id
                where fare.RouteId == routeId && fare.VehicleModelId == vehicleModel
                select new FareDTO
                {
                    Id = fare.Id,
                    Name = fare.Name,
                    Amount = fare.Amount,
                    RouteId = route.Id,
                    RouteName = route.Name,
                    VehicleModelId = vehiclemodel.Id,
                    VehicleModelName = vehiclemodel.Name,
                    ChildrenDiscountPercentage = fare.ChildrenDiscountPercentage
                };

            var computedfare = await fares.AsNoTracking().FirstOrDefaultAsync();
            if (computedfare != null)
            {


                var fareCalendar = new FareCalendarDTO();
                fareCalendar = null;
                var farecalendarList = new List<FareCalendarDTO>();

                farecalendarList = await _fareCalendarSvc.GetFareCalendaListByRoutesAsync(routeId, departureDate);
                foreach (var calendar in farecalendarList)
                {
                    if (calendar.VehicleModelId != null)
                    {
                        if (vehicleModel == calendar.VehicleModelId)
                        {
                            fareCalendar = calendar;

                        }
                        break;
                    }

                    fareCalendar = calendar;
                }
                if (fareCalendar == null)
                {
                    var DepartureterminalId = await _routeSvc.GetDepartureterminalIdFromRoute(routeId);
                    farecalendarList = await _fareCalendarSvc.GetFareCalendaListByTerminalsAsync(DepartureterminalId, departureDate);
                    foreach (var calendar in farecalendarList)
                    {

                        if (calendar.VehicleModelId != null)
                        {
                            if (vehicleModel == calendar.VehicleModelId)
                            {
                                fareCalendar = calendar;

                            }
                            break;
                        }

                        fareCalendar = calendar;
                    }
                }


                decimal fareDifference = 0;
                if (fareCalendar != null)
                {
                    if (fareCalendar.FareAdjustmentType == FareAdjustmentType.Percentage)
                    {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * ((fareCalendar?.FareValue).GetValueOrDefault(0.0m) * computedfare.Amount / 100);

                    }
                    else
                    {
                        fareDifference = (fareCalendar?.FareType == FareType.Increase ? 1 : -1) * (fareCalendar?.FareValue).GetValueOrDefault(0.0m);

                    }
                }

                computedfare.Amount += fareDifference;
            }

            return computedfare;
        }

        public async Task AddFare(FareDTO fareDto)
        {
            if (await IsDefinedFareAsync(fareDto.RouteId, fareDto.VehicleModelId))
            {
                throw new LMEGenericException($"Fare already exist!");

            }

            if (!await IsValidRoute(fareDto.RouteId))
            {
                throw new LMEGenericException($"Route does not exist!");
            }

            if (!await IsValidVehicleModel(fareDto.VehicleModelId))
            {
                throw new LMEGenericException($"Route does not exist!");
            }

            _repo.Insert(new Fare
            {
                Amount = fareDto.Amount,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                RouteId = fareDto.RouteId,
                VehicleModelId = fareDto.VehicleModelId,
                ChildrenDiscountPercentage = fareDto.ChildrenDiscountPercentage
            });

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> IsDefinedFareAsync(int routeId, int vehicleModelId)
        {
            return await _repo.ExistAsync(f => f.RouteId == routeId && f.VehicleModelId == vehicleModelId);
        }

        private async Task<bool> IsValidRoute(int routeId)
        {
            return routeId > 0 &&
                 await _routeRepo.ExistAsync(m => m.Id == routeId);
        }

        private async Task<bool> IsValidVehicleModel(int vehicleModelId)
        {
            return vehicleModelId > 0 &&
                 await _vehicleModelRepo.ExistAsync(m => m.Id == vehicleModelId);
        }

        public Task<IPagedList<FareDTO>> GetFares(int page, int size, string q = null)
        {
            var fares =
                from fare in _repo.GetAll()
                join route in _routeRepo.GetAll() on fare.RouteId equals route.Id
                join vehiclemodel in _vehicleModelRepo.GetAll() on fare.VehicleModelId equals vehiclemodel.Id
                where string.IsNullOrWhiteSpace(q) || fare.Name.Contains(q)
                orderby fare.CreationTime descending
                select new FareDTO
                {
                    Id = fare.Id,
                    Name = fare.Name,
                    Amount = fare.Amount,
                    RouteId = route.Id,
                    RouteName = route.Name,
                    VehicleModelId = vehiclemodel.Id,
                    VehicleModelName = vehiclemodel.Name,
                    ChildrenDiscountPercentage = fare.ChildrenDiscountPercentage
                };

            return fares.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task<FareDTO> GetFareById(int fareId)
        {
            var fare = await _repo.GetAsync(fareId);

            if (fare == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }

            return new FareDTO
            {
                Id = fare.Id,
                Name = fare.Name,
                Amount = fare.Amount,
                RouteId = fare.RouteId,
                RouteName = fare?.Route?.Name,
                VehicleModelId = fare.VehicleModelId,
                VehicleModelName = fare?.VehicleModel?.Name,
                ChildrenDiscountPercentage = fare.ChildrenDiscountPercentage
            };
        }

        public async Task UpdateFare(int fareId, FareDTO dto)
        {
            var fare = await _repo.GetAsync(fareId);

            if (fare == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }

            if (fare.RouteId != dto.RouteId || fare.VehicleModelId != dto.VehicleModelId)
            {
                if (await IsDefinedFareAsync(dto.RouteId, dto.VehicleModelId))
                {
                    throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_EXIST);
                }
            }

            fare.Amount = dto.Amount;
            fare.RouteId = dto.RouteId;
            fare.VehicleModelId = dto.VehicleModelId;
            fare.ChildrenDiscountPercentage = dto.ChildrenDiscountPercentage;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteFare(int fareId)
        {
            var fare = await _repo.GetAsync(fareId);

            if (fare == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }

            _repo.Delete(fare);

            await _unitOfWork.SaveChangesAsync();
        }

        public decimal GetFareByRouteId(int routeId, int modelId)
        {
            var farerefcode = _repo.GetAll().Where(a => a.RouteId == routeId && a.VehicleModelId == modelId);           
            return farerefcode.FirstOrDefault().Amount;
        }
    }
}