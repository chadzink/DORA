using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("user_passwords")]
    public class UserPassword
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; }

        [Column("user_id")]
        [JsonProperty("user_id")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [Column("created_stamp")]
        [JsonProperty("created_stamp")]
        public DateTime? CreatedStamp { get; set; }

        [Column("archived_stamp")]
        [JsonProperty("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }
    }
}
