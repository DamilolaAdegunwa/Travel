using IdentityModel;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IUserService _userSvc;
        private readonly IRoleService _roleSvc;
        private readonly IEmployeeService _employeeService;

        public AccountController(IUserService userSvc, IRoleService roleSvc, IEmployeeService employeeService)
        {
            _userSvc = userSvc;
            _roleSvc = roleSvc;
            _employeeService = employeeService;
        }

        private async Task<List<Claim>> GetUserIdentityClaims(User user)
        {
            var userClaims = user.UserToClaims();

            var roles = await _userSvc.GetUserRoles(user);

            foreach (var item in roles) {

                userClaims.Add(new Claim(JwtClaimTypes.Role, item));

                var roleClaims = await _roleSvc.GetClaimsAsync(item);
                userClaims.AddRange(roleClaims);
            }
            var employee = await _employeeService.GetEmployeesByemailAsync(user.Email);
            if (employee != null)
            {
                userClaims.Add(new Claim("location", employee.TerminalId?.ToString()));
            }
            return userClaims;
        }

        [HttpGet]
        [Route("GetCurrentUserClaims")]
        public async Task<IServiceResponse<IEnumerable<Claim>>> GetCurrentUserClaims()
        {
            return await HandleApiOperationAsync(async () => {

                var response = new ServiceResponse<IEnumerable<Claim>>();

                var user = await _userSvc.FindByNameAsync(User?.FindFirst(JwtClaimTypes.Name)?.Value);

                var claims = await GetUserIdentityClaims(user);

                response.Object = claims;

                return response;
            });
        }

        [HttpGet]
        [Route("GetProfile")]
        public async Task<IServiceResponse<UserProfileDTO>> GetCurrentUserProfile()
        {
            return await HandleApiOperationAsync(async () => {

                var response = new ServiceResponse<UserProfileDTO>();

                var profile = await _userSvc.GetProfile(User.FindFirst(JwtClaimTypes.Name)?.Value);
                response.Object = profile;
                return response;
            });
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IServiceResponse<bool>> UpdatetUserProfile(UserProfileDTO model)
        {
            return await HandleApiOperationAsync(async () => {

                var result = await _userSvc.UpdateProfile(User.FindFirst(JwtClaimTypes.Name)?.Value, model);

                return new ServiceResponse<bool>(result);
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Activate")]
        public async Task<ServiceResponse<UserDTO>> Activate(string usernameOrEmail, string activationCode)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _userSvc.ActivateAccount(usernameOrEmail, activationCode);
                return new ServiceResponse<UserDTO>(result);
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword/{usernameOrEmail}")]
        public async Task<ServiceResponse<bool>> ForgotPassword(string usernameOrEmail)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _userSvc.ForgotPassword(usernameOrEmail);
                return new ServiceResponse<bool>(result);
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<ServiceResponse<bool>> ResetPassword(PassordResetDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _userSvc.ResetPassword(model);
                return new ServiceResponse<bool>(result);
            });
        }


        [HttpPost]
        [Route("ChangePassword")]
        public async Task<ServiceResponse<bool>> ChangePassword(ChangePassordDTO model)
        {
            return await HandleApiOperationAsync(async () => {
                var result = await _userSvc.ChangePassword(User.FindFirst(JwtClaimTypes.Name)?.Value, model);
                return new ServiceResponse<bool>(result);
            });
        }
    }
}