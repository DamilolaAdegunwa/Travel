using Travel.Core.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IReferralService
    {
        Task<string> AddReferralCode(ReferralDTO referralDTO);
        Task<bool> ExistAsync(Expression<Func<Referral, bool>> predicate);
        Task<ReferralDTO> GetReferralByEmail(string email);
    }

    public class ReferralService : IReferralService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Referral, long> _repository;

        public ReferralService(IUnitOfWork unitOfWork, IRepository<Referral, long> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<string> AddReferralCode(ReferralDTO referralDTO)
        {
            referralDTO.ReferralCode = CommonHelper.GenereateRandonAlphaNumeric();

            bool exists;

            try {
                //if person already has a code
                if (String.IsNullOrEmpty(referralDTO.Email) || referralDTO.Email == "noemail@Travel.com") {
                    exists = await _repository.ExistAsync(x => x.PhoneNumber == referralDTO.PhoneNumber);
                }
                else
                    exists = await _repository.ExistAsync(x => x.ReferralCode == referralDTO.ReferralCode
                                  || x.Email == referralDTO.Email || x.PhoneNumber == referralDTO.PhoneNumber);

                //if referral code already exists
                if (await _repository.ExistAsync(x => x.ReferralCode == referralDTO.ReferralCode)) {
                    referralDTO.ReferralCode = CommonHelper.GenereateRandonAlphaNumeric();
                }
            }
            catch (Exception) {
                exists = true;
            }

            if (!exists) {

                var entity = new Referral
                {
                    Email = referralDTO.Email,
                    PhoneNumber = referralDTO.PhoneNumber,
                    ReferralCode = referralDTO.ReferralCode,
                    UserType = referralDTO.UserType
                };

                _repository.Insert(entity);

                _unitOfWork.SaveChanges();

                return referralDTO.ReferralCode;
            }
            else
                return null;
        }

        public Task<bool> ExistAsync(Expression<Func<Referral, bool>> predicate)
        {
            return _repository.ExistAsync(predicate);
        }

        public async Task<ReferralDTO> GetReferralByEmail(string emailOrPhone)
        {
            var referral = await _repository.FirstOrDefaultAsync(x => x.Email == emailOrPhone || x.PhoneNumber == emailOrPhone);

            return new ReferralDTO
            {
                Email = referral.Email,
                PhoneNumber = referral.PhoneNumber,
                ReferralCode = referral.ReferralCode,
                RefferalId = referral.Id,
                UserType = referral.UserType
            };
        }
    }
}