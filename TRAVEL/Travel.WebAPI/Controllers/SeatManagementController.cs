using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Core.Messaging.Email;
using Travel.WebAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class SeatManagementController : BaseController
    {
        private readonly IUserService _userManagerSvc;
        private readonly IMailService _mailSvc;
        private readonly ISeatManagementService _service;
        private readonly IServiceHelper _serviceHelper;

        public SeatManagementController(IUserService userManagerSvc,
            IMailService mailSvc,
            ISeatManagementService seatmgtSvc,
            IServiceHelper serviceHelper)
        {
            _userManagerSvc = userManagerSvc;
            _mailSvc = mailSvc;
            _service = seatmgtSvc;
            _serviceHelper = serviceHelper;
        }


        [HttpPut]
        [Route("removeseat/{seatManagementId}")]
        public async Task<IServiceResponse<bool>> RemoveSeatFromManifest(int seatManagementId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.RemoveSeatFromManifest(seatManagementId);

                return new ServiceResponse<bool>();
            });
        }

        [HttpPut]
        [Route("rescheduleseat/{seatManagementId}")]
        public async Task<IServiceResponse<bool>> RescheduleSeatFromManifest(int seatManagementId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.RescheduleSeatFromManifest(seatManagementId);

                return new ServiceResponse<bool>();
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getseat/{seatManagementId}/{isPrinted}")]
        public async Task<IServiceResponse<SeatManagementDTO>> GetSeatManagementByIdUpdatePrint(int seatManagementId, bool isPrinted)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var seatManagement = await _service.GetSeatByIdUpdatePrint(seatManagementId, isPrinted);

                return new ServiceResponse<SeatManagementDTO>
                {
                    Object = seatManagement
                };
            });
        }

    }
}