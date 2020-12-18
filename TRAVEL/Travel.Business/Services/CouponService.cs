using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Domain.Entities.Enums;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface ICouponService
    {
        Task<CouponDTO> GetValidCouponByPhone(string couponCode, string phone);
    }

    public class CouponService : ICouponService
    {
        private readonly IRepository<Coupon, Guid> _repository;
        private readonly IRepository<SeatManagement, long> _seatMgtRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public CouponService(IRepository<Coupon, Guid> repository,
            IRepository<SeatManagement, long> seatMgtRepository,
            IUnitOfWork unitOfWork,
            IServiceHelper serviceHelper)
        {
            _repository = repository;
            _seatMgtRepository = seatMgtRepository;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public IQueryable<Coupon> GetAll()
        {
            return _repository.GetAll();
        }


        public async Task<CouponDTO> GetValidCouponByPhone(string couponCode, string phone)
        {
            var coupon = await GetCouponByCodeAsync(couponCode);

            if (coupon == null) {
                throw await _serviceHelper.GetExceptionAsync("Coupon does not exist");
            }
            coupon = await GetValidCouponByCodeAsync(couponCode);
            if (coupon == null) {
                throw await _serviceHelper.GetExceptionAsync("Coupon is invalid");
            }

            var couponexpiryDate = await GetCouponExpiryDate(couponCode);

            if (DateTime.Now > couponexpiryDate) {
                throw await _serviceHelper.GetExceptionAsync("Coupon has Expired");
            }


            var couponuseByGuest = await GetCouponUsedByPhoneAsync(couponCode, phone);
            if (couponuseByGuest != null) {
                throw await _serviceHelper.GetExceptionAsync("Coupon already used by Guest");
            }
            return coupon;
        }

        public Task<SeatManagementDTO> GetCouponUsedByPhoneAsync(string couponCode, string phone)
        {
            var coupons =
                 from seat in _seatMgtRepository.GetAll()
                 where seat.HasCoupon && seat.CouponCode == couponCode && seat.PhoneNumber == phone
                 select new SeatManagementDTO
                 {
                     Id = seat.Id,
                     BookingReferenceCode = seat.BookingReferenceCode,
                     DateCreated = seat.CreationTime,
                     BookingStatus = seat.BookingStatus
                 };

            return coupons.AsNoTracking().FirstOrDefaultAsync();
        }

        private async Task<DateTime> GetCouponExpiryDate(string couponCode)
        {
            DateTime Expiry = DateTime.Now;
            var coupon = await GetCouponByCodeAsync(couponCode);

            if (coupon == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.COUPON_NOT_EXIST);
            }

            if (coupon.DurationType == DurationType.Second) {
                Expiry = coupon.DateCreated.AddSeconds(coupon.Duration);
            }
            else if (coupon.DurationType == DurationType.Minute) {
                Expiry = coupon.DateCreated.AddMinutes(coupon.Duration);
            }
            else if (coupon.DurationType == DurationType.Hour) {
                Expiry = coupon.DateCreated.AddHours(coupon.Duration);
            }
            else if (coupon.DurationType == DurationType.Day) {
                Expiry = coupon.DateCreated.AddDays(coupon.Duration);
            }
            else if (coupon.DurationType == DurationType.Month) {
                Expiry = coupon.DateCreated.AddMonths(coupon.Duration);
            }
            else if (coupon.DurationType == DurationType.Year) {
                Expiry = coupon.DateCreated.AddYears(coupon.Duration);
            }

            return Expiry;
        }

        private Task<CouponDTO> GetValidCouponByCodeAsync(string couponCode)
        {
            var coupons =
                from coupon in GetAll()
                where coupon.CouponCode == couponCode && coupon.Validity == true
                select new CouponDTO
                {
                    Id = coupon.Id,
                    CouponType = coupon.CouponType,
                    CouponCode = coupon.CouponCode,
                    Validity = coupon.Validity,
                    CouponValue = coupon.CouponValue,
                    DateCreated = coupon.CreationTime,
                    Duration = coupon.Duration,
                    DurationType = coupon.DurationType
                };

            return coupons.AsNoTracking().FirstOrDefaultAsync();
        }

        private Task<CouponDTO> GetCouponByCodeAsync(string couponCode)
        {
            var coupons =
                 from coupon in GetAll()
                 where coupon.CouponCode == couponCode
                 select new CouponDTO
                 {
                     Id = coupon.Id,
                     CouponType = coupon.CouponType,
                     CouponCode = coupon.CouponCode,
                     Validity = coupon.Validity,
                     CouponValue = coupon.CouponValue,
                     DateCreated = coupon.CreationTime,
                     Duration = coupon.Duration,
                     DurationType = coupon.DurationType
                 };

            return coupons.AsNoTracking().FirstOrDefaultAsync();
        }
    }
}