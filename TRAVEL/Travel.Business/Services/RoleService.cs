using IPagedList;
using Travel.Core.Domain.DataTransferObjects;
using Travel.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Travel.Business.Services
{
    public interface IRoleService
    {
        Task<IList<Claim>> GetClaimsAsync(string name);
        Task<bool> CreateAsync(RoleDTO role);
        Task<IPagedList<RoleDTO>> Get(int pageNumber, int pageSize, string query);

        Task<RoleDTO> FindByIdAsync(int roleId);
        Task<Role> FindByName(string name);
    }

    public class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleService(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public virtual Task<Role> FindByNameAsync(string name)
        {
            return _roleManager.FindByNameAsync(name);
        }

        public virtual Task<IdentityResult> CreateAsync(Role role)
        {
            return _roleManager.CreateAsync(role);
        }

        public virtual Task<IdentityResult> AddClaimToRoleAsync(Role role, Claim claim)
        {
            return _roleManager.AddClaimAsync(role, claim);
        }

        public virtual async Task<bool> CreateAsync(RoleDTO role)
        {
            var dbrole = FindByNameAsync(role.Name).Result;

            if (dbrole is null) {

                dbrole = new Role
                {
                    Name = role.Name,
                    IsActive = role.IsActive,
                };

                var createStatus = await CreateAsync(dbrole);

                if (createStatus.Succeeded) {
                    var claims = PermissionClaimsProvider.GetClaims();

                    foreach (var item in role.Claims) {
                        var claim = claims.FirstOrDefault(x => x.Value == item);
                        if (claim != null) {
                            await AddClaimToRoleAsync(dbrole, claim);
                        }
                    }
                }

                return createStatus.Succeeded;
            }
            return false;
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(string name)
        {
            var role = await FindByNameAsync(name);

            return role is null ? new List<Claim>() : await _roleManager.GetClaimsAsync(role);
        }

        public async Task<Role> FindByName(string name)
        {
            return await _roleManager.FindByNameAsync(name);
        }

        public Task<IPagedList<RoleDTO>> Get(int pageNumber, int pageSize, string query)
        {
            var roles = from r in _roleManager.Roles
                        where string.IsNullOrWhiteSpace(query) || r.Name.Contains(query)
                        orderby r.CreationTime descending

                        select new RoleDTO
                        {
                            Id = r.Id,
                            Name = r.Name
                        };

            return roles.ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<RoleDTO> FindByIdAsync(int roleId)
        {
            var result = await _roleManager.FindByIdAsync($"{roleId}");

            return result == null ? null : new RoleDTO { Id = result.Id, Name = result.Name };
        }
    }
}