using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("resources")]
    public class Resource
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("key_code")]
        [JsonPropertyName("key_code")]
        public string KeyCode { get; set; }

        [Column("sql_object_name")]
        [JsonIgnore]
        public string SqlObjectName { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonPropertyName("resource_accesses")]
        public ICollection<ResourceAccess> ResourceAccesses { get; set; }

        [NotMapped]
        [JsonPropertyName("role_resource_accesses")]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }

        [NotMapped]
        [JsonPropertyName("included_resources")]
        public ICollection<IncludedResource> IncludedResources { get; set; }
    }
}
