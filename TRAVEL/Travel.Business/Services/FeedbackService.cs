using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Data.Repository;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Travel.Core.Messaging.Sms;
using Travel.Core.Utils;
using Travel.Core.Exceptions;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Email;
using Microsoft.Extensions.Options;
using Travel.Core.Configuration;

namespace Travel.Business.Services
{
    public interface IFeedbackService
    {
        Task AddComplaint(ComplaintDTO complaintDTO);
        Task<IPagedList<ComplaintDTO>> GetComplaint(int page, int size, string query = null);
        Task PostSendSms(SmsDetailsDto smsDetails);
        Task<ComplaintDTO> GetComplaintById(int id);
        Task UpdateComplaint(int id, ComplaintDTO complaintDTO);
    }


    public class FeedbackService : IFeedbackService
    {
      private readonly IRepository<Complaint> _complaintRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly ISMSService _smsSvc;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mailSvc;
        private readonly AppConfig appConfig;

        public FeedbackService(IRepository<Complaint> complaintRepo, 
            IServiceHelper serviceHelper, IMailService mailSvc, ISMSService smsSvc, IOptions<AppConfig> _appConfig, IUnitOfWork unitOfWork)
        {
            _complaintRepo = complaintRepo;
            _serviceHelper = serviceHelper;
            _smsSvc = smsSvc;
            _unitOfWork = unitOfWork;
            _mailSvc = mailSvc;
            appConfig = _appConfig.Value;
        }

        public Task AddComplaint(ComplaintDTO complaintDTO)
        {
            DateTime dateTime = DateTime.Now;
            var complaint = new Complaint
            {
                BookingReference = complaintDTO.BookingReference,
                ComplaintType = complaintDTO.ComplaintType,
                Email = complaintDTO.Email,
                FullName = complaintDTO.FullName,
                Message = complaintDTO.Message,
                PriorityLevel = complaintDTO.PriorityLevel,
                CreationTime = dateTime
            };
            var newlyInserted =_complaintRepo.InsertAndGetId(complaint);
            if (newlyInserted <= 0)
            {
                throw new Exception("Could not insert new record");
            }

            return Task.FromResult(newlyInserted);
        }

        public Task<IPagedList<ComplaintDTO>> GetComplaint(int page, int size, string query = null)
        {
            //DateTime dateTime = DateTime.Now;
            var complaints = from complaint in _complaintRepo.GetAll()
                                 where string.IsNullOrWhiteSpace(query) || complaint.FullName .Contains(query)
                                 orderby complaint.CreationTime descending

                             select new ComplaintDTO
                             {
                                 Id = complaint.Id,
                                 BookingReference = complaint.BookingReference,
                                 ComplaintType = complaint.ComplaintType,
                                 Email = complaint.Email,
                                 FullName = complaint.FullName,
                                 Message = complaint.Message,
                                 PriorityLevel = complaint.PriorityLevel,
                                 Priority = complaint.PriorityLevel.ToString(),
                                 CreationTime = complaint.CreationTime,
                                 Responded = complaint.Responded,
                                 RepliedMessage = complaint.RepliedMessage
                                 //Id       = state.Id,
                             };
            return complaints.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task<ComplaintDTO> GetComplaintById(int id)
        {
            var complaint = await _complaintRepo.GetAsync(id);

            if (complaint == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.NULL_ENTRY_REJECTED);
            }

            return new ComplaintDTO
            {
                Id = complaint.Id,
                BookingReference = complaint.BookingReference,
                Complaints = complaint.ComplaintType.ToString(),
                Message = complaint.Message,
                FullName = complaint.FullName,
                Email = complaint.Email,
                PriorityLevel = complaint.PriorityLevel,
                Priority = complaint.PriorityLevel.ToString(),
                CreationTime = complaint.CreationTime,
                ComplaintType = complaint.ComplaintType,
                Responded = complaint.Responded,
                RepliedMessage = complaint.RepliedMessage
            };
        }
        public async Task UpdateComplaint(int id, ComplaintDTO complaintDTO)
        {
            var complaint = await _complaintRepo.GetAsync(id);

            if (complaint == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.NULL_ENTRY_REJECTED);
            }

            var complaintss = _complaintRepo.GetAll().Where(a => a.Id == id);

            complaintss.FirstOrDefault().RepliedMessage = complaintDTO.RepliedMessage;
            complaintss.FirstOrDefault().Responded = true;

            await _unitOfWork.SaveChangesAsync();

            try
            {
                string countMessage = "Dear Valued Customer <strong>" + complaint.FullName + " </strong>, <br><br>This is to notify you that we received your email"+
                "<br> <br> Aplogies for the delay.<br> <br>" + complaintDTO.RepliedMessage + " <br> <br> Regards";

                var mail = new Mail(appConfig.AppEmail, "Complaint ", complaint.Email)
                {
                    Body = countMessage
                };

                await _mailSvc.SendMailAsync(mail);
            }
            catch
            {

            }

        }
        public async Task PostSendSms(SmsDetailsDto smsDetails)
        {
          
            try
            {
                await Task.Factory.StartNew(() => _smsSvc.SendSMSNow(smsDetails.Message, recipient: smsDetails.PhoneNumber.ToNigeriaMobile()));
            }
            catch (Exception)
            {
            }
        }




    }
}
