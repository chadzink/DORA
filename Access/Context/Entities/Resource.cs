using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("resources")]
    public class Resource
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("key_code")]
        [JsonProperty("key_code")]
        public string KeyCode { get; set; }

        [Column("sql_object_name")]
        [JsonIgnore]
        public string SqlObjectName { get; set; }

        [Column("archived_stamp")]
        [JsonProperty("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("resource_accesses")]
        public ICollection<ResourceAccess> ResourceAccesses { get; set; }

        [NotMapped]
        [JsonProperty("role_resource_accesses")]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }

        [NotMapped]
        [JsonProperty("included_resources")]
        public ICollection<IncludedResource> IncludedResources { get; set; }
    }
}
