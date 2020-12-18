using Travel.Business.Services;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Utils.Extensions;
using Travel.WebAPI.Utils;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private string _databaseErrorMessage;

        public UserClaims CurrentUser
        {
            get
            {
                if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated) {
                    return new UserClaims(HttpContext.User);
                }

                return null;
            }
        }

        protected async Task<ServiceResponse<T>> HandleApiOperationAsync<T>(
       Func<Task<ServiceResponse<T>>> action, [CallerLineNumber] int lineNo = 0, [CallerMemberName] string method = "")
        {
            var _logger = LogManager.GetLogger(typeof(BaseController));

            _logger.Info($">>> ENTERS ({method}) >>> ");

            var serviceResponse = new ServiceResponse<T>
            {
                Code = HttpHelpers.GetStatusCodeValue(HttpStatusCode.OK),
                ShortDescription = "SUCCESS"
            };

            try {

                if (!ModelState.IsValid)
                    throw new LMEGenericException("There were errors in your input, please correct them and try again.",
                        HttpHelpers.GetStatusCodeValue(HttpStatusCode.BadRequest));

                var actionResponse = await action();

                serviceResponse.Object = actionResponse.Object;
                serviceResponse.ShortDescription = actionResponse.ShortDescription ?? serviceResponse.ShortDescription;
                serviceResponse.Code = actionResponse.Code ??serviceResponse.Code;

            }
            catch (LMEGenericException ex) {

                _logger.Warn($"L{lineNo} - {ex.ErrorCode}: {ex.Message}");

                serviceResponse.ShortDescription = ex.Message;
                serviceResponse.Code = ex.ErrorCode ?? HttpHelpers.GetStatusCodeValue(HttpStatusCode.BadRequest);

                if (!ModelState.IsValid) {
                    serviceResponse.ValidationErrors = ModelState.ToDictionary(
                        m => {
                            var tokens = m.Key.Split('.');
                            return tokens.Length > 0 ? tokens[tokens.Length - 1] : tokens[0];
                        },
                        m => m.Value.Errors.Select(e => e.Exception?.Message ?? e.ErrorMessage)
                    );
                }
            }
            catch (DbUpdateException duex)
            when (duex.IsDatabaseFkDeleteException(out _databaseErrorMessage)) {

                _logger.Warn($"L{lineNo} - DFK001: {_databaseErrorMessage}");

                serviceResponse.ShortDescription = "You cannot delete this record because it's currently in use.";
                serviceResponse.Code = "DFK001";
            }
            catch (DbUpdateException duex)
            when (duex.IsUpdateConcurrencyException(out _databaseErrorMessage)) {
                _logger.Warn($"L{lineNo} - DCU001: {_databaseErrorMessage}");
                serviceResponse.ShortDescription = "An error occured while updating your record";
                serviceResponse.Code = "DCU001";
            }

            catch (Exception ex) {

                _logger.Warn($"L{lineNo} - DBV001: {ex.Message}");

                serviceResponse.ShortDescription = ex.Message;
                serviceResponse.Code = HttpHelpers.GetStatusCodeValue(HttpStatusCode.InternalServerError);

                _logger.Error(ex.Message, ex);
            }

            _logger.Info($"<<< EXITS ({method}) <<< ");

            return serviceResponse;
        }

        /// <summary>
        /// Read ModelError into string collection
        /// </summary>
        /// <returns></returns>
        private List<string> ListModelErrors
        {
            get
            {
                return ModelState.Values
                  .SelectMany(x => x.Errors
                    .Select(ie => ie.ErrorMessage))
                    .ToList();
            }
        }
    }
}