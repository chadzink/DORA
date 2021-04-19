using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Access.Context.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("username")]
        [JsonProperty("username")]
        public string UserName { get; set; }

        [Column("display_name")]
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [Column("first")]
        [JsonProperty("first")]
        public string FirstName { get; set; }

        [Column("last")]
        [JsonProperty("last")]
        public string LastName { get; set; }

        [Column("email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Column("phone")]
        [JsonProperty("phone")]
        public string Phone { get; set; }

        [Column("first_login_stamp")]
        [JsonProperty("firstLoginStamp")]
        public DateTime? FirstLoginStamp { get; set; }

        [Column("last_login_stamp")]
        [JsonProperty("lastLoginStamp")]
        public DateTime? LastLoginStamp { get; set; }

        [Column("external_id")]
        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [Column("archived_stamp")]
        [JsonIgnore]
        public DateTime? ArchivedStamp { get; set; }

        [Column("user_password_id")]
        [JsonIgnore]
        public Guid CurrentUserPasswordId { get; set; }

        [Column("needs_password_change")]
        [JsonIgnore]
        public bool NeedsPasswordChange { get; set; }

        [Column("enabled")]
        [JsonProperty("enabled")]
        public Int16 enabled { get; set; }

        [Column("requested_password_reset")]
        [JsonProperty("requestedPasswordReset")]
        public DateTime? RequestedPasswordReset { get; set; }

        [Column("password_reset_token")]
        [JsonProperty("passwordResetToken")]
        public Guid? PasswordResetToken { get; set; }

        [Column("created_stamp")]
        [JsonProperty("createdStamp")]
        public DateTime? CreatedStamp { get; set; }

        [Column("last_updated_stamp")]
        [JsonProperty("lastUpdatedStamp")]
        public DateTime? LastUpdatedStamp { get; set; }

        [NotMapped]
        [JsonProperty("accessToken")]
        public string JwtToken { get; set; }

        [NotMapped]
        [JsonProperty("tokenExp")]
        public DateTime JwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonProperty("refreshToken")]
        public string RefreshJwtToken { get; set; }

        [NotMapped]
        [JsonProperty("refreshTokenExp")]
        public DateTime RefreshJwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
