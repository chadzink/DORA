using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("access_resources")]
    public class Resource
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("key_code")]
        [JsonPropertyName("key_code")]
        public string KeyCode { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<ResourceAccess> ResourceAccesses { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<RoleResourceAccess> RoleResourceAccesses { get; set; }
    }
}
