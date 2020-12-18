using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [ApiController]
    [Authorize]

    public class JourneyController : BaseController
    {
        private readonly IJourneyManagementService _journeyService;
        private readonly IEmployeeService _employeeService;
        private readonly IServiceHelper _serviceHelper;

        public JourneyController(IJourneyManagementService journeyService, IEmployeeService employeeService, IServiceHelper serviceHelper)
        {
            _journeyService = journeyService;
            _employeeService = employeeService;
            _serviceHelper = serviceHelper;
        }

        [HttpGet]
        [Route("approve/{JourneyManagementId}")]
        public async Task<IServiceResponse<bool>> ApproveJourney(Guid JourneyManagementId)
        {
            return await HandleApiOperationAsync(async () => {
                await _journeyService.ApproveJourney(JourneyManagementId);
                return new ServiceResponse<bool> { Object = true };
            });
        }
        [HttpGet]
        [Route("receive/{JourneyManagementId}")]
        public async Task<IServiceResponse<bool>> ReceiveJourney(Guid JourneyManagementId)
        {
            return await HandleApiOperationAsync(async () => {
                await _journeyService.ReceiveJourney(JourneyManagementId);
                return new ServiceResponse<bool> { Object = true };
            });
        }
        [HttpPost]
        [Route("journeysincoming")]

        public async Task<IServiceResponse<List<JourneyDto>>> JourneysIncoming(JourneyQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () => {

                var employee = _serviceHelper.GetCurrentUserEmail();

                queryDto.TerminalId = await _employeeService.GetAssignedTerminal(employee) ?? throw new Exception("Invalid user access");

                //queryDto.StartDate = new DateTime(2019, 1, 2);
                queryDto.StartDate = queryDto.StartDate ?? DateTime.Now.Date;
                queryDto.EndDate = queryDto.EndDate ?? DateTime.Now;

                var journeys = await _journeyService.GetIncomingJournies(queryDto.TerminalId.Value, queryDto.StartDate, queryDto.EndDate);

                return new ServiceResponse<List<JourneyDto>>
                {
                    Object = journeys
                };
            });
        }
        [HttpPost]
        [Route("journeysoutgoing")]
        public async Task<IServiceResponse<List<JourneyDto>>> JourneysExiting(JourneyQueryDto queryDto)
        {
            return await HandleApiOperationAsync(async () => {
                var employee = _serviceHelper.GetCurrentUserEmail();

                queryDto.TerminalId = await _employeeService.GetAssignedTerminal(employee) ?? throw new Exception("Invalid user access");
                //queryDto.StartDate =  new DateTime(2019, 1, 2);
                queryDto.StartDate = queryDto.StartDate ?? DateTime.Now.Date;
                queryDto.EndDate = queryDto.EndDate ?? DateTime.Now;

                var journeys = await _journeyService.GetOutgoingJournies(queryDto.TerminalId.Value, queryDto.StartDate, queryDto.EndDate);

                return new ServiceResponse<List<JourneyDto>>
                {
                    Object = journeys
                };
            });
        }

        [HttpGet]
        [Route("blowjourneys")]
        public async Task<IServiceResponse<List<VehicleTripRegistrationDTO>>> BlowJourneys()
        {
            return await HandleApiOperationAsync(async () => {

                var employee = CurrentUser.UserName;

                var journeys = await _journeyService.GetIncomingBlowJournies(employee);

                return new ServiceResponse<List<VehicleTripRegistrationDTO>>
                {
                    Object = journeys
                };
            });
        }

    }
}