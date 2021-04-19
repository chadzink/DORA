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
        [JsonProperty("display_name")]
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
        [JsonProperty("first_login_stamp")]
        public DateTime? FirstLoginStamp { get; set; }

        [Column("last_login_stamp")]
        [JsonProperty("last_login_stamp")]
        public DateTime? LastLoginStamp { get; set; }

        [Column("external_id")]
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [Column("archived_stamp")]
        [JsonProperty("archived_stamp")]
        public DateTime? ArchivedStamp { get; set; }

        [Column("user_password_id")]
        [JsonIgnore]
        public Guid CurrentUserPasswordId { get; set; }

        [Column("needs_password_change")]
        [JsonProperty("needs_password_change")]
        public bool NeedsPasswordChange { get; set; }

        [Column("enabled")]
        [JsonProperty("enabled")]
        public Int16 enabled { get; set; }

        [Column("requested_password_reset")]
        [JsonProperty("requested_password_reset")]
        public DateTime? RequestedPasswordReset { get; set; }

        [Column("password_reset_token")]
        [JsonProperty("password_reset_token")]
        public Guid? PasswordResetToken { get; set; }

        [Column("created_stamp")]
        [JsonProperty("created_stamp")]
        public DateTime? CreatedStamp { get; set; }

        [Column("last_updated_stamp")]
        [JsonProperty("last_updated_stamp")]
        public DateTime? LastUpdatedStamp { get; set; }

        [NotMapped]
        [JsonProperty("access_token")]
        public string JwtToken { get; set; }

        [NotMapped]
        [JsonProperty("token_exp")]
        public DateTime JwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonProperty("refresh_token")]
        public string RefreshJwtToken { get; set; }

        [NotMapped]
        [JsonProperty("refresh_token_exp")]
        public DateTime RefreshJwtTokenExpiresOn { get; set; }

        [NotMapped]
        [JsonProperty("user_roles")]
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
