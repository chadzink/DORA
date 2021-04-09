using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Context.Repositories;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Common;

namespace DORA.DotAPI.Controllers
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
