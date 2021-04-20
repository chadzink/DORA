using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("role_resource_accesses")]
    public class RoleResourceAccess
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("role_id")]
        [JsonProperty("roleId")]
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [JsonProperty("role")]
        public Role Role { get; set; }

        [Column("resource_id")]
        [JsonProperty("resourceId")]
        [ForeignKey(nameof(Resource))]
        public Guid ResourceId { get; set; }

        [JsonProperty("resource")]
        public Resource Resource { get; set; }

        [Column("resource_access_id")]
        [JsonProperty("resourceAccessId")]
        [ForeignKey(nameof(ResourceAccess))]
        public Guid ResourceAccessId { get; set; }

        [JsonProperty("resourceAccess")]
        public ResourceAccess ResourceAccess { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }

        [NotMapped]
        [JsonProperty("roles")]
        public ICollection<Role> Roles { get; set; }

        [NotMapped]
        [JsonProperty("resources")]
        public ICollection<Resource> Resources { get; set; }

        [NotMapped]
        [JsonProperty("resourceAccesses")]
        public ICollection<ResourceAccess> ResourceAccesses { get; set; }
    }
}