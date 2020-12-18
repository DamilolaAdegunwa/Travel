using System;
using System.Threading.Tasks;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Timing;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using System.Linq;
using System.Collections.Generic;

namespace Travel.Business.Services
{
    public interface IJourneyManagementService
    {
        Task AddJourneyManagementFromManifest(Guid vehicleTripRegistrationId, JourneyType loaded);
        Task<List<JourneyDto>> GetIncomingJournies(int terminalId, DateTime? StartDate = null, DateTime? EndDate = null);
        Task<List<JourneyDto>> GetOutgoingJournies(int terminalId, DateTime? StartDate = null, DateTime? EndDate = null);
        Task<List<VehicleTripRegistrationDTO>> GetIncomingBlowJournies(string username);
        IQueryable<JourneyManagement> GetAll();
        Task ReceiveJourney(Guid journeyManagementId);
        Task ApproveJourney(Guid journeyManagementId);
    }

    public class JourneyManagementService : IJourneyManagementService
    {
        private readonly IVehicleTripRegistrationService _vehicleTripSvc;
        private readonly IRepository<JourneyManagement, Guid> _repo;
        private readonly IRepository<VehicleTripRegistration, Guid> _vtripRepo;
        private readonly IRepository<Route> _routeRepo;
        private readonly IRepository<Trip, Guid> _tripRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly ITerminalService _terminalSvc;




        private readonly IUnitOfWork _uow;

        public JourneyManagementService(
            IVehicleTripRegistrationService vehicleTripSvc, IRepository<VehicleTripRegistration, Guid> vtriprepo,
            IRepository<JourneyManagement, Guid> repo, IUnitOfWork uow, IRepository<Trip, Guid> tripRepo, IRepository<Terminal> terminalRepo,
             IRepository<Route> routeRepo, IServiceHelper serviceHelper, ITerminalService terminalSvc
             )
        {
            _vehicleTripSvc = vehicleTripSvc;
            _repo = repo;
            _uow = uow;
            _vtripRepo = vtriprepo;
            _tripRepo = tripRepo;
            _terminalRepo = terminalRepo;
            _routeRepo = routeRepo;
            _serviceHelper = serviceHelper;
            _terminalSvc = terminalSvc;
        }

