using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DORA.Access.Context.Entities;
using Newtonsoft.Json;

namespace DORA.Navigation.Context.Entities
{
    [Table("role_navigation_group")]
    public class RoleNavGroup
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        // Group
        [Column("nav_group_id")]
        [JsonProperty("navGroupId")]
        [ForeignKey(nameof(NavGroup))]
        public Guid NavGroupId { get; set; }

        [JsonProperty("navGroup")]
        public NavGroup NavGroup { get; set; }

        [Column("role_id")]
        [JsonProperty("roleId")]
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [JsonProperty("role")]
        public Role Role { get; set; }
    }
}
