using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DORA.Access.Context.Entities;
using Newtonsoft.Json;

namespace DORA.Navigation.Context.Entities
{
    [Table("role_navigation_item")]
    public class RoleNavItem
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        // Group
        [Column("nav_item_id")]
        [JsonProperty("navItemId")]
        [ForeignKey(nameof(NavItem))]
        public Guid NavItemId { get; set; }

        [JsonProperty("navItem")]
        public NavItem NavItem { get; set; }

        [Column("role_id")]
        [JsonProperty("roleId")]
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [JsonProperty("role")]
        public Role Role { get; set; }
    }
}
