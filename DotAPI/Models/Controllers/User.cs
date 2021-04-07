using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DORA.DotAPI.Models.Controllers.User
{
    public class AssignUserRoles
    {
        [Required]
        public DORA.DotAPI.Context.Entities.User assignToUser { get; set; }
        public List<DORA.DotAPI.Context.Entities.Role> assignedRoles { get; set; }
    }
}
