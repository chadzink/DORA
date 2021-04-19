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
        [JsonProperty("resource_id")]
        [ForeignKey(nameof(Resource))]
        public Guid ResourceId { get; set; }

        [JsonIgnore]
        public Resource Resource { get; set; }

        [Column("key_code")]
        [JsonProperty("key_code")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonProperty("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("resources")]
        public ICollection<Resource> Resources { get; set; }

        [NotMapped]
        [JsonProperty("role_resource_accesses")]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }
    }
}