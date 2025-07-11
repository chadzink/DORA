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
using Access.Helpers;

namespace DORA.Access.Common
{
    public class AccessViewController<TEntityRepository, TContext, TEntity, TUser> : ControllerBase
        where TEntityRepository : Repository<TContext, TEntity>
        where TContext : DbContext
    {
        private readonly TEntityRepository _dataRepository;
        private readonly string _resourceCode = null;
        private readonly bool _includeUser = false;

        public AccessViewController(
            TEntityRepository dataRepository,
            string resourceCode,
            bool includeUser = false
        ) : base()
        {
            _dataRepository = dataRepository;
            _resourceCode = resourceCode;
            _includeUser = includeUser;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool NoAccess(string resourceCode)
        {
            IEnumerable<Claim> roleClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role);
            bool hasAccess = false;
            
            hasAccess = (roleClaims != null && roleClaims.Count() > 0 && DataRepository.ReadAccess(resourceCode, roleClaims));

            return !hasAccess;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool NoAccess()
        {
            return this.NoAccess(this._resourceCode);
        }
        public bool NoRead { get { return this.NoAccess(); } }
        public bool IncludeUser { get { return this._includeUser; } }
        public string ResourceCode { get { return this._resourceCode; } }
        public TEntityRepository DataRepository { get { return this._dataRepository; } }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult InvalidAccess()
        {
            JsonDataError JsonErr = new JsonDataError(string.Format("Invalid Acces to Resource [{0}]", this._resourceCode));
            return new ObjectResult(JsonErr.Serialize()) { StatusCode = 403 };
        }

        /// <summary>
        ///     Utility feature for entity controllers.
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        /// <param name="command">
        ///     filters: Gets the options for filtering the Entity records.
        ///     includes: Gets the options for includes on the Entity.
        ///     typescript: Get tempated typescript for the entity to use with common DORA React library.
        /// </param>
        /// <response code="201">JSON with columns and operators</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpGet("util")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FilterInfo([FromQuery] string command)
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            JsonDataError JsonErr;

            switch(command)
            {
                case "filters":
                    FilterInfo<TEntity> filterInfo = new FilterInfo<TEntity>();
                    return Ok(new JsonData<FilterInfo<TEntity>>(filterInfo).Serialize());
                case "includes":
                    List<IncludedResource> includedResources = DataRepository.IncludedResources(this._resourceCode).ToList();
                    return Ok(new JsonData<IncludedResource>(includedResources).Serialize());
                case "typescript":
                    EntityTypeScript entityTypeScript = new EntityTypeScript(typeof(TEntity));
                    return Ok(entityTypeScript.TypeDefinition);
            }

            if (command.StartsWith("typescript"))
            {
                string typename = command.Split(':')[1];

                if (Type.GetType(typename) == null)
                {
                    JsonErr = new JsonDataError("Type not found, make sure you have the full namespace.");
                    return NotFound(JsonErr.Serialize());
                }

                EntityTypeScript entityTypeScript = new EntityTypeScript(Type.GetType(typename));
                return Ok(entityTypeScript.TypeDefinition);
            }

            JsonErr = new JsonDataError("Invalid Command: Try [filters, includes, typescript, typescript:<typename>]");
            return NotFound(JsonErr.Serialize());
        }

        /// <summary>
        ///     List Entities that met criteria for filter
        /// </summary>
        /// <remarks>
        ///     Gets all the Entity records that match the applied filter. Must have READ authorization in user roles.
        /// </remarks>
        /// <response code="201">JSON with {success: true, roles: [record]}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpPost("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ApplyFilter(
            [FromBody] List<FilterField> filters = null,
            [FromQuery] List<string> includes = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 25,
            [FromQuery] string orderBy = null,
            [FromQuery] string orderDir = "ASC"
            )
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            if (this.IncludeUser)
                _dataRepository.SetUser(this.User);

            IQueryable<TEntity> query = includes != null
                ? DataRepository.FindAllWithIncludes(includes.ToArray())
                : DataRepository.FindAll();

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? FilterResult<TEntity>.ToJson(query, user, filters, includes, page, size, orderBy, orderDir)
                : FilterResult<TEntity>.ToJson(query, filters, includes, page, size, orderBy, orderDir);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     List all Entities
        /// </summary>
        /// <remarks>
        ///     Gets all the Entities records. Must have READ authorization "READ-[controller]" in user roles.
        /// </remarks>
        /// <response code="201">Common JSON</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get(
            [FromQuery] List<string> includes = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 25,
            [FromQuery] string order_by = null,
            [FromQuery] string order_dir = "ASC"
        )
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            if (this.IncludeUser)
                DataRepository.SetUser(this.User);

            IQueryable<TEntity> query = includes != null
                ? DataRepository.FindAllWithIncludes(includes.ToArray())
                : DataRepository.FindAll();

            PagedResults<TEntity> paged = Paging<TEntity>.Page(query, includes, page, size, order_by, order_dir);

            List<TEntity> entities = paged != null && paged.query != null ? paged.query.ToList() : null;

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(entities, user.JwtToken, user.RefreshJwtToken, paged.meta)
                : new JsonData<TEntity>(entities, paged.meta);

            return Ok(jsonResult.Serialize());
        }

        /// <summary>
        ///     Gets Entity by Id
        /// </summary>
        /// <remarks>
        ///     Gets Entity record that matches Id. Must have READ authorization in user roles.
        /// </remarks>
        /// <response code="201">JSON {success: true, role: record}</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        /// <response code="404">JSON {success: false, errors: 'The record couldn't be found.'}</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(string id)
        {
            // check READ role access to this resource
            if (this.NoRead)
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
                JsonDataError JsonErr = new JsonDataError("The record couldn't be found.");
                return NotFound(JsonErr.Serialize());
            }

            User user = this.IncludeUser ? DataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(entity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(entity);

            return Ok(jsonResult.Serialize());
        }
    }
}