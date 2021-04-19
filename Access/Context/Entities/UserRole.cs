using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("user_roles")]
    public class UserRole
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("role_id")]
        [JsonProperty("roleId")]
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [JsonIgnore]
        public Role Role { get; set; }

        [Column("user_id")]
        [JsonProperty("userId")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }
    }
}
