using IPagedList;
using Travel.Core;
using Travel.Core.Configuration;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel.Core.Domain.Entities;

namespace Travel.Business.Services
{
    public interface IFranchizeService
    {
   
        Task<IPagedList<FranchiseUserDTO>> GetFranchiseUsers(int pageNumber, int pageSize, string query);
        Task AddFranchiseUser(FranchiseUserDTO franchiseUser);
        Task<FranchiseUserDTO> GetFranchiseUser(int id);
        Task UpdateFranchiseUser(int id, FranchiseUserDTO model);
        Task RemoveFranchise(int id);
        IQueryable<Franchize> GetAll();

    }

    public class FranchizeService : IFranchizeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;
        private readonly IGuidGenerator _guidGenerator;
        private readonly AppConfig appConfig;
        private readonly IRepository<Franchize> _repo;


        public FranchizeService(IUnitOfWork unitOfWork, IRepository<Franchize> franchiseRepo,
            IServiceHelper serviceHelper,
            IGuidGenerator guidGenerator,
            IOptions<AppConfig> _appConfig)
        {
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;     
            appConfig = _appConfig.Value;          
            _guidGenerator = guidGenerator;
            _repo = franchiseRepo;

        }

        public IQueryable<Franchize> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<IPagedList<FranchiseUserDTO>> GetFranchiseUsers(int pageNumber, int pageSize, string query)
        {
            var franchiseUser = from Franchize in _repo.GetAll()
                                where string.IsNullOrWhiteSpace(query) ||
                                (Franchize.FirstName.Contains(query) ||
                                Franchize.LastName.Contains(query) ||
                                Franchize.ReferralCode.Contains(query))
                                select new FranchiseUserDTO
                                {
                                    Id = Franchize.Id,
                                    FirstName = Franchize.FirstName,
                                    MiddleName = Franchize.MiddleName,
                                    LastName = Franchize.LastName,
                                    Address = Franchize.Address,
                                    IsFirstTimeLogin = Franchize.IsFirstTimeLogin,
                                    OptionalPhoneNumber = Franchize.OptionalPhoneNumber,
                                    Image = Franchize.Image,
                                    RefreshToken = Franchize.RefreshToken,
                                    Title = Franchize.Title,
                                    DeviceToken = Franchize.DeviceToken,
                                    Referrer = Franchize.Referrer,
                                    ReferralCode = Franchize.ReferralCode,
                                    NextOfKinName = Franchize.NextOfKinName,
                                    NextOfKinPhone = Franchize.NextOfKinPhone,
                                    DateOfBirth = Franchize.DateOfBirth,
                                    AccountConfirmationCode = Franchize.AccountConfirmationCode,
                                    Photo = Franchize.Photo,
                                    OTP = Franchize.OTP,
                                    PhoneNumber = Franchize.PhoneNumber,
                                    Email = Franchize.Email
                                };
            return franchiseUser.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }


        public async Task AddFranchiseUser(FranchiseUserDTO franchiseUser)
        {
            if (await IsdefinedFranchise(franchiseUser.Id))
            {
                throw new LMEGenericException($"Franchise already exist!");
            }

            var franchises = new Franchize
            {
                Id = franchiseUser.Id,
                FirstName = franchiseUser.FirstName,
                MiddleName = franchiseUser.MiddleName,
                LastName = franchiseUser.LastName,
                Address = franchiseUser.Address,
                IsFirstTimeLogin = franchiseUser.IsFirstTimeLogin,
                OptionalPhoneNumber = franchiseUser.OptionalPhoneNumber,
                Image = franchiseUser.Image,
                RefreshToken = franchiseUser.RefreshToken,
                Title = franchiseUser.Title,
                DeviceToken = franchiseUser.DeviceToken,
                Referrer = franchiseUser.Referrer,
                ReferralCode = franchiseUser.ReferralCode,
                NextOfKinName = franchiseUser.NextOfKinName,
                NextOfKinPhone = franchiseUser.NextOfKinPhone,
                DateOfBirth = franchiseUser.DateOfBirth,
                AccountConfirmationCode = franchiseUser.AccountConfirmationCode,
                Photo = franchiseUser.Photo,
                OTP = franchiseUser.OTP,
                PhoneNumber = franchiseUser.PhoneNumber,
                Email = franchiseUser.Email
            };

            _repo.Insert(franchises);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> IsdefinedFranchise(int id)
        {
            return await _repo.ExistAsync(f => f.Id == id);
        }

        public async Task<FranchiseUserDTO> GetFranchiseUser(int id)
        {
            var franchise = await _repo.GetAsync(id);
            if (franchise == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FRANCHISE_NOT_EXIST);
            }

            return new FranchiseUserDTO
            {
                 Id = franchise.Id,
                FirstName = franchise.FirstName,
                MiddleName = franchise.MiddleName,
                LastName = franchise.LastName,
                Address = franchise.Address,
                IsFirstTimeLogin = franchise.IsFirstTimeLogin,
                OptionalPhoneNumber = franchise.OptionalPhoneNumber,
                Image = franchise.Image,
                RefreshToken = franchise.RefreshToken,
                Title = franchise.Title,
                DeviceToken = franchise.DeviceToken,
                Referrer = franchise.Referrer,
                ReferralCode = franchise.ReferralCode,
                NextOfKinName = franchise.NextOfKinName,
                NextOfKinPhone = franchise.NextOfKinPhone,
                DateOfBirth = franchise.DateOfBirth,
                AccountConfirmationCode = franchise.AccountConfirmationCode,
                Photo = franchise.Photo,
                OTP = franchise.OTP,
                PhoneNumber = franchise.PhoneNumber,
                Email = franchise.Email
            };

        }

  

        public async Task UpdateFranchiseUser(int id, FranchiseUserDTO model)
        {
            var Franchise = await _repo.GetAsync(id);
            if (Franchise == null)
            {
                throw new LMEGenericException($"Transaction Not Exist");
            }

            Franchise.FirstName = model.FirstName;
            Franchise.MiddleName = model.MiddleName;
            Franchise.LastName = model.LastName;
            Franchise.Address = model.Address;
            Franchise.IsFirstTimeLogin = model.IsFirstTimeLogin;
            Franchise.OptionalPhoneNumber = model.OptionalPhoneNumber;
            Franchise.Image = model.Image;
            Franchise.RefreshToken = model.RefreshToken;
            Franchise.Title = model.Title;
            Franchise.DeviceToken = model.DeviceToken;
            Franchise.Referrer = model.Referrer;
            Franchise.ReferralCode = model.ReferralCode;
            Franchise.NextOfKinName = model.NextOfKinName;
            Franchise.NextOfKinPhone = model.NextOfKinPhone;
            Franchise.DateOfBirth = model.DateOfBirth;
            Franchise.AccountConfirmationCode = model.AccountConfirmationCode;
            Franchise.Photo = model.Photo;
            Franchise.OTP = model.OTP;
            Franchise.PhoneNumber = model.PhoneNumber;
            Franchise.Email = model.Email;

            await _unitOfWork.SaveChangesAsync();
                          
        }

        public async Task RemoveFranchise(int id)
        {
            var franchise  = await _repo.GetAsync(id);

            if (franchise == null)
            {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.FARE_NOT_EXIST);
            }

            _repo.Delete(franchise);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}