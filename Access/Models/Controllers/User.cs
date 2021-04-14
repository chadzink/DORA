using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DORA.Access.Models.Controllers.User
{
    public class AssignUserRoles
    {
        [Required]
        public DORA.Access.Context.Entities.User assignToUser { get; set; }
        public List<DORA.Access.Context.Entities.Role> assignedRoles { get; set; }
    }
}
