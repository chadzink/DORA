using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using DORA.Access.Models;
using DORA.Access.Helpers;
using System;
using System.Security.Claims;
using DORA.Access.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace DORA.Access.Common
{
    public class AccessCrudController<TEntityRepository, TContext, TEntity, TUser> : AccessViewController<TEntityRepository, TContext, TEntity, TUser>
        where TEntityRepository : Repository<TContext, TEntity>
        where TContext : DbContext
    {
        public AccessCrudController(
            TEntityRepository dataRepository,
            string resourceCode,
            bool includeUser = false
        ) : base(dataRepository, resourceCode, includeUser) { }

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool NoAccess(ResourceAccessType ResourceAccess, string resourceCode)
        {
            IEnumerable<Claim> roleClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role);
            bool hasAccess = false;

            switch (ResourceAccess)
            {
                case ResourceAccessType.CREATE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && DataRepository.CreateAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.READ:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && DataRepository.ReadAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.UPDATE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && DataRepository.UpdateAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.DELETE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && DataRepository.DeleteAccess(resourceCode, roleClaims));
                    break;
            }

            return !hasAccess;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool NoAccess(ResourceAccessType ResourceAccess)
        {
            return this.NoAccess(ResourceAccess, this.ResourceCode);
        }
        public bool NoCreate { get { return this.NoAccess(ResourceAccessType.CREATE); } }
        public bool NoUpdate { get { return this.NoAccess(ResourceAccessType.UPDATE); } }
        public bool NoDelete { get { return this.NoAccess(ResourceAccessType.DELETE); } }

        /// <summary>
        ///     Add new Entity
        /// </summary>
        /// <remarks>
        ///     Adds Entity record. Must have UPDATE authorization in user roles.
        /// </remarks>
        /// <response code="201">Common JSON</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        /// <response code="404">Common JSON Error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PostAdd([FromBody] TEntity entity)
        {
            // check CREATE role access to this resource
            if (this.NoCreate)
                return this.InvalidAccess();

            if (this.IncludeUser)
                DataRepository.SetUser(this.User);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("Role is null");
                return BadRequest(JsonErr.Serialize()); ;
            }

            TEntity newEntity = DataRepository.Create(entity);

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(newEntity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(newEntity);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Update Entity by Id
        /// </summary>
        /// <remarks>
        ///     Update the Entity record that matches Id. Must have UPDATE authorization in user roles.
        /// </remarks>
        /// <response code="201">Common JSON</response>
        /// <response code="400">No updates sent: Common Error JSON</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        /// <response code="404">Id did not match: Common Error JSON</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutUpdateById(string id, [FromBody] TEntity entity)
        {
            // check UPDATE role access to this resource
            if (this.NoUpdate)
                return this.InvalidAccess();

            if (this.IncludeUser)
                DataRepository.SetUser(this.User);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("Role is null");
                return BadRequest(JsonErr.Serialize());
            }

            Guid Id;
            if (!Guid.TryParse(id, out Id))
            {
                JsonDataError JsonErr = new JsonDataError("Invalid Id format");
                return NotFound(JsonErr.Serialize());
            }

            TEntity updateEntity = DataRepository.Find(Id);

            if (updateEntity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Entity record couldn't be found");
                return NotFound(JsonErr.Serialize());
            }

            TEntity updatedEntity = DataRepository.Update(updateEntity, entity);

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(updatedEntity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(updatedEntity);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Remove Entity by Id
        /// </summary>
        /// <remarks>
        ///     Deletes Entity record that matches Id to current date time. Must have DELETE authorization in user roles.
        /// </remarks>
        /// <response code="200">The entity removed.</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        /// <response code="404">No record found for id: Common JSON Error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteById(string id)
        {
            // check DELETE role access to this resource
            if (this.NoDelete)
                return this.InvalidAccess();

            if (this.IncludeUser)
                DataRepository.SetUser(this.User);

            Guid Id;
            if (!Guid.TryParse(id, out Id))
            {
                JsonDataError JsonErr = new JsonDataError("Invalid Id format");
                return NotFound(JsonErr.Serialize());
            }

            TEntity entity = DataRepository.Find(Id);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Role record couldn't be found");
                return NotFound(JsonErr.Serialize());
            }

            TEntity deletedEntity = DataRepository.Delete(new TEntity[] { entity }).First();

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;
            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(deletedEntity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(deletedEntity);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Restore Entity by Id
        /// </summary>
        /// <remarks>
        ///     Restores Entity record that matches Id to null. Must have DELETE authorization in user roles.
        /// </remarks>
        /// <response code="200">Common JSON</response>
        /// <response code="400">Id is null: Common JSON Error</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        /// <response code="404">No record found for id: Common JSON Error</response>
        [HttpPut("restore/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RestoreById(string id)
        {
            // check DELETE role access to this resource
            if (this.NoDelete)
                return this.InvalidAccess();

            if (this.IncludeUser)
                DataRepository.SetUser(this.User);

            if (id == null)
            {
                JsonDataError JsonErr = new JsonDataError("Entity Id is null");
                return BadRequest(JsonErr.Serialize());
            }

            Guid Id;
            if (!Guid.TryParse(id, out Id))
            {
                JsonDataError JsonErr = new JsonDataError("Invalid Id format");
                return NotFound(JsonErr.Serialize());
            }

            TEntity restoredEntity = DataRepository.Restore(Id);

            if (restoredEntity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Entity record couldn't be found or doe snot have restore feature.");
                return NotFound(JsonErr.Serialize());
            }

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(restoredEntity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(restoredEntity);

            return Ok(jsonResult.Serialize());
        }
    }
}