using Microsoft.AspNetCore.Mvc;
using DORA.Navigation.Context;
using DORA.Navigation.Context.Entities;
using DORA.Navigation.Context.Repositories;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using DORA.Access.Context.Entities;
using Microsoft.AspNetCore.Http;
using System;
using DORA.Access.Helpers;

namespace DORA.Navigation.Controllers
{
    [ApiExplorerSettings(GroupName = "Navigation-Item")]
    public class NavItemController
        : AccessCrudController<NavItemRepository, NavigationContext, NavItem, User>
    {
        private NavigationContext _context = null;
        private IConfiguration _configuration = null;

        public NavItemController(
            NavigationContext context,
            IConfiguration configuration
        )
            : base(new NavItemRepository(context, configuration), "NAV-ITEM", true)
        {
            this._context = context;
            this._configuration = configuration;
        }

        public NavigationContext Context { get { return this._context; } }
        public IConfiguration Configuration { get { return this._configuration; } }

        /// <summary>
        ///     Assign Role to Navigation Item
        /// </summary>
        /// <remarks>
        ///     Assigned a navigation item to a role and allows users of that role to access the navigation item
        /// </remarks>
        /// <response code="201">JSON with {success: true, roles: [record]}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("assign-role-nav-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AssignRoleToNavItem(string RoleId, string NavItemId)
        {
            // check READ role access to this resource
            if (this.NoUpdate)
                return this.InvalidAccess();

            DataRepository.SetUser(this.User);

            Guid guidRoleID = Guid.Parse(RoleId);
            Guid guidNavItemId = Guid.Parse(NavItemId);

            // Check if the role has already been assigned to the nav item
            RoleNavItemRepository roleNavItemRepo = new RoleNavItemRepository(this.Context, this.Configuration);

            RoleNavItem existingRoleItem = roleNavItemRepo.FindOneBy(r => r.RoleId == guidRoleID && r.NavItemId == guidNavItemId);

            if (existingRoleItem != null)
            {
                JsonDataError JsonErr = new JsonDataError("Role already linked to navigation item.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            RoleNavItem newRoleItem = roleNavItemRepo.Create(new RoleNavItem
            {
                Id = Guid.NewGuid(),
                RoleId = guidRoleID,
                NavItemId = guidNavItemId,
            });

            if (newRoleItem == null)
            {
                JsonDataError JsonErr = new JsonDataError("Unable to create role link to navigation item on CREATE.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            User user = DataRepository.CurrentUser();
            JsonData<RoleNavItem> jsonResult = new JsonData<RoleNavItem>(newRoleItem, user.JwtToken, user.RefreshJwtToken);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Remove Role from Navigation Item
        /// </summary>
        /// <remarks>
        ///     Removes a role from a navigation item and stops users of that role from accessing the navigation item
        /// </remarks>
        /// <response code="201">JSON with {success: true, roles: [record]}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("remove-role-nav-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RemoveRoleToNavItem(string RoleId, string NavItemId)
        {
            // check READ role access to this resource
            if (this.NoUpdate)
                return this.InvalidAccess();

            DataRepository.SetUser(this.User);

            Guid guidRoleID = Guid.Parse(RoleId);
            Guid guidNavItemId = Guid.Parse(NavItemId);

            // Check if the role has already been assigned to the nav item
            RoleNavItemRepository roleNavItemRepo = new RoleNavItemRepository(this.Context, this.Configuration);

            RoleNavItem existingRoleItem = roleNavItemRepo.FindOneBy(r => r.RoleId == guidRoleID && r.NavItemId == guidNavItemId);

            if (existingRoleItem == null)
            {
                JsonDataError JsonErr = new JsonDataError("Role not linked to navigation item.");
                return BadRequest(JsonErr.Serialize()); ;
            }

            RoleNavItem deletedRoleItem = roleNavItemRepo.Delete(existingRoleItem);

            User user = DataRepository.CurrentUser();
            JsonData<RoleNavItem> jsonResult = new JsonData<RoleNavItem>(deletedRoleItem, user.JwtToken, user.RefreshJwtToken);

            return Ok(jsonResult.Serialize());
        }
    }
}
