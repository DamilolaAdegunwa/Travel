using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Travel.Business.Services
{
    public interface ITripAvailabilityService {

        List<Guid> GetAvailableTripsForRoute(int routeId, int dayOfWeek);
    }

   public class TripAvailabilityService: ITripAvailabilityService
    {

        private readonly IRepository<TripAvailability,Guid> _tripavailabilityRepo;
        private readonly IRepository<TripSetting, Guid> _tripSettingRepo;

        public TripAvailabilityService(
            IRepository<TripAvailability, Guid> tripavailabilityRepo,
            IRepository<TripSetting, Guid> tripSettingRepo)
        {
            _tripavailabilityRepo = tripavailabilityRepo;
            _tripSettingRepo = tripSettingRepo;
        }

        public List<Guid> GetAvailableTripsForRoute(int routeId, int dayOfWeek)
        {

            var weekDay = (WeekDays) dayOfWeek;

            var availableTrips = from trip in _tripavailabilityRepo.GetAll().AsNoTracking()
                                 join setting in _tripSettingRepo.GetAll().AsNoTracking() on trip.TripSettingId equals setting.TripSettingId

                                 where setting.RouteId == routeId
                                     && setting.WeekDays == weekDay
                                     && setting.IsDeleted == false
                                     && trip.IsDeleted == false

                                 select trip.TripId;

            return availableTrips.ToList();
        }
    }
}
