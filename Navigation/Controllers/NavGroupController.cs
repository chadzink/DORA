using Microsoft.AspNetCore.Mvc;
using DORA.Navigation.Context;
using DORA.Navigation.Context.Entities;
using DORA.Navigation.Context.Repositories;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using DORA.Access.Context.Entities;
using System;
using DORA.Access.Helpers;
using Microsoft.AspNetCore.Http;

namespace DORA.Navigation.Controllers
{
    [ApiExplorerSettings(GroupName = "Navigation-Group")]
    public class NavGroupController
        : AccessCrudController<NavGroupRepository, NavigationContext, NavGroup, User>
    {
        private NavigationContext _context = null;
        private IConfiguration _configuration = null;

        public NavGroupController(
            NavigationContext context,
            IConfiguration configuration
        )
            : base(new NavGroupRepository(context, configuration), "NAV-GROUP", true)
        {
            this._context = context;
            this._configuration = configuration;
        }

        public NavigationContext Context { get { return this._context; } }
        public IConfiguration Configuration { get { return this._configuration; } }

        /// <summary>
        ///     Assign Role to Navigation Group
        /// </summary>
        /// <remarks>
        ///     Assigned a navigation group to a role and allows users of that role to access the navigation group
        /// </remarks>
        /// <response code="201">JSON with {success: true, roles: [record]}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("assign-role-nav-group")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AssignRoleToNavGroup(string RoleId, string NavGroupId)
        {
            // check READ role access to this resource
            if (this.NoUpdate)
                return this.InvalidAccess();

            DataRepository.SetUser(this.User);

            Guid guidRoleID = Guid.Parse(RoleId);
            Guid guidNavGroupId = Guid.Parse(NavGroupId);

            // Check if the role has already been assigned to the nav group
            RoleNavGroupRepository roleNavGroupRepo = new RoleNavGroupRepository(this.Context, this.Configuration);

            RoleNavGroup existingRoleGroup = roleNavGroupRepo.FindOneBy(r => r.RoleId == guidRoleID && r.NavGroupId == guidNavGroupId);

            if (existingRoleGroup != null)
            {
                JsonDataError JsonErr = new JsonDataError("Role already linked to navigation group.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            RoleNavGroup newRoleGroup = roleNavGroupRepo.Create(new RoleNavGroup
            {
                Id = Guid.NewGuid(),
                RoleId = guidRoleID,
                NavGroupId = guidNavGroupId,
            });

            if (newRoleGroup == null)
            {
                JsonDataError JsonErr = new JsonDataError("Unable to create role link to navigation group on CREATE.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            User user = DataRepository.CurrentUser();
            JsonData<RoleNavGroup> jsonResult = new JsonData<RoleNavGroup>(newRoleGroup, user.JwtToken, user.RefreshJwtToken);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Remove Role from Navigation Group
        /// </summary>
        /// <remarks>
        ///     Removes a role from a navigation group and stops users of that role from accessing the navigation group
        /// </remarks>
        /// <response code="201">JSON with {success: true, roles: [record]}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("remove-role-nav-group")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RemoveRoleToNavGroup(string RoleId, string NavGroupId)
        {
            // check READ role access to this resource
            if (this.NoUpdate)
                return this.InvalidAccess();

            DataRepository.SetUser(this.User);

            Guid guidRoleID = Guid.Parse(RoleId);
            Guid guidNavGroupId = Guid.Parse(NavGroupId);

            // Check if the role has already been assigned to the nav group
            RoleNavGroupRepository roleNavGroupRepo = new RoleNavGroupRepository(this.Context, this.Configuration);

            RoleNavGroup existingRoleGroup = roleNavGroupRepo.FindOneBy(r => r.RoleId == guidRoleID && r.NavGroupId == guidNavGroupId);

            if (existingRoleGroup == null)
            {
                JsonDataError JsonErr = new JsonDataError("Role not linked to navigation group.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            RoleNavGroup deletedRoleGroup = roleNavGroupRepo.Delete(existingRoleGroup);

            User user = DataRepository.CurrentUser();
            JsonData<RoleNavGroup> jsonResult = new JsonData<RoleNavGroup>(deletedRoleGroup, user.JwtToken, user.RefreshJwtToken);

            return Ok(jsonResult.Serialize());
        }
    }
}
