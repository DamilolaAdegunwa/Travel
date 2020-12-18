using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{

    public interface IPickupPointService
    {
        Task<List<PickupPointDTO>> GetTripPickupPoints(Guid tripId);
    }


    public class PickupPointService: IPickupPointService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Trip, Guid> _tripRepo;
        private readonly IRepository<PickupPoint> _pickupPointRepo;

        public PickupPointService(IUnitOfWork unitOfWork,
            IRepository<Trip, Guid> tripRepo,
            IRepository<PickupPoint> pickupPointRepo)
        {
            _unitOfWork = unitOfWork;
            _tripRepo = tripRepo;
            _pickupPointRepo = pickupPointRepo;
        }

        public Task<List<PickupPointDTO>> GetTripPickupPoints(Guid tripId)
        {
            var pickuppoints =
                 from pickuppoint in _pickupPointRepo.GetAll()
                 join trip in _tripRepo.GetAll() on pickuppoint.TripId equals trip.Id

                 where pickuppoint.TripId == tripId

                 select new PickupPointDTO
                 {
                     Id = pickuppoint.Id,
                     Name = pickuppoint.Name,
                     Latitude = pickuppoint.Latitude,
                     Longitude = pickuppoint.Longitude,
                     Image = pickuppoint.Image,
                     TripId = trip.Id,
                     DepartureTime = trip.DepartureTime,
                     Time = pickuppoint.PickupTime
                 };

            return pickuppoints.AsNoTracking().ToListAsync();
        }
    }
}