using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface IDiscountService
    {
        Task<DiscountDTO> GetDiscountByBookingTypeAsync(BookingTypes type);
        Task<IPagedList<DiscountDTO>> GetDiscountsAsync(int page, int size, string q = null);
        Task<DiscountDTO> GetDiscountByIdAsync(Guid discountId);
        Task AddDiscount(DiscountDTO discount);
        Task UpdateDiscount(Guid discountid, DiscountDTO discount);
        Task DeleteDiscount(Guid discountId);
        Task ToggleActiveStatus(Guid discount);
    }

    public class DiscountService : IDiscountService
    {
        readonly IRepository<Discount, Guid> _discountRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        public DiscountService(IRepository<Discount, Guid> discountRepo, IUnitOfWork unitOfWork, IServiceHelper serviceHelper)
        {
            _discountRepo = discountRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public Task<DiscountDTO> GetDiscountByBookingTypeAsync(BookingTypes type)

        {
            var discounts =
                from discount in _discountRepo.GetAll()
                where discount.BookingType == type && discount.Active
                select new DiscountDTO
                {
                    Id = discount.Id,
                    BookingTypeName = discount.BookingType.ToString(),
                    BookingType = discount.BookingType,
                    AdultDiscount = discount.AdultDiscount,
                    MinorDiscount = discount.MinorDiscount,
                    MemberDiscount = discount.MemberDiscount,
                    ReturnDiscount = discount.ReturnDiscount,
                    PromoDiscount = discount.PromoDiscount,
                    AppDiscountIos = discount.AppDiscountIos,
                    AppDiscountAndroid = discount.AppDiscountAndroid,
                    AppDiscountWeb = discount.AppDiscountWeb,
                    AppReturnDiscountIos = discount.AppReturnDiscountIos,
                    AppReturnDiscountAndroid = discount.AppReturnDiscountAndroid,
                    AppReturnDiscountWeb = discount.AppReturnDiscountWeb,
                    Active = discount.Active,
                    DateCreated = discount.CreationTime,
                    DateModified = discount.LastModificationTime,
                    CustomerDiscount = discount.CustomerDiscount
                };

            return discounts.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task<IPagedList<DiscountDTO>> GetDiscountsAsync(int page, int size, string q = null)
        {
            var discounts =
            from discount in _discountRepo.GetAll()

            orderby discount.CreationTime descending

            select new DiscountDTO
            {
                Id = discount.Id,
                BookingType = discount.BookingType,
                BookingTypeName = discount.BookingType.ToString(),
                AdultDiscount = discount.AdultDiscount,
                MinorDiscount = discount.MinorDiscount,
                MemberDiscount = discount.MemberDiscount,
                ReturnDiscount = discount.ReturnDiscount,
                PromoDiscount = discount.PromoDiscount,
                AppDiscountIos = discount.AppDiscountIos,
                AppDiscountAndroid = discount.AppDiscountAndroid,
                AppDiscountWeb = discount.AppDiscountWeb,
                AppReturnDiscountIos = discount.AppReturnDiscountIos,
                AppReturnDiscountAndroid = discount.AppReturnDiscountAndroid,
                AppReturnDiscountWeb = discount.AppReturnDiscountWeb,
                Active = discount.Active,
                DateCreated = discount.CreationTime,
                DateModified = discount.LastModificationTime,
                CustomerDiscount = discount.CustomerDiscount
            };


            return discounts.AsNoTracking().ToPagedListAsync(page, size); ;
        }


        public Task<DiscountDTO> GetDiscountByIdAsync(Guid discountId)

        {
            var discounts =
                 from discount in _discountRepo.GetAll()
                 where discount.Id == discountId
                 select new DiscountDTO
                 {
                     Id = discount.Id,
                     BookingTypeName = discount.BookingType.ToString(),
                     BookingType = discount.BookingType,
                     AdultDiscount = discount.AdultDiscount,
                     MinorDiscount = discount.MinorDiscount,
                     MemberDiscount = discount.MemberDiscount,
                     ReturnDiscount = discount.ReturnDiscount,
                     PromoDiscount = discount.PromoDiscount,
                     AppDiscountIos = discount.AppDiscountIos,
                     AppDiscountAndroid = discount.AppDiscountAndroid,
                     AppDiscountWeb = discount.AppDiscountWeb,
                     AppReturnDiscountIos = discount.AppReturnDiscountIos,
                     AppReturnDiscountAndroid = discount.AppReturnDiscountAndroid,
                     AppReturnDiscountWeb = discount.AppReturnDiscountWeb,
                     CustomerDiscount = discount.CustomerDiscount,
                     Active = discount.Active,
                     DateCreated = discount.CreationTime,
                     DateModified = discount.LastModificationTime
                 };
            return discounts.AsNoTracking().FirstOrDefaultAsync();
        }


        public async Task AddDiscount(DiscountDTO discount)
        {
            if ((await GetDiscountByBookingTypeAsync(discount.BookingType)) != null)
            {
                throw new LMEGenericException($"Discount already exist!");

            }


            _discountRepo.Insert(new Discount
            {
                BookingType = discount.BookingType,
                AdultDiscount = discount.AdultDiscount,
                MinorDiscount = discount.MinorDiscount,
                MemberDiscount = discount.MemberDiscount,
                ReturnDiscount = discount.ReturnDiscount,
                PromoDiscount = discount.PromoDiscount,
                AppDiscountIos = discount.AppDiscountIos,
                AppDiscountAndroid = discount.AppDiscountAndroid,
                AppDiscountWeb = discount.AppDiscountWeb,
                AppReturnDiscountIos = discount.AppReturnDiscountIos,
                AppReturnDiscountAndroid = discount.AppReturnDiscountAndroid,
                AppReturnDiscountWeb = discount.AppReturnDiscountWeb,
                CustomerDiscount = discount.CustomerDiscount,
                Active=discount.Active
            });

            await _unitOfWork.SaveChangesAsync();
        }


        public async Task UpdateDiscount(Guid discountid, DiscountDTO discount)
        {
            var existingDiscount = await _discountRepo.GetAsync(discountid);

            if (discount == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DISCOUNT_NOT_EXIST);
            }



            existingDiscount.MinorDiscount = discount.MinorDiscount;

            existingDiscount.AdultDiscount = discount.AdultDiscount;

            existingDiscount.MemberDiscount = discount.MemberDiscount;
            existingDiscount.AppDiscountIos = discount.AppDiscountIos;
            existingDiscount.AppDiscountAndroid = discount.AppDiscountAndroid;
            existingDiscount.AppDiscountWeb = discount.AppDiscountWeb;
            existingDiscount.AppReturnDiscountIos = discount.AppReturnDiscountIos;
            existingDiscount.AppReturnDiscountAndroid = discount.AppReturnDiscountAndroid;
            existingDiscount.AppReturnDiscountWeb = discount.AppReturnDiscountWeb;
            existingDiscount.ReturnDiscount = discount.ReturnDiscount;
            existingDiscount.PromoDiscount = discount.PromoDiscount;
            existingDiscount.Active = discount.Active;
            existingDiscount.CustomerDiscount = discount.CustomerDiscount;

            existingDiscount.BookingType = discount.BookingType;

            await _unitOfWork.SaveChangesAsync();
        }


        public async Task DeleteDiscount(Guid discountId)
        {
            var discount = await _discountRepo.GetAsync(discountId);

            if (discount == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DISCOUNT_NOT_EXIST);
            }

            _discountRepo.Delete(discount);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ToggleActiveStatus(Guid discount)
        {
            var disc = await _discountRepo.GetAsync(discount);

            if (disc == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.DISCOUNT_NOT_EXIST);
            }
            disc.Active = !disc.Active;

            await _unitOfWork.SaveChangesAsync();
        }


    }
}