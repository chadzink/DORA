using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("label")]
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [Column("key_code")]
        [JsonPropertyName("key_code")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonPropertyName("user_roles")]
        public ICollection<UserRole> UserRoles { get; set; }

        [NotMapped]
        [JsonPropertyName("role_resources_accesses")]
        public ICollection<RoleResourceAccess> RoleResourcesAccess { get; set; }
    }
}
