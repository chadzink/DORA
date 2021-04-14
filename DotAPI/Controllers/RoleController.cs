using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Context.Repositories;
using DORA.DotAPI.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Controllers
{
    [ApiController]
    [Authorize, Route("[controller]")]
    public class RoleController
        : AccessCrudController<RoleRepository, AccessContext, Role, User>
    {
        public RoleController(
            AccessContext accessContext,
            IConfiguration configuration
        )
            : base(new RoleRepository(accessContext, configuration), "ROLE", true) { }
    }
}
