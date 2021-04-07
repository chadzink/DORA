using System;
using System.ComponentModel.DataAnnotations;

namespace DORA.DotAPI.Models.Controllers.Auth
{
    public class NewUserModel
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string retypePassword { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string firstname { get; set; }

        [Required]
        public string lastname { get; set; }

        [Required]
        public string phonenumber { get; set; }

        public string[] RolesRequested { get; set; }
    }

    public class AuthModel
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }
    }

    public class TokenApiModel
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
