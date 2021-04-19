using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("label")]
        [JsonProperty("label")]
        public string Label { get; set; }

        [Column("key_code")]
        [JsonProperty("key_code")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonProperty("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("user_roles")]
        public ICollection<UserRole> UserRoles { get; set; }

        [NotMapped]
        [JsonProperty("role_resources_accesses")]
        public ICollection<RoleResourceAccess> RoleResourcesAccess { get; set; }
    }
}
