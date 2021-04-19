using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("user_roles")]
    public class UserRole
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("role_id")]
        [JsonPropertyName("role_id")]
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [JsonIgnore]
        public Role Role { get; set; }

        [Column("user_id")]
        [JsonPropertyName("user_id")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }
    }
}
