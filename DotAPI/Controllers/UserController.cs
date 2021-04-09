using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Context.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DORA.DotAPI.Helpers;
using System;
using DORA.DotAPI.Models.Controllers.User;
using DORA.DotAPI.Common;

namespace DORA.DotAPI.Controllers
{
    [ApiController]
    [Authorize, Route("[controller]")]
    public class UserController
        : AccessCrudController<UserRepository, AccessContext, User, User>
    {
        public UserController(
            AccessContext accessContext,
            IConfiguration configuration
        ) : base(new UserRepository(accessContext, configuration), "USER", true) { }

        /// <summary>
        ///     Assigns roles to user
        /// </summary>
        /// <remarks>
        ///     Assignes the user to the roles provided.
        /// </remarks>
        /// <response code="201">JSON with columns and operators</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("assign-roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AssignRolesToUser([FromBody] AssignUserRoles assignments)
        {
            List<UserRole> rolesActuallyAssigned = new List<UserRole>();
            User user = this.IncludeUser ? this.DataRepository.CurrentUser() : null;

            // check READ role access to this resource
            if (this.NoAccess(ResourceAccessType.CREATE, "USER-ROLE") || this.NoUpdate)
            {
                return this.InvalidAccess();
            }

            if (this.IncludeUser)
                this.DataRepository.SetUser(this.User);

            // Setup a user role repository and a role repository using the current controller repository
            UserRoleRepository userRoleRepository = new UserRoleRepository(
                this.DataRepository.dbContext,
                this.DataRepository.Config
            );

            RoleRepository roleRepository = new RoleRepository(
                this.DataRepository.dbContext,
                this.DataRepository.Config
            );

            // check that the repositories got created
            if (userRoleRepository == null || roleRepository == null)
            {
                JsonDataError JsonErr = new JsonDataError("Unable to connect to user roles.");
                return NotFound(JsonErr.Serialize());
            }

            // by fetching the suer from the repo, access to user is checked.
            User dbUser = this.DataRepository.Find(assignments.assignToUser.Id.Value);

            if (dbUser == null)
            {
                JsonDataError JsonErr = new JsonDataError("Invalid user access");
                return NotFound(JsonErr.Serialize());
            }

            // assign the current user to the repository to filter out access rights at record level
            userRoleRepository.SetUser(this.User);

            foreach (Role role in assignments.assignedRoles)
            {
                // get the role from the repository to verify user access to role
                Role dbRole = roleRepository.Find(role.Id.Value);

                if (dbRole != null)
                {
                    // passed all checks, now the new join record can be added
                    rolesActuallyAssigned.Add( userRoleRepository.Create(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id.Value,
                        UserId = dbUser.Id.Value
                    }));
                }
            }

            JsonData<UserRole> jsonResult = new JsonData<UserRole>(rolesActuallyAssigned, user.JwtToken, user.RefreshJwtToken);

            return Ok(jsonResult.Serialize());
        }
    }
}
