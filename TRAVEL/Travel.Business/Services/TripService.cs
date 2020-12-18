using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using IPagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface ITripService
    {
        Task<IPagedList<TripDTO>> GetTrips(int page, int size, string query = null);
        Task<TripDTO> GetTripById(Guid tripId);
        Task<List<TripDTO>> GetTripByRouteId(int routeId);
        Task<List<TripDTO>> GetTripsByVehicleId(string vehicleRegId);
        Task RemoveTrip(Guid tripId);
        Task UpdateTrip(Guid tripId, TripDTO trip);
        Task AddTrip(TripDTO trip);
        Task ToggleOnlineStatus(Guid tripId);
        Task<Trip> GetAsync(Guid guid, params Expression<Func<Trip, object>>[] properties );
        IQueryable<Trip> GetAll();
        Trip FirstOrDefault(Expression<Func<Trip, bool>> filter);
    }

    public class TripService : ITripService
    {
        private readonly IRepository<Trip, Guid> _tripRepo;
        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<JourneyManagement, Guid> _journeyMgtRepo;
        private readonly IRepository<VehicleTripRegistration, Guid> _vehicleTripRegRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;

        public TripService(IRepository<Trip, Guid> tripRepo,
            IRepository<Route> routeRepo,
            IRepository<VehicleModel> vehicleModelRepo,
            IRepository<JourneyManagement, Guid> journeyMgtRepo,
            IRepository<VehicleTripRegistration, Guid> vehicleTripRegRepo,
            IServiceHelper serviceHelper,
            IUnitOfWork unitOfWork)
        {
            _tripRepo = tripRepo;
            _routeRepo = routeRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _journeyMgtRepo = journeyMgtRepo;
            _vehicleTripRegRepo = vehicleTripRegRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task AddTrip(TripDTO tripDto)
        {
            if (!await IsValidRoute(tripDto.RouteId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            if (!await IsValidVehicleModel(tripDto.VehicleModelId.GetValueOrDefault())) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            _tripRepo.Insert(new Trip
            {
                DepartureTime = tripDto.DepartureTime,
                TripCode = tripDto.Code,
                RouteId = tripDto.RouteId,
                ParentRouteId = tripDto.ParentRouteId,
                ParentRouteDepartureTime = tripDto.ParentDepartureTime,
                ParentTripId = tripDto.ParentTripId,
                VehicleModelId = tripDto.VehicleModelId,
                AvailableOnline = tripDto.AvailableOnline
            });

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> IsValidRoute(int routeId)
        {
            return routeId > 0 && await _routeRepo.ExistAsync(m => m.Id == routeId);
        }

        public Task<TripDTO> GetTripById(Guid tripId)
        {
            var trips =
                from trip in _tripRepo.GetAll()
                join route in _routeRepo.GetAll() on trip.RouteId equals route.Id
                join parentroute in _routeRepo.GetAll() on trip.ParentRouteId equals parentroute.Id
                into parentroutes
                from parentroute in parentroutes.DefaultIfEmpty()

                join parentrip in _tripRepo.GetAll() on trip.ParentTripId equals parentrip.Id
                into parentrips
                from parentrip in parentrips.DefaultIfEmpty()
                join vehiclemodel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehiclemodel.Id
                where trip.Id == tripId

                select new TripDTO
                {
                    Id = trip.Id,
                    RouteId = trip.RouteId,
                    ParentRouteId = trip.ParentRouteId,
                    ParentRouteName = parentroute.Name,
                    ParentDepartureTime = parentrip.DepartureTime,
                    RouteName = route.Name + " " + trip.DepartureTime,
                    VehicleModelId = trip.VehicleModelId,
                    VehicleModelName = vehiclemodel.Name,
                    DepartureTime = trip.DepartureTime,
                    Code = trip.TripCode,
                    AvailableOnline = trip.AvailableOnline
                };

            return trips.FirstOrDefaultAsync();
        }

        public Task<List<TripDTO>> GetTripByRouteId(int routeId)
        {
            var trips =
               from trip in _tripRepo.GetAll()
               join route in _routeRepo.GetAll() on trip.RouteId equals route.Id

               join parentroute in _routeRepo.GetAll() on trip.ParentRouteId equals parentroute.Id
               into parentroutes
               from parentroute in parentroutes.DefaultIfEmpty()

               join parentrip in _tripRepo.GetAll() on trip.ParentTripId equals parentrip.Id
              into parentrips
               from parentrip in parentrips.DefaultIfEmpty()

               join vehicleModel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehicleModel.Id
               where trip.RouteId == routeId

               select new TripDTO
               {
                   Id = trip.Id,
                   RouteId = trip.RouteId,
                   ParentRouteId = trip.ParentRouteId,
                   ParentRouteName = parentroute.Name,
                   ParentDepartureTime = parentrip.DepartureTime,
                   Code = trip.TripCode,
                   VehicleModelId = vehicleModel.Id,
                   RouteName = route.Name + " " + trip.DepartureTime + "(" + vehicleModel.Name + ")",
                   DepartureTime = trip.DepartureTime,
                   AvailableOnline = trip.AvailableOnline
               };

            return trips.AsNoTracking().ToListAsync();
        }

        public Task<IPagedList<TripDTO>> GetTrips(int pageNumber, int pageSize, string query = null)
        {
            var trips =
                from trip in _tripRepo.GetAll()
                join route in _routeRepo.GetAll() on trip.RouteId equals route.Id

                join parentroute in _routeRepo.GetAll() on trip.ParentRouteId equals parentroute.Id
                into parentroutes
                from parentroute in parentroutes.DefaultIfEmpty()

                join parentrip in _tripRepo.GetAll() on trip.ParentTripId equals parentrip.Id
                into parentrips
                from parentrip in parentrips.DefaultIfEmpty()

                join vehiclemodel in _vehicleModelRepo.GetAll() on trip.VehicleModelId equals vehiclemodel.Id
                where string.IsNullOrWhiteSpace(query) || trip.TripCode.Contains(query) || route.Name.Contains(query) || trip.DepartureTime.Contains(query) || parentroute.Name.Contains(query) 

                select new TripDTO
                {
                    Id = trip.Id,
                    DepartureTime = trip.DepartureTime,
                    RouteId = route.Id,
                    RouteName = route.Name,
                    ParentRouteId = trip.ParentRouteId,
                    ParentRouteName = parentroute.Name,
                    ParentDepartureTime = parentrip.DepartureTime,
                    Code = trip.TripCode,
                    VehicleModelId = vehiclemodel.Id,
                    VehicleModelName = vehiclemodel.Name,
                    AvailableOnline = trip.AvailableOnline
                };

            return trips.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<List<TripDTO>> GetTripsByVehicleId(string vehicleRegId)
        {
            var trips = from trip in _tripRepo.GetAll()
                        join vtr in _vehicleTripRegRepo.GetAll() on trip.Id equals vtr.TripId
                        join route in _routeRepo.GetAll() on trip.RouteId equals route.Id
                        join journyMgt in _journeyMgtRepo.GetAll()
                             on vtr.Id equals journyMgt.VehicleTripRegistrationId

                        where vtr.PhysicalBusRegistrationNumber == vehicleRegId && vtr.IsBlownBus == false

                        select new TripDTO
                        {
                            RouteName = route.Name,
                            DepartureTime = trip.DepartureTime,
                            DepartureDate = vtr.DepartureDate.ToString(),
                            JourneyStatus = journyMgt.JourneyStatus.ToString(),
                            DriverCode = vtr.DriverCode,
                            VehicleTripRegId = vtr.Id.ToString()
                        };

            return await Task.FromResult(trips.ToList());
        }

        public async Task RemoveTrip(Guid tripId)
        {
            var trip = await _tripRepo.GetAsync(tripId);

            if (trip is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TRIP_NOT_EXIST);
            }

            _tripRepo.Delete(trip);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateTrip(Guid tripId, TripDTO tripDto)
        {
            var trip = await _tripRepo.GetAsync(tripId);

            if (trip is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TRIP_NOT_EXIST);
            }

            if (!await IsValidVehicleModel(tripDto.VehicleModelId.GetValueOrDefault())) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.VEHICLE_MODEL_NOT_EXIST);
            }

            trip.DepartureTime = tripDto.DepartureTime;
            trip.ParentTripId = tripDto.ParentTripId;
            trip.TripCode = tripDto.Code;
            trip.RouteId = tripDto.RouteId;
            trip.ParentRouteId = tripDto.ParentRouteId;
            trip.VehicleModelId = tripDto.VehicleModelId;
            trip.AvailableOnline = tripDto.AvailableOnline;
            trip.ParentRouteDepartureTime = tripDto.ParentDepartureTime;

            await _unitOfWork.SaveChangesAsync();
        }


        private async Task<bool> IsValidVehicleModel(int vehicleModelId)
        {
            return vehicleModelId > 0 &&
                 await _vehicleModelRepo.ExistAsync(m => m.Id == vehicleModelId);
        }

        public async Task ToggleOnlineStatus(Guid tripId)
        {
            var trips = await _tripRepo.GetAsync(tripId);

            if (trips == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.TRIP_NOT_EXIST);
            }
            trips.AvailableOnline = !trips.AvailableOnline;

            await _unitOfWork.SaveChangesAsync();
        }

        public Task<Trip> GetAsync(Guid id, params Expression<Func<Trip, object>>[] properties)
        {
            return properties is null ? _tripRepo.GetAsync(id) :
                _tripRepo.GetAllIncluding(properties).FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<Trip> GetAll()
        {
            return _tripRepo.GetAll();
        }

        public Trip FirstOrDefault(Expression<Func<Trip, bool>> filter)
        {
            return _tripRepo.FirstOrDefault(filter);
        }
    }
}