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
        [JsonProperty("keyCode")]
        public string KeyCode { get; set; }

        [Column("sql_object_name")]
        [JsonProperty("sqlObjectName")]
        public string SqlObjectName { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("resourceAccesses")]
        public ICollection<ResourceAccess> ResourceAccesses { get; set; }

        [NotMapped]
        [JsonProperty("roleResourceAccesses")]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }

        [NotMapped]
        [JsonProperty("includedResources")]
        public ICollection<IncludedResource> IncludedResources { get; set; }
    }
}
