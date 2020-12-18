using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Travel.Core.Exceptions;
using Travel.Data.Repository;
using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IStateService
    {
        Task<IPagedList<StateDTO>> GetStates(int page, int size, string query = null);
        Task<StateDTO> GetStateById(int stateId);
        Task AddState(StateDTO state);
        Task UpdateState(int stateId, StateDTO state);
        Task RemoveState(int stateId);
    }

    public class StateService : IStateService
    {
        private readonly IRepository<State> _stateRepo;
        private readonly IRepository<Region> _regionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public StateService(
            IRepository<State> stateRepo,
            IRepository<Region> regionRepo,
            IUnitOfWork unitOfWork,
            IServiceHelper serviceHelper)
        {
            _stateRepo = stateRepo;
            _regionRepo = regionRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        private async Task<bool> IsValidRegion(int regionId)
        {
            return regionId > 0 &&
                 await _regionRepo.ExistAsync(m => m.Id == regionId);
        }

        public async Task AddState(StateDTO stateDto)
        {
            if (!await IsValidRegion(stateDto.RegionId)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.REGION_NOT_EXIST);
            }

            stateDto.Name = stateDto.Name.Trim();

            if (await _stateRepo.ExistAsync(v => v.Name == stateDto.Name)) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_EXIST);
            }

            _stateRepo.Insert(new State
            {
                Id = stateDto.Id,
                Name = stateDto.Name,
                RegionId = stateDto.RegionId
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<StateDTO> GetStateById(int stateId)
        {
            var state = await _stateRepo.GetAsync(stateId);

            if (state == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            return new StateDTO
            {
                Id = state.Id,
                Name = state.Name,
                RegionId = state.RegionId
            };
        }

        public Task<IPagedList<StateDTO>> GetStates(int page, int size, string query = null)
        {
            var states =
                from state in _stateRepo.GetAll()
                join region in _regionRepo.GetAll() on state.RegionId equals region.Id
                orderby state.Id descending
                where string.IsNullOrWhiteSpace(query) || state.Name.Contains(query)
                select new StateDTO
                {
                    Id = state.Id,
                    Name = state.Name,
                    RegionId = region.Id,
                    RegionName = region.Name
                };

            return states.AsNoTracking().ToPagedListAsync(page, size);
        }

        public async Task RemoveState(int stateId)
        {
            var state = await _stateRepo.GetAsync(stateId);

            if (state == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            _stateRepo.Delete(state);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateState(int stateId, StateDTO state)
        {
            var states = await _stateRepo.GetAsync(stateId);

            if (states == null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.STATE_NOT_EXIST);
            }

            states.Name = state.Name.Trim();

            await _unitOfWork.SaveChangesAsync();
        }
    }
}