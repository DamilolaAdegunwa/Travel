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
    public interface ISubRouteService
    {
        Task<IPagedList<SubRouteDTO>> GetSubRoutes(int pageNumber, int pageSize, string query = null);
        Task AddSubRoute(SubRouteDTO subrouteDto);
        Task<List<SubRouteDTO>> GetRouteById(int routeId);
        Task<SubRouteDTO> GetSubRouteById(int id);
        Task UpdateSubRoute(int id, SubRouteDTO subroute);
        Task<List<SubRouteViewModel>> GetByRouteViewId(int routeId);
    }

    public class SubRouteService : ISubRouteService
    {
        private readonly IRepository<SubRoute> _repo;
        private readonly IRepository<Route> _routeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceHelper _serviceHelper;

        public SubRouteService(
            IRepository<SubRoute> repo,
            IRepository<Route> routeRepo,
            IUnitOfWork unitOfWork,
            IServiceHelper serviceHelper)
        {
            _repo = repo;
            _routeRepo = routeRepo;
            _unitOfWork = unitOfWork;
            _serviceHelper = serviceHelper;
        }

        public async Task AddSubRoute(SubRouteDTO subrouteDto)
        {
            var subroute = await _routeRepo.GetAsync(subrouteDto.NameId.GetValueOrDefault());

            _repo.Insert(new SubRoute
            {
                RouteId = subrouteDto.RouteId,
                CreatorUserId = _serviceHelper.GetCurrentUserId(),
                NameId = subrouteDto.NameId.GetValueOrDefault(),
                Name = subroute.Name
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<SubRouteDTO>> GetRouteById(int routeId)
        {
            var subroutes =
                from subroute in _repo.GetAll()
                join route in _routeRepo.GetAll() on subroute.RouteId equals route.Id

                where route.Id == routeId

                select new SubRouteDTO
                {
                    RouteId = route.Id,
                    RouteName = route.Name,
                    Id = subroute.Id,
                    NameId = subroute.NameId,
                    Name = subroute.Name
                };

            if (subroutes is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            return await subroutes.AsNoTracking().ToListAsync();
        }

        public async Task<List<SubRouteViewModel>> GetByRouteViewId(int routeId)
        {
            var subroutes =
                from subroute in _repo.GetAll()
                join route in _routeRepo.GetAll() on subroute.RouteId equals route.Id

                where route.Id == routeId

                select new SubRouteViewModel
                {
                    RouteId = route.Id,
                    RouteName = route.Name,
                    SubRouteId = subroute.Id,
                    SubRouteNameId = subroute.NameId,
                    SubRouteName = subroute.Name
                };

            if (subroutes is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            return await subroutes.AsNoTracking().ToListAsync();
        }

        public async Task<SubRouteDTO> GetSubRouteById(int id)
        {
            var subroute = await _repo.GetAllIncluding(x => x.Route)
                                .FirstOrDefaultAsync(x => x.Id == id);

            if (subroute is null) {
                throw await _serviceHelper.GetExceptionAsync(ErrorConstants.ROUTE_NOT_EXIST);
            }

            return new SubRouteDTO
            {
                RouteId = subroute.RouteId,
                RouteName = subroute.Route.Name,
                Id = subroute.Id,
                Name = subroute.Name
            };
        }

        public Task<IPagedList<SubRouteDTO>> GetSubRoutes(int pageNumber, int pageSize, string query)
        {
            var subroutes = from sRoute in _repo.GetAllIncluding(x => x.Route)
                            where string.IsNullOrWhiteSpace(query) || sRoute.Name.Contains(query)
                            select new SubRouteDTO
                            {
                                RouteId = sRoute.RouteId,
                                RouteName = sRoute.Route.Name,
                                Id = sRoute.Id,
                                Name = sRoute.Name
                            };

            return subroutes.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task UpdateSubRoute(int id, SubRouteDTO subrouteDto)
        {
            var subroute = await _repo.GetAsync(id);

            subroute.RouteId = subrouteDto.RouteId;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}