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
        [JsonProperty("keyCode")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("roleUsers")]
        public ICollection<UserRole> UserRoles { get; set; }

        [NotMapped]
        [JsonProperty("roleResourcesAccesses")]
        public ICollection<RoleResourceAccess> RoleResourcesAccess { get; set; }
    }
}
