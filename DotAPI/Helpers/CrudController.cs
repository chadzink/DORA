using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using DORA.DotAPI.Context.Repositories;
using DORA.DotAPI.Models;
using DORA.DotAPI.Helpers;
using System;
using System.Security.Claims;
using DORA.DotAPI.Context.Entities;

namespace DORA.DotAPI.Controllers
{
    public enum ResourceAccessType
    {
        CREATE,
        READ,
        UPDATE,
        DELETE
    }

    public class AccessCrudController<TEntityRepository, TContext, TEntity, TUser> : ControllerBase
        where TEntityRepository : Repository<TContext, TEntity>
    {
        private readonly TEntityRepository _dataRepository;
        private readonly string _resourceCode = null;
        private readonly bool _includeUser = false;

        public AccessCrudController(
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
        public bool NoAccess(ResourceAccessType ResourceAccess, string resourceCode)
        {
            IEnumerable<Claim> roleClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role);
            bool hasAccess = false;

            switch (ResourceAccess)
            {
                case ResourceAccessType.CREATE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && _dataRepository.CreateAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.READ:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && _dataRepository.ReadAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.UPDATE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && _dataRepository.UpdateAccess(resourceCode, roleClaims));
                    break;
                case ResourceAccessType.DELETE:
                    hasAccess = (roleClaims != null && roleClaims.Count() > 0 && _dataRepository.DeleteAccess(resourceCode, roleClaims));
                    break;
            }

            return !hasAccess;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool NoAccess(ResourceAccessType ResourceAccess)
        {
            return this.NoAccess(ResourceAccess, this._resourceCode);
        }
        public bool NoCreate { get { return this.NoAccess(ResourceAccessType.CREATE); } }
        public bool NoRead { get { return this.NoAccess(ResourceAccessType.READ); } }
        public bool NoUpdate { get { return this.NoAccess(ResourceAccessType.UPDATE); } }
        public bool NoDelete { get { return this.NoAccess(ResourceAccessType.DELETE); } }
        public bool IncludeUser { get { return this._includeUser; } }
        public TEntityRepository DataRepository { get { return this._dataRepository; } }

        [ApiExplorerSettings(IgnoreApi = true)]
        public ObjectResult InvalidAccess()
        {
            JsonDataError JsonErr = new JsonDataError(string.Format("Invalid Acces to Resource [{0}]", this._resourceCode));
            return new ObjectResult(JsonErr.Serialize()) { StatusCode = 403 };
        }

        /// <summary>
        ///     Get filter options for the Entity
        /// </summary>
        /// <remarks>
        ///     Gets the options for filtering the Entity records.
        /// </remarks>
        /// <response code="201">JSON with columns and operators</response>
        /// <response code="403">Invalid Access In Resource Manager</response>
        [HttpGet("filters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult FilterInfo()
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            FilterInfo<TEntity> filterInfo = new FilterInfo<TEntity>();

            return Ok(new JsonData<FilterInfo<TEntity>>(filterInfo).Serialize());
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
        public IActionResult ApplyFilter([FromBody] List<FilterField> filters = null, [FromQuery] int page = 1, [FromQuery] int size = 25)
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            if (this.IncludeUser)
                _dataRepository.SetUser(this.User);

            IQueryable<TEntity> query = _dataRepository.FindAll();

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? FilterResult<TEntity>.ToJson(query, user, filters, page, size)
                : FilterResult<TEntity>.ToJson(query, filters, page, size);

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
        public IActionResult Get([FromQuery] int page = 1, [FromQuery] int size = 25/*, [FromQuery] string order = "Label ASC" */)
        {
            // check READ role access to this resource
            if (this.NoRead)
                return this.InvalidAccess();

            if (this.IncludeUser)
                _dataRepository.SetUser(this.User);

            IQueryable<TEntity> query = _dataRepository.FindAll();
            PagedResults<TEntity> paged = Paging<TEntity>.Page(query, page, size/*, order*/);

            List<TEntity> entities = paged != null && paged.query != null ? paged.query.ToList() : null;

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

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
                _dataRepository.SetUser(this.User);

            Guid Id;
            if (!Guid.TryParse(id, out Id))
            {
                JsonDataError JsonErr = new JsonDataError("Invalid Id format");
                return NotFound(JsonErr.Serialize());
            }

            TEntity entity = _dataRepository.Find(Id);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The record couldn't be found.");
                return NotFound(JsonErr.Serialize());
            }

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(entity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(entity);

            return Ok(jsonResult.Serialize());
        }

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
                _dataRepository.SetUser(this.User);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("Role is null");
                return BadRequest(JsonErr.Serialize()); ;
            }

            TEntity newEntity = _dataRepository.Create(entity);

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

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
                _dataRepository.SetUser(this.User);

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

            TEntity updateEntity = _dataRepository.Find(Id);

            if (updateEntity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Entity record couldn't be found");
                return NotFound(JsonErr.Serialize());
            }

            TEntity updatedEntity = _dataRepository.Update(updateEntity, entity);

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

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
                _dataRepository.SetUser(this.User);

            Guid Id;
            if (!Guid.TryParse(id, out Id))
            {
                JsonDataError JsonErr = new JsonDataError("Invalid Id format");
                return NotFound(JsonErr.Serialize());
            }

            TEntity entity = _dataRepository.Find(Id);

            if (entity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Role record couldn't be found");
                return NotFound(JsonErr.Serialize());
            }

            TEntity deletedEntity = _dataRepository.Delete(entity);

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;
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
                _dataRepository.SetUser(this.User);

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

            TEntity restoredEntity = _dataRepository.Restore(Id);

            if (restoredEntity == null)
            {
                JsonDataError JsonErr = new JsonDataError("The Entity record couldn't be found or doe snot have restore feature.");
                return NotFound(JsonErr.Serialize());
            }

            User user = this.IncludeUser ? _dataRepository.CurrentUser() : null;

            JsonData<TEntity> jsonResult = this.IncludeUser
                ? new JsonData<TEntity>(restoredEntity, user.JwtToken, user.RefreshJwtToken)
                : new JsonData<TEntity>(restoredEntity);

            return Ok(jsonResult.Serialize());
        }
    }
}