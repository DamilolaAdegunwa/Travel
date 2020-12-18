using IPagedList;
using Travel.Business.Services;
using Travel.Core.Domain.DataTransferObjects;
using Travel.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Travel.WebAPI.Controllers
{
    [Authorize]
    public class DiscountController : BaseController
    {
        private readonly IDiscountService _DiscountService;

        public DiscountController(IDiscountService DiscountService)
        {
            _DiscountService = DiscountService;
        }

        [HttpGet]
        [Route("Get")]
        [Route("Get/{pageNumber}/{pageSize}")]
        [Route("Get/{pageNumber}/{pageSize}/{query}")]
        public async Task<IServiceResponse<IPagedList<DiscountDTO>>> GetDiscounts(int pageNumber = 1,
            int pageSize = WebConstants.DefaultPageSize, string query = null)
        {
            return await HandleApiOperationAsync(async () => {
                var Discounts = await _DiscountService.GetDiscountsAsync(pageNumber, pageSize, query);

                return new ServiceResponse<IPagedList<DiscountDTO>>
                {
                    Object = Discounts
                };
            });
        }

        [HttpGet]
        [Route("Get/{id}")]
        public async Task<IServiceResponse<DiscountDTO>> GetDiscountById(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                var Discount = await _DiscountService.GetDiscountByIdAsync(id);

                return new ServiceResponse<DiscountDTO>
                {
                    Object = Discount
                };
            });
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IServiceResponse<bool>> AddDiscount(DiscountDTO Discount)
        {
            return await HandleApiOperationAsync(async () => {
                await _DiscountService.AddDiscount(Discount);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IServiceResponse<bool>> UpateDiscount(Guid id, DiscountDTO Discount)
        {
            return await HandleApiOperationAsync(async () => {
                await _DiscountService.UpdateDiscount(id, Discount);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IServiceResponse<bool>> DeleteDiscount(Guid id)
        {
            return await HandleApiOperationAsync(async () => {
                await _DiscountService.DeleteDiscount(id);

                return new ServiceResponse<bool>(true);
            });
        }

        [HttpPut]
        [Route("toggleactive/{discountId}")]
        public async Task<IServiceResponse<bool>> ToggleActiveStatus(Guid discountId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _DiscountService.ToggleActiveStatus(discountId);

                return new ServiceResponse<bool>();
            });
        }
    }
}