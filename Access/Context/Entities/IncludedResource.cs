using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("included_resources")]
    public class IncludedResource
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

        [Column("included_recource_id")]
        [JsonProperty("included_recource_id")]
        [ForeignKey(nameof(IncludedRecourceId))]
        public Guid IncludedRecourceId { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Resource IncludedRecource { get; set; }

        [Column("collection_name")]
        [JsonProperty("collection_name")]
        public string CollectionName { get; set; }

        [Column("description")]
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
