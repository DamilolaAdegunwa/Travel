using Travel.Core.Configuration;
using Travel.Core.DataTransferObjects;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Data.UnitOfWork;
using Travel.Core.Messaging.Email;
using Travel.Core.Messaging.Sms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface ICustomerService
    {
        Task<UserDTO> CreateAccount(CustomerDTO customerDTO);
        Task SendActivationCode(string usernameOrEmail);
        Task UpdateAccount(CustomerDTO customerDTO);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IUserService _userManagerSvc;
        private readonly IRoleService _roleManagerSvc;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _svcHelper;
        private readonly IReferralService _referralSvc;
        private readonly ISMSService _smsSvc;
        private readonly IMailService _emailSvc;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AppConfig appConfig;


        public CustomerService(IUnitOfWork unitOfWork, IRoleService roleManagerSvc,
            ISMSService smsSvc, IUserService userManagerSvc,
            IReferralService referralSvc, IServiceHelper svcHelper,
            IMailService emailSvc, IGuidGenerator guidGenerator,
            IHostingEnvironment hostingEnvironment, IOptions<AppConfig> _appConfig)
        {
            appConfig = _appConfig.Value;
            _unitOfWork = unitOfWork;
            _userManagerSvc = userManagerSvc;
            _svcHelper = svcHelper;
            _referralSvc = referralSvc;
            _roleManagerSvc = roleManagerSvc;
            _smsSvc = smsSvc;
            _emailSvc = emailSvc;
            _guidGenerator = guidGenerator;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<UserDTO> CreateAccount(CustomerDTO customerDTO)
        {
            if (customerDTO is null)
                throw new ArgumentNullException(nameof(customerDTO));

            var customer = await _userManagerSvc.FindFirstAsync(c => c.Email == customerDTO.Email
                                                    || c.PhoneNumber.Trim() == customerDTO.Phone);

            customer = new User
            {
                UserName = customerDTO.Phone,
                Title = customerDTO.Title,
                FirstName = customerDTO.FirstName,
                MiddleName = customerDTO.MiddleName,
                LastName = customerDTO.LastName,
                DateOfBirth = customerDTO.DateOfBirth,
                Email = customerDTO.Email,
                Address = customerDTO.Address,
                PhoneNumber = customerDTO.Phone,
                OptionalPhoneNumber = customerDTO.OptionalPhoneNumber,
                NextOfKinName = customerDTO.NextOfKinName,
                NextOfKinPhone = customerDTO.NextOfKinPhone,
                ReferralCode = CommonHelper.GenereateRandonAlphaNumeric(),
                Referrer = customerDTO.ReferralCode,
                UserType = UserType.Customer,
                AccountConfirmationCode = _guidGenerator.Create().ToString().Substring(0, 8)
            };

            var customerAcctResult = !string.IsNullOrWhiteSpace(customerDTO.Password)
                             ? await _userManagerSvc.CreateAsync(customer, customerDTO.Password)
                             : await _userManagerSvc.CreateAsync(customer);

            if (customerAcctResult.Succeeded) {
                // await _userManagerSvc.AddToRoleAsync(customer, CoreConstants.Roles.Customer);

                //Password customer created from front-end, backend create non-password
                if (!string.IsNullOrWhiteSpace(customer.PasswordHash)) {
                    await SendActivationMessage(customer);
                }
                //    if (!String.IsNullOrEmpty(customer.ReferralCode)) {
                //        _smsSvc.SendSMSNow($"Your referrer code is {customer.ReferralCode}", recipient: customer.PhoneNumber);
                //    }


                return new UserDTO
                {
                    PhoneNumber = customer.PhoneNumber,
                    Email = customer.Email,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    UserId = customer.Id,
                    IsActive = customer.EmailConfirmed,
                    AccountIsDeleted = customer.IsDeleted,
                    ReferralCode = customer.ReferralCode
                };
            }
            else {
                throw await _svcHelper.GetExceptionAsync(customerAcctResult.Errors.FirstOrDefault()?.Description);
            }
        }

        public async Task UpdateAccount(CustomerDTO customerDTO)
        {
            if (customerDTO is null)
                throw new ArgumentNullException(nameof(customerDTO));

            var customer = await _userManagerSvc.FindFirstAsync(c => c.Email == customerDTO.Email
                                                    || c.PhoneNumber.Trim() == customerDTO.Phone);

            if (customer == null)
            {
                throw await _svcHelper.GetExceptionAsync(ErrorConstants.CUSTOMER_NOT_EXIST);
            }

            customer.Title = customerDTO.Title;
            customer.FirstName = customerDTO.FirstName;
            customer.MiddleName = customerDTO.MiddleName;
            customer.LastName = customerDTO.LastName;
            customer.DateOfBirth = customerDTO.DateOfBirth;
        
            customer.Email = customerDTO.Email;
            customer.Address = customerDTO.Address;
            customer.PhoneNumber = customerDTO.Phone;
            customer.OptionalPhoneNumber = customerDTO.OptionalPhoneNumber;
            customer.NextOfKinName = customerDTO.NextOfKinName;
            customer.NextOfKinPhone = customerDTO.NextOfKinPhone;

        }

        private async Task SendActivationMessage(User user)
        {
            try {
                string message = $"Welcome to Libmot.com  Please activate your account with this code: {user.AccountConfirmationCode}";

                _smsSvc.SendSMSNow(message, recipient: user.PhoneNumber);

                var replacement = new StringDictionary
                {
                    ["FirstName"] = user.FirstName,
                    ["ActivationCode"] = user.AccountConfirmationCode,
                };

                var mail = new Mail(appConfig.AppEmail, "Libmot.com: New account activation code", user.Email)
                {
                    BodyIsFile = true,
                    BodyPath = Path.Combine(_hostingEnvironment.ContentRootPath, CoreConstants.Url.ActivationCodeEmail)
                };

                await _emailSvc.SendMailAsync(mail, replacement);
            }
            catch (Exception) {
            }
        }

        public async Task SendActivationCode(string usernameOrEmail)
        {
            if (string.IsNullOrEmpty(usernameOrEmail)) {
                throw await _svcHelper.GetExceptionAsync(ErrorConstants.USER_ACCOUNT_NOT_EXIST);
            }

            var user = await _userManagerSvc.FindByNameAsync(usernameOrEmail) ?? await _userManagerSvc.FindByEmailAsync(usernameOrEmail);

            if (user == null)
                throw await _svcHelper.GetExceptionAsync(ErrorConstants.USER_ACCOUNT_NOT_EXIST);

            if (user.IsDeleted)
                throw await _svcHelper.GetExceptionAsync(ErrorConstants.USER_ACCOUNT_NOT_EXIST);

            if (user.IsConfirmed())
                throw new LMEGenericException("Your account was activated earlier.");

            if (string.IsNullOrWhiteSpace(user.AccountConfirmationCode))
                throw new LMEGenericException("Activation code is missing. Please contact support.");

            await SendActivationMessage(user);
        }
    }
}