using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.DotAPI.Context.Entities
{
    [Table("access_resource_access")]
    public class ResourceAccess
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("resource_id")]
        [JsonPropertyName("resource_id")]
        [ForeignKey(nameof(Resource))]
        public Guid ResourceId { get; set; }

        [JsonIgnore]
        public Resource Resource { get; set; }

        [Column("key_code")]
        [JsonPropertyName("key_code")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<Resource> Resources { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }
    }
}