using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.DotAPI.Context.Entities
{
    [Table("access_roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("label")]
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [Column("name_canonical")]
        [JsonPropertyName("name_canonical")]
        public string NameCanonical { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<RoleResourceAccess> RoleResourcesAccess { get; set; }
    }
}
