using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("included_resources")]
    public class IncludedResource
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

        [Column("included_recource_id")]
        [JsonPropertyName("included_recource_id")]
        [ForeignKey(nameof(IncludedRecourceId))]
        public Guid IncludedRecourceId { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Resource IncludedRecource { get; set; }

        [Column("collection_name")]
        [JsonPropertyName("collection_name")]
        public string CollectionName { get; set; }

        [Column("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
