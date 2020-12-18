using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Sms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Travel.Core.Messaging.Email;
using Travel.Core.Utils;
using Travel.Core.Configuration;
using Microsoft.Extensions.Options;
namespace Travel.Business.Services
{
    public interface IMtuReports
    {
        Task AddReport(MtuReportModelDTO report);
        Task<IPagedList<MtuReportModelDTO>> GetAllReport(DateModel date, int pageNumber, int pageSize, string query);
        Task<MtuReportModelDTO> GetReportById(int id);
        Task UpdateReport(int id, MtuReportModelDTO vehicle);
        Task DeleteReport(int id);
    }

    public class MtuReportService : IMtuReports
    {
        private readonly IRepository<Vehicle> _vehicleRepo;       
        private readonly IDriverService _driversvc;
        private readonly IRepository<VehiclePart> _vehiclepartRepo;
        private readonly IRepository<VehicleModel> _vehicleModelRepo;
        private readonly IRepository<Terminal> _terminalRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        private readonly IRepository<State> _stateRepo;
        private readonly IFranchizeService _franchizeService;
        private readonly ISMSService _smsSvc;
        private readonly IMailService _mailSvc;
        private readonly IRepository<MtuReportModel> _mtuRepo;
        private readonly IRepository<MtuPhoto> _mtuPhotoRepo;
        private readonly AppConfig appConfig;
        private readonly IUserService _userSvc;
        private readonly IEmployeeService _employeeSvc;


        public MtuReportService(IRepository<Vehicle> vehicleRepo, IDriverService driversvc, IFranchizeService franchizeService, IOptions<AppConfig> _appConfig,
            IRepository<VehiclePart> vehiclepartRepo, IRepository<VehicleModel> vehicleModelRepo,
            IRepository<Terminal> terminalRepo, IRepository<Employee> employeeRepo,
            IUnitOfWork unitOfWork, IServiceHelper serviceHelper, IRepository<MtuPhoto> mtuPhotoRepo, IEmployeeService employeeSvc, IUserService userSvc, IRepository<State> stateRepo, ISMSService smsSvc, IRepository<MtuReportModel> mtuRepo, IMailService mailSvc)
        {
            _vehicleRepo = vehicleRepo;         
            _driversvc = driversvc;
            _vehiclepartRepo = vehiclepartRepo;
            _vehicleModelRepo = vehicleModelRepo;
            _terminalRepo = terminalRepo;
            _employeeRepo = employeeRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
            _stateRepo = stateRepo;
            _franchizeService = franchizeService;
            _smsSvc = smsSvc;
            _mtuRepo = mtuRepo;
            _mailSvc = mailSvc;
            appConfig = _appConfig.Value;
            _employeeSvc = employeeSvc;
            _userSvc = userSvc;
            _mtuPhotoRepo = mtuPhotoRepo;
        }
        public async Task AddReport(MtuReportModelDTO report)
        {
            //var email = await _userSvc.FindByNameAsync(_serviceHelper.GetCurrentUserEmail());

            var employee = await _employeeSvc.GetEmployeesByemailAsync(report.Email);

            report.FullName = employee.FullName;

            var mtuReport = new MtuReportModel
            {
                Id = report.Id,
                Notes = report.Notes,
                FullName = report.FullName,
                Email = report.Email,
                VehicleId = report.VehicleId,
                RegistrationNumber = report.RegistrationNumber,
                DriverId = report.DriverCode,
                Status =  report.Status,  
                Picture = report.Picture
            };

            _mtuRepo.Insert(mtuReport);

            await _unitOfWork.SaveChangesAsync();
            var getMtu = from repor in _mtuRepo.GetAll()
                         where repor.CreationTime == mtuReport.CreationTime

                         select new MtuReportModel
                         {
                             Id = repor.Id
                         };

            foreach (var pics in report.MtuPhotos)
            {
                var mtuPhoto = new MtuPhoto
                {
                    FileName = pics.FileName,
                    MtuReportModelId = getMtu.Select(c =>c.Id).FirstOrDefault()
                };

                _mtuPhotoRepo.Insert(mtuPhoto);
            }

            await _unitOfWork.SaveChangesAsync();
            string smsMessageb = $"An MTU Report was created for Vehicle No: { report.RegistrationNumber}, with Driver: { report.DriverCode} and the status of the Vehicle is: {report.Status.ToString()}. It was reported by {report.FullName}." ;
            await SendSMS(appConfig.MtuSms, smsMessageb);
        }
        public async Task<MtuReportModelDTO> GetReportById(int id)
        {
            var report = await _mtuRepo.GetAsync(id);

            if (report == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.NULL_ENTRY_REJECTED);
            }

            var pics = from pic in _mtuPhotoRepo.GetAll()
                       where pic.MtuReportModelId == id
                       select new MtuPhoto
                       {
                           FileName = pic.FileName,
                           Id = pic.Id
                       };

