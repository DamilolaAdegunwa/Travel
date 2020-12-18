using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        [Route("AddComplaint")]
        public async Task<IServiceResponse<bool>> AddComplaint(ComplaintDTO fare)
        {
            return await HandleApiOperationAsync(async () => {
                await _feedbackService.AddComplaint(fare);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<ComplaintDTO>>> GetComplaint(int pageNumber = 1,
        int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var Complaints = await _feedbackService.GetComplaint(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<ComplaintDTO>>()
                {
                    Object = Complaints
                };
            });
        }


        [HttpGet]
        [Route("Get/{id}")]
        public async Task<ServiceResponse<ComplaintDTO>> GetReportById(int id)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var complain = await _feedbackService.GetComplaintById(id);

                return new ServiceResponse<ComplaintDTO>
                {
                    Object = complain
                };
            });
        }

        [HttpPost]
        [Route("PostSendSms")]
        public async Task<IServiceResponse<SmsDetailsDto>> PostSendSms(SmsDetailsDto smsDetails)
        {
            return await HandleApiOperationAsync(async () => {
               await _feedbackService.PostSendSms(smsDetails);
                return new ServiceResponse<SmsDetailsDto>();
            });

        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpdateComplaint(int id, ComplaintDTO complaintDTO)
        {
            return await HandleApiOperationAsync(async () => {
                await _feedbackService.UpdateComplaint(id, complaintDTO);
                return new ServiceResponse<bool>(true);
            });
        }

    }
}