using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.DotAPI.Context.Entities
{
    [Table("refresh_token")]
    public class JwtRefreshToken
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("refresh_token")]
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [Column("username")]
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [Column("valid")]
        [JsonPropertyName("valid")]
        public DateTime ValidUntil { get; set; }
    }
}
