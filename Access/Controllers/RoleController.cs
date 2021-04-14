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
