using Travel.Business.Services;
using Travel.Core.DataTransferObjects;
using Travel.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    public class CustomerController : BaseController
    {
        readonly ICustomerService _customerSvc;
        readonly IUserService _userSvc;

        public CustomerController(ICustomerService customerSvc, IUserService userSvc)
        {
            _customerSvc = customerSvc;
            _userSvc = userSvc;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ServiceResponse<UserDTO>> CreateUserProfile(CustomerDTO profile)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _customerSvc.CreateAccount(profile);
                return new ServiceResponse<UserDTO>(result);
            });
        }

        [HttpPost]
        [Route("SendActivationCode")]
        public async Task<ServiceResponse<bool>> SendActivationCode(string userNameOrEmail)
        {
            return await HandleApiOperationAsync(async () => {
                await _customerSvc.SendActivationCode(userNameOrEmail);
                return new ServiceResponse<bool>(true);
            });
        }
    }
}