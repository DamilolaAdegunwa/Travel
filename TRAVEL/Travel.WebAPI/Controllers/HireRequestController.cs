using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    public class HireRequestController: BaseController
    {

        private readonly IHireRequestService _hireRequestSvc;

        public HireRequestController(IHireRequestService hireRequestSvc)
        {
            _hireRequestSvc = hireRequestSvc;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> Add(HireRequestDTO request)
        {
            return await HandleApiOperationAsync(async () => {
                await _hireRequestSvc.CreateRequest(request);
                return new ServiceResponse<bool>(true);
            });
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{search}")]
        public async Task<IServiceResponse<IPagedList<HireRequestDTO>>> GetHireRequests(int pageNumber = 1, int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var requests = await _hireRequestSvc.GetRequests(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<HireRequestDTO>>
                {
                    Object = requests
                };
            });
        }
    }
}