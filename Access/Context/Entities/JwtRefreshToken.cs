using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("refresh_token")]
    public class JwtRefreshToken
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("refresh_token")]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [Column("username")]
        [JsonProperty("username")]
        public string UserName { get; set; }

        [Column("valid")]
        [JsonProperty("valid")]
        public DateTime ValidUntil { get; set; }
    }
}
