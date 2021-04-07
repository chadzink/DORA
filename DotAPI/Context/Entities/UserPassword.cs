using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.DotAPI.Context.Entities
{
    [Table("access_user_passwords")]
    public class UserPassword
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; }

        [Column("user_id")]
        [JsonPropertyName("user_id")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [Column("created_stamp")]
        [JsonPropertyName("created_stamp")]
        public DateTime? CreatedStamp { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }
    }
}
