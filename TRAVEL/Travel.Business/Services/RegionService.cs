using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Core.Utils;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IRegionService
    {
        Task<IPagedList<RegionDTO>> GetRegions(int page, int size, string search = null);
        Task<RegionDTO> GetRegionById(int regionId);
        Task AddRegion(RegionDTO region);
        Task UpdateRegion(int regionId, RegionDTO region);
        Task RemoveRegion(int regionId);
    }

    public class RegionService : IRegionService
    {
        private readonly IRepository<Region> _regionRepo;
        private readonly IServiceHelper _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;

        public RegionService(
            IRepository<Region> regionRepo,
            IServiceHelper serviceHelper,
            IUnitOfWork unitOfWork)
        {
            _regionRepo = regionRepo;
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task AddRegion(RegionDTO regionDto)
        {
            regionDto.Name = regionDto.Name.Trim();

            var regionName = regionDto.Name.ToLower();

            if (await _regionRepo.ExistAsync(v => v.Name.ToLower() == regionName)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_EXIST);
            }

            _regionRepo.Insert(new Region
            {
                Name = regionDto.Name,
                CreatorUserId = _serviceHelper.GetCurrentUserId()
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<RegionDTO> GetRegionById(int regionId)
        {
            var region = await _regionRepo.GetAsync(regionId);

            if (region is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_NOT_EXIST);
            }

            return new RegionDTO
            {
                Name = region.Name,
                Id = region.Id
            };
        }

        public Task<IPagedList<RegionDTO>> GetRegions(int page, int size, string search)
        {
            var regions = from region in _regionRepo.GetAll()
                          where string.IsNullOrWhiteSpace(search) || region.Name.Contains(search)
                          orderby region.CreationTime descending
                          select new RegionDTO
                          {
                              Id = region.Id,
                              Name = region.Name,
                              DateCreated = region.CreationTime.ToString(CoreConstants.DateFormat)
                          };

            return regions.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task RemoveRegion(int regionId)
        {
            var region = await _regionRepo.GetAsync(regionId);

            if (region is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_NOT_EXIST);
            }

            _regionRepo.Delete(region);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateRegion(int regionId, RegionDTO regionDto)
        {
            var region = await _regionRepo.GetAsync(regionId);

            if (region is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_NOT_EXIST);
            }

            region.Name = regionDto.Name.Trim();

            await _unitOfWork.SaveChangesAsync();
        }
    }
}