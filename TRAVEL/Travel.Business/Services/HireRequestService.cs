using IPagedList;
using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Core.Timing;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IHireRequestService
    {
        Task CreateRequest(HireRequestDTO hireRequest);
        Task<IPagedList<HireRequestDTO>> GetRequests(int pageNumber, int pageSize, string query);
    }

    public class HireRequestService : IHireRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<HireRequest> _repo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IMailService _mailSvc;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AppConfig appConfig;

        public HireRequestService(
            IUnitOfWork unitOfWork,
            IRepository<HireRequest> repo,
            IServiceHelper serviceHelper,
            IHostingEnvironment hostingEnvironment,
            IMailService mailSvc,
            IOptions<AppConfig> _appConfig)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
            _serviceHelper = serviceHelper;
            _hostingEnvironment = hostingEnvironment;
            _mailSvc = mailSvc;
            appConfig = _appConfig.Value;
        }

        public async Task CreateRequest(HireRequestDTO hireRequest)
        {
            if (hireRequest is null) {
                throw new LMEGenericException($"Request is invalid.");
            }

            if (hireRequest.RequestDate < Clock.Now) {
                throw new LMEGenericException($"Request date cannot be in the past.");
            }

            if (hireRequest.DepartureDate < Clock.Now) {
                throw new LMEGenericException($"Departure date cannot be in the past.");
            }

            _repo.Insert(new HireRequest
            {
                Address = hireRequest.Address,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                Departure = hireRequest.Departure,
                DepartureDate = hireRequest.DepartureDate,
                Destination = hireRequest.Destination,
                Email = hireRequest.Email,
                FirstName = hireRequest.FirstName,
                Gender = hireRequest.Gender,
                LastName = hireRequest.LastName,
                MiddleName = hireRequest.MiddleName,
                NextOfKinName = hireRequest.NextOfKinName,
                NextOfKinPhoneNumber = hireRequest.NextOfKinPhoneNumber,
                NumberOfBuses = hireRequest.NumberOfBuses,
                PhoneNumber = hireRequest.PhoneNumber,
                RequestDate = hireRequest.RequestDate
            });

            await _unitOfWork.SaveChangesAsync();
            await SendRequestEmail(hireRequest);
        }

        public Task<IPagedList<HireRequestDTO>> GetRequests(int pageNumber, int pageSize, string query)
        {
            var requests =
               from request in _repo.GetAll()

               where (string.IsNullOrWhiteSpace(query)
               || (request.FirstName.Contains(query)
               || request.LastName.Contains(query)
               || request.Destination.Contains(query)
               || request.Departure.Contains(query)
               ))

               orderby request.DepartureDate

               select new HireRequestDTO
               {
                   Id = request.Id,
                   RequestDate = request.RequestDate,
                   FirstName = request.FirstName,
                   LastName = request.LastName,
                   MiddleName = request.MiddleName,
                   Gender = request.Gender,
                   Email = request.Email,
                   PhoneNumber = request.PhoneNumber,
                   Address = request.Address,
                   AdditionalRequest = request.AdditionalRequest,
                   Departure = request.Departure,
                   DepartureDate = request.DepartureDate,
                   Destination = request.Destination,
                   NextOfKinName = request.NextOfKinName,
                   NextOfKinPhoneNumber = request.NextOfKinPhoneNumber,
                   NumberOfBuses = request.NumberOfBuses,
               };

            return requests.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }


        //Todo: implement this...
        private async Task SendRequestEmail(HireRequestDTO request)
        {
            try {
                var replacement = new StringDictionary
                {
                    ["Departurelocation"] = request.Departure,
                    ["Arrivallocation"] = request.Destination,
                    ["Departuredate"] = request.DepartureDate.ToString(CoreConstants.DateFormat),
                    ["Firstname"] = request.FirstName,
                    ["Lastname"] = request.LastName,
                    ["Email"] = request.Email,
                    ["Phone"] = request.PhoneNumber,
                    ["BusNo"] = request.NumberOfBuses.ToString(),
                };

                var adminMail = new Mail(appConfig.AppEmail, "Libmot.com: New Hire Request", appConfig.HiredBookingEmail.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    BodyIsFile = true,
                    BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.AdminHireBookingEmail)
                };

                var customerMail = new Mail(appConfig.AppEmail, "Libmot.com:Booking Request confirmation", request.Email)
                {
                    BodyIsFile = true,
                    BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.CustomerHireBookingEmail)
                };

                await Task.WhenAll(_mailSvc.SendMailAsync(adminMail, replacement),
                      _mailSvc.SendMailAsync(customerMail, replacement));
            }
            catch (Exception) {

            }
        }
    }
}