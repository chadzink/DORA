using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("resource_accesses")]
    public class ResourceAccess
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("resource_id")]
        [JsonProperty("resourceId")]
        [ForeignKey(nameof(Resource))]
        public Guid ResourceId { get; set; }

        [JsonProperty("resource")]
        public Resource Resource { get; set; }

        [Column("key_code")]
        [JsonProperty("keyCode")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("resources")]
        public ICollection<Resource> Resources { get; set; }

        [NotMapped]
        [JsonProperty("roleResourceAccesses")]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }
    }
}