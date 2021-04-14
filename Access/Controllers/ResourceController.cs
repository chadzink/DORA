using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DORA.Access.Context;
using DORA.Access.Context.Entities;
using DORA.Access.Context.Repositories;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.Access.Controllers
{
    [ApiController]
    [Authorize, Route("[controller]")]
    public class ResourceController
        : AccessCrudController<ResourceRepository, AccessContext, Resource, User>
    {
        public ResourceController(
            AccessContext accessContext,
            IConfiguration configuration
        ) : base (new ResourceRepository(accessContext, configuration), "RESOURCE", false) { }
    }
}