            return new MtuReportModelDTO
            {
                Id = report.Id,
                Notes = report.Notes,
                Email = report.Email,
                FullName = report.FullName,
                Date = report.CreationTime,
                VehicleId = report.VehicleId,
                RegistrationNumber = report.RegistrationNumber,
                DriverCode = report.DriverId,
                Status = report.Status,
                VehicleStatus = report.Status.ToString(),
                Picture = report.Picture,
                MtuPhotos = pics.ToListAsync().Result

            };
        }

        public async Task DeleteReport(int id)
        {
            var report = await _mtuRepo.GetAsync(id);

            if (report == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.NULL_ENTRY_REJECTED);
            }
            _mtuRepo.Delete(report);

            await _unitOfWork.SaveChangesAsync();
        }

        public  Task<IPagedList<MtuReportModelDTO>> GetAllReport(DateModel date, int pageNumber, int pageSize, string query)
        {
            if (date.Keyword == "Select an Employee")
            {
                var reports2 = from report in _mtuRepo.GetAll()
                               where report.CreationTime >= date.StartDate && report.CreationTime <= date.EndDate
                               where string.IsNullOrWhiteSpace(query) || report.FullName.Contains(query) || report.Email.Contains(query) || report.Notes.Contains(query) || report.Status.Equals(query) || report.VehicleId.Contains(query) || report.RegistrationNumber.Contains(query) || report.DriverId.Contains(query)
                               orderby report.CreationTime descending

                               select new MtuReportModelDTO
                               {
                                   Id = report.Id,
                                   Notes = report.Notes,
                                   Email = report.Email,
                                   FullName = report.FullName,
                                   Date = report.CreationTime,
                                   VehicleId = report.VehicleId,
                                   RegistrationNumber = report.RegistrationNumber,
                                   DriverCode = report.DriverId,
                                   Status = report.Status,
                                   VehicleStatus = report.Status.ToString(),
                                   Picture = report.Picture
                               };

                return reports2.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
            }
            else
            {
                var reports = from report in _mtuRepo.GetAll()
                              where report.CreationTime >= date.StartDate && report.CreationTime <= date.EndDate && report.FullName.Contains(date.Keyword)
                              where string.IsNullOrWhiteSpace(query) || report.FullName.Contains(query) || report.Email.Contains(query) || report.Notes.Contains(query) || report.Status.Equals(query) || report.VehicleId.Contains(query) || report.RegistrationNumber.Contains(query) || report.DriverId.Contains(query)
                              orderby report.CreationTime descending

                              select new MtuReportModelDTO
                              {
                                  Id = report.Id,
                                  Notes = report.Notes,
                                  Email = report.Email,
                                  FullName = report.FullName,
                                  Date = report.CreationTime,
                                  VehicleId = report.VehicleId,
                                  RegistrationNumber = report.RegistrationNumber,
                                  DriverCode = report.DriverId,
                                  Status = report.Status,
                                  VehicleStatus = report.Status.ToString(),
                                  Picture = report.Picture
                              };

                return reports.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
            }
        }
        public Task<List<MtuReportModelDTO>> GetAllReportSearch(DateModel date, string query)
        {
            var reports = from report in _mtuRepo.GetAll()
                          where report.CreationTime >= date.StartDate && report.CreationTime <= date.EndDate &&  report.FullName == query
                          orderby report.CreationTime descending

                          select new MtuReportModelDTO
                          {
                              Id = report.Id,
                              Notes = report.Notes,
                              Email = report.Email,
                              FullName = report.FullName,
                              Date = report.CreationTime,
                              VehicleId = report.VehicleId,
                              RegistrationNumber = report.RegistrationNumber,
                              DriverCode = report.DriverId,
                              Status = report.Status,
                              VehicleStatus = report.Status.ToString(),
                              Picture = report.Picture
                          };

            return reports.AsNoTracking().ToListAsync();
        }
        public Task UpdateReport(int id, MtuReportModelDTO vehicle)
        {
            throw new NotImplementedException();
        }

        private async Task SendSMS(string phoneNumber, string smsMessageb)
        {
            string smsMessage = smsMessageb;
            try
            {
                await Task.Factory.StartNew(() => _smsSvc.SendSMSNow(smsMessage, recipient: phoneNumber.ToNigeriaMobile()));
            }

            catch (Exception)
            {
            }
        }
    }
}

//string email = "john.dhara@gmail.com";
////string countMessage = "Dear Staff, <br><br>This is to notify you for testing<strong>";
////var mails = new Mail(appConfig.AppEmail, "Testing MTU Notification!", email)
////{
////    Body = countMessage
////};

////await _mailSvc.SendMailAsync(mail);

////string customEmail = "noemail@libmot.com";
////var mainbooker = await GetBookingByRefcodeAsync(booking?.MainBookerReferenceCode);
//string tester = "09084985923";
//string smsMessage = "";
//smsMessage =
//    $"Testing MTU Reports.";
//try
//{
//    _smsSvc.SendSMSNow(smsMessage, recipient: tester.ToNigeriaMobile());

//        var mail = new Mail(appConfig.AppEmail, "Booking Suspension Notification!", email)
//        {
//            Body = "Testing MTU Notification!"
//        };

//        _mailSvc.SendMail(mail);
//}
//catch
//{
//}