        public async Task AddJourneyManagementFromManifest(Guid vehicleTripRegistrationId, JourneyType journeyType)
        {
            if (!await _repo.ExistAsync(v => v.VehicleTripRegistrationId == vehicleTripRegistrationId)) {
                var vehicleTrip = await _vehicleTripSvc.GetVehicleTripRegistrationDTO(vehicleTripRegistrationId);
                var journeyManagement = new JourneyManagement
                { 
                    VehicleTripRegistrationId = vehicleTripRegistrationId,
                    JourneyStatus = JourneyStatus.Pending,
                    JourneyType = JourneyType.Loaded,
                    JourneyDate = vehicleTrip?.DepartureDate ?? Clock.Now
                };

                _repo.Insert(journeyManagement);
            }
        }
        public async Task ApproveJourney(Guid journeyManagementId)
        {
            var journey = _repo.Get(journeyManagementId);

            journey.JourneyStatus = JourneyStatus.InTransit;
            journey.ApprovedBy = _serviceHelper.GetCurrentUserEmail() ?? "Anon";
            journey.LastModificationTime = DateTime.Now;

            await _repo.UpdateAsync(journey);
            await _uow.SaveChangesAsync();
        }
        public async Task ReceiveJourney(Guid journeyManagementId)
        {
            var journey = _repo.Get(journeyManagementId);

            journey.JourneyStatus = JourneyStatus.Received;
            journey.ReceivedBy = _serviceHelper.GetCurrentUserEmail() ?? "Anon";
            journey.LastModificationTime = DateTime.Now;
            await _repo.UpdateAsync(journey);
            await _uow.SaveChangesAsync();
        }
        public  Task<List<JourneyDto>> GetIncomingJournies(int terminalId, DateTime? StartDate = null, DateTime? EndDate = null)
        {
            StartDate = StartDate ?? DateTime.Now.Date;
            EndDate = EndDate ?? DateTime.Now;
            var journeys = from jney in _repo.GetAll()
                          join vrtip in _vtripRepo.GetAll()
                          on jney.VehicleTripRegistrationId equals vrtip.Id
                          join trip in _tripRepo.GetAll()
                          on vrtip.TripId equals trip.Id
                          join route in _routeRepo.GetAll()
                          on trip.RouteId equals route.Id
                          join destinationTerminal in _terminalRepo.GetAll()
                          on route.DestinationTerminalId equals destinationTerminal.Id

                          where (vrtip.DepartureDate >= StartDate && vrtip.DepartureDate <= EndDate) && (destinationTerminal.Id == terminalId)
                          && (jney.JourneyStatus == JourneyStatus.InTransit)
                          select new JourneyDto
                          {
                              ApprovedBy = jney.ApprovedBy ?? "anon",
                              AvailableOnline = trip.AvailableOnline,
                              DriverFee = route.DriverFee,
                              DepartureDate = vrtip.DepartureDate,
                              RouteName = route.Name ,
                              JourneyStatus = jney.JourneyStatus,
                              DepartureTerminalName = route.DepartureTerminal == null?  "NA"  : route.DepartureTerminal.Name,
                              DestinationTerminalName = destinationTerminal.Name,
                              JourneyType = jney.JourneyType,
                              DepartureTime = trip.DepartureTime,
                              DispatchFee = route.DispatchFee,
                              LoaderFee = route.LoaderFee,
                              ManifestPrinted = vrtip.ManifestPrinted,
                              AppovedBy = jney.ApprovedBy ?? "anon",
                              TripCode = trip.TripCode,
                              JourneyManagementId = jney.Id,
                              PhysicalBusRegistrationNumber = vrtip.PhysicalBusRegistrationNumber
                          };

            return Task.FromResult(journeys.ToList());
        }
        public  Task<List<JourneyDto>> GetOutgoingJournies(int terminalId, DateTime? StartDate = null, DateTime? EndDate = null)
        {
            StartDate = StartDate ?? DateTime.Now.Date;
            EndDate = EndDate ?? DateTime.Now;
            var journeys = from jney in _repo.GetAll()
                           join vrtip in _vtripRepo.GetAll()
                           on jney.VehicleTripRegistrationId equals vrtip.Id
                           join trip in _tripRepo.GetAll()
                           on vrtip.TripId equals trip.Id
                           join route in _routeRepo.GetAll()
                           on trip.RouteId equals route.Id
                           join departureTerminal in _terminalRepo.GetAll()
                           on route.DepartureTerminalId equals departureTerminal.Id

                           where (vrtip.DepartureDate >= StartDate && vrtip.DepartureDate <= EndDate) && (departureTerminal.Id == terminalId)
                           && (jney.JourneyStatus == JourneyStatus.Pending)
                           select new JourneyDto
                           {
                               ApprovedBy = jney.ApprovedBy ?? "anon",
                               AvailableOnline = trip.AvailableOnline,
                               DriverFee = route.DriverFee,
                               DepartureDate = vrtip.DepartureDate,
                               RouteName = route.Name,
                               JourneyStatus = jney.JourneyStatus,
                               DepartureTerminalName = route.DestinationTerminal == null ? "NA" : route.DestinationTerminal.Name,
                               DestinationTerminalName = departureTerminal.Name,
                               JourneyType = jney.JourneyType,
                               DepartureTime = trip.DepartureTime,
                               DispatchFee = route.DispatchFee,
                               LoaderFee = route.LoaderFee,
                               ManifestPrinted = vrtip.ManifestPrinted,
                               AppovedBy = jney.ApprovedBy ?? "anon",
                               TripCode = trip.TripCode,
                               JourneyManagementId = jney.Id,
                               PhysicalBusRegistrationNumber = vrtip.PhysicalBusRegistrationNumber


                           };
            return Task.FromResult(journeys.ToList());

        }
        public IQueryable<JourneyManagement> GetAll()
        {
            return _repo.GetAllIncluding(x => x.CaptainFee, y => y.Id, y => y.JourneyStatus, y=> y.JourneyType);
        }

        public Task<List<VehicleTripRegistrationDTO>> GetIncomingBlowJournies(string username)
        {
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now;
            var terminalId = _terminalSvc.GetLoginEmployeeTerminal(username).Result;


            var journeys = from vrtip in _vtripRepo.GetAll()
                           join trip in _tripRepo.GetAll()
                           on vrtip.TripId equals trip.Id
                           join route in _routeRepo.GetAll()
                           on trip.RouteId equals route.Id
                           join destinationTerminal in _terminalRepo.GetAll()
                           on route.DestinationTerminalId equals destinationTerminal.Id

                           where (vrtip.DepartureDate >= startDate && vrtip.DepartureDate <= endDate) && (destinationTerminal.Id == terminalId.Id)
                           && vrtip.IsBlownBus == true && vrtip.JourneyType == JourneyType.Blown
                           select new VehicleTripRegistrationDTO
                           {
                               Id = vrtip.Id,
                               DepartureDate = vrtip.DepartureDate,
                               DriverCode = vrtip.DriverCode,
                               PhysicalBusRegistrationNumber = vrtip.PhysicalBusRegistrationNumber,
                               JourneyType = vrtip.JourneyType,
                               RouteName = trip.Route.Name
                           };

            return Task.FromResult(journeys.ToList());
        }
    }
}