using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DORA.Access.Context.Entities
{
    [Table("access_users")]
    public class User
    {
        [Key]
        [Column("id")]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Column("username")]
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [Column("display_name")]
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [Column("first")]
        [JsonPropertyName("first")]
        public string FirstName { get; set; }

        [Column("last")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [Column("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Column("phone")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [Column("first_login_stamp")]
        [JsonPropertyName("first_login_stamp")]
        public DateTime? FirstLoginStamp { get; set; }

        [Column("last_login_stamp")]
        [JsonPropertyName("last_login_stamp")]
        public DateTime? LastLoginStamp { get; set; }

        [Column("external_id")]
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [Column("archived_stamp")]
        [JsonPropertyName("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [Column("user_password_id")]
        [JsonIgnore]
        public Guid CurrentUserPasswordId { get; set; }

        [Column("needs_password_change")]
        [JsonPropertyName("needs_password_change")]
        public bool NeedsPasswordChange { get; set; }

        [Column("enabled")]
        [JsonPropertyName("enabled")]
        public Int16 enabled { get; set; }

        [Column("requested_password_reset")]
        [JsonPropertyName("requested_password_reset")]
        public DateTime? RequestedPasswordReset { get; set; }

        [Column("password_reset_token")]
        [JsonPropertyName("password_reset_token")]
        public Guid? PasswordResetToken { get; set; }

        [Column("created_stamp")]
        [JsonPropertyName("created_stamp")]
        public DateTime? CreatedStamp { get; set; }

        [Column("last_updated_stamp")]
        [JsonPropertyName("last_updated_stamp")]
        public DateTime? LastUpdatedStamp { get; set; }

        [NotMapped]
        [JsonPropertyName("access_token")]
        public string JwtToken { get; set; }

        [NotMapped]
        [JsonPropertyName("token_exp")]
        public DateTime JwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonPropertyName("refresh_token")]
        public string RefreshJwtToken { get; set; }

        [NotMapped]
        [JsonPropertyName("refresh_token_exp")]
        public DateTime RefreshJwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
