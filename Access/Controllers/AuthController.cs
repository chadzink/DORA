using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DORA.Access.Context;
using DORA.Access.Context.Entities;
using DORA.Access.Context.Repositories;
using DORA.Access.Services;
using DORA.Access.Helpers;
using DORA.Access.Models.Controllers.Auth;
using DORA.Notify;
using Microsoft.AspNetCore.Http;

namespace Access.Controllers
{
    /// <summary>
    ///     Controller used for registering new users, creating authentication tokens used to access the system, fetching current user, refreshing tokens, and revoking tokens.
    /// </summary>
    [ApiController]
    [Authorize, Route("/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AccessContext _accessContext;
        private readonly IUserService _userService;
        private readonly UserRepository _userRepository;
        private readonly JwtRefreshTokenRepository _jwtRefreshTokenRepository;
        private IConfiguration _configuration;

        public IConfiguration Config
        {
            get
            {
                return this._configuration;
            }
        }

        public AuthController(
            AccessContext accessContext,
            IConfiguration config
        )
        {
            _accessContext = accessContext;
            _configuration = config;

            _userRepository = new UserRepository(accessContext, config);
            _jwtRefreshTokenRepository = new JwtRefreshTokenRepository(accessContext, config);

            _userService = new UserService(
                accessContext,
                _userRepository,
                _jwtRefreshTokenRepository,
                config
            );
        }

        /// <summary>
        ///     Accepts new user data and creates a new user in the system.
        /// </summary>
        /// <remarks>
        ///     Using this to register a user will only create a user that can login. No Resources, Roles, or other authorization will be done.
        /// </remarks>
        /// <param name="newUser">Must include, username, password, retypePassword, email, firstname, lastname, and phonenumber</param>
        /// <response code="200">JSON object with success = true, message = Logged In, and user = full user object with auth tokens</response>
        /// <response code="400">JSON object with success = false, message for error (passwords do not match, or register failed)</response>
        [AllowAnonymous, HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ApiRegisterNewUser([FromBody] NewUserModel newUser)
        {
            if (newUser.password != newUser.retypePassword)
            {
                JsonDataError JsonErr = new JsonDataError("Password provided does not match");
                return BadRequest(JsonErr.Serialize());
            }

            if (string.IsNullOrEmpty(newUser.email))
            {
                JsonDataError JsonErr = new JsonDataError("Must provide a unique user email to register.");
                return BadRequest(JsonErr.Serialize());
            }

            User user = _userService.NewUser(newUser.username, newUser.password, newUser.email, newUser.firstname, newUser.lastname, newUser.phonenumber);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("Failed to register new user");
                return BadRequest(JsonErr.Serialize());
            }

            // Need to check if the current user can create USER-ROLES
            UserRoleRepository userRoleRepository = new UserRoleRepository(_accessContext, this.Config);

            if (userRoleRepository.CreateAccess("USER-ROLE", User.Claims.Where(c => c.Type == ClaimTypes.Role)))
            {
                // Add roles requested to user
                RoleRepository roleRepository = new RoleRepository(_accessContext, this.Config);

                IQueryable<Role> onlyUserRoles = roleRepository.FindBy(r => newUser.RolesRequested.Contains(r.KeyCode));

                if (onlyUserRoles.Count() > 0)
                {
                    foreach (Role role in onlyUserRoles)
                    {
                        // now we know what user and what roles the user wants comapred to what roles the logged in user has access to
                        userRoleRepository.Create(new UserRole() {
                            Id = Guid.NewGuid(),
                            UserId = user.Id.Value,
                            RoleId = role.Id.Value,
                        });
                    }
                }
            }

            // create a common project JSON response object
            JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Logged In");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Primary login for user to to generate user model with tokens.
        /// </summary>
        /// <remarks>
        ///     Authenticates the user and generates a token and refresh token if valid.
        /// </remarks>
        /// <param name="model">Auth Model, must contain the information needed to login. Username and Password</param>
        /// <response code="200">JSON object with user with updated JWT and refresh tokens.</response>
        /// <response code="308">Redirect to Save Token URL in JWT Client Record with token and refreshToken querystring params.</response>
        /// <response code="400">JSON object with success = false and message containing error type.</response>
        [AllowAnonymous, HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        public IActionResult ApiLogin([FromBody] AuthModel model)
        {
            User user = _userService.Authenticate(model.username, model.password);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("Username or password is incorrect");
                return BadRequest(JsonErr.Serialize());
            }

            JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Logged In");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Primary Change Password Route
        /// </summary>
        /// <remarks>
        ///     Allows the currently logged in user to change the user password to a new value
        /// </remarks>
        /// <param name="model">Auth Change Model, must contain username, old password, and new password</param>
        /// <response code="200">JSON object with user with updated JWT and refresh tokens.</response>
        /// <response code="308">Redirect to Save Token URL in JWT Client Record with token and refreshToken querystring params.</response>
        /// <response code="400">JSON object with success = false and message containing error type.</response>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        public IActionResult ApiChangePassword([FromBody] AuthChangeModel model)
        {
            // only the current user can change their own password
            _userRepository.SetUser(this.User);
            User curUser = _userRepository.CurrentUser();

            if (curUser.UserName != model.username)
            {
                JsonDataError JsonErr = new JsonDataError(string.Format("Must be logged in as {0} to change password.", model.username));
                return BadRequest(JsonErr.Serialize());
            }

            try
            {
                User user = _userService.ChangePassword(model.username, model.oldPassword, model.newPassword);

                if (user == null)
                {
                    JsonDataError JsonErr = new JsonDataError("Unable to assign new password to user");
                    return BadRequest(JsonErr.Serialize());
                }

                JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Logged In");
                return Ok(JsonObj.Serialize());
            }
            catch (Exception e)
            {
                JsonDataError JsonErr = new JsonDataError(e.Message);
                return BadRequest(JsonErr.Serialize());
            }
        }

        /// <summary>
        ///     Request Password Reset
        /// </summary>
        /// <remarks>
        ///     Allows a user to provide username or email and request that their user be allowed too change their password.
        /// </remarks>
        /// <response code="200">JSON object with user with updated JWT and refresh tokens.</response>
        /// <response code="308">Redirect to Save Token URL in JWT Client Record with token and refreshToken querystring params.</response>
        /// <response code="400">JSON object with success = false and message containing error type.</response>
        [AllowAnonymous, HttpGet("request-rest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        public IActionResult ApiRequestUserReset([FromQuery] string email)
        {
            // check that user exist
            User user = _userRepository.FindOneBy(r => r.Email == email && r.enabled != 0);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("No user found with email address provided.");
                return BadRequest(JsonErr.Serialize());
            }

            // Set the User RequestedPasswordReset Datetime & PasswordResetToken
            user = _userService.SerResetRequested(user);

            if (user == null || !user.PasswordResetToken.HasValue)
            {
                JsonDataError JsonErr = new JsonDataError("Unable to assign password reset token for user.");
                return BadRequest(JsonErr.Serialize());
            }

            // load the email template to use
            string templateType = this.Config["AuthConfig:ResetUserEMailTemplateType"];
            string templatebody = null;

            if (templateType == null)
            {
                JsonDataError JsonErr = new JsonDataError("Password reset template not configured. Use AuthConfig.ResetUserEMailTemplateType in app config with value of [config] or [file].");
                return BadRequest(JsonErr.Serialize());
            }
            else if (templateType == "config")
            {
                templatebody = this.Config["AuthConfig:ResetUserEMailHTMLTemplate"];

                if (string.IsNullOrEmpty(templatebody))
                {
                    JsonDataError JsonErr = new JsonDataError("Password reset template not configured. Use AuthConfig.ResetUserEMailHTMLTemplate needs to have a string template setup.");
                    return BadRequest(JsonErr.Serialize());
                }
            }
            else if (templateType == "file")
            {
                string templateFilePath = this.Config["AuthConfig:ResetUserEMailHTMLTemplate"];
                if (string.IsNullOrEmpty(templateFilePath))
                {
                    JsonDataError JsonErr = new JsonDataError("Password reset template not configured. Use AuthConfig.ResetUserEMailHTMLTemplate needs to have a string template setup.");
                    return BadRequest(JsonErr.Serialize());
                }

                FileInfo templateFileInfo = new FileInfo(templateFilePath);

                if (!templateFileInfo.Exists)
                {
                    JsonDataError JsonErr = new JsonDataError(string.Format("The file path in AuthConfig.ResetUserEMailHTMLTemplate [{0}] was not found.", templateFilePath));
                    return BadRequest(JsonErr.Serialize());
                }

                templatebody = System.IO.File.ReadAllText(templateFileInfo.FullName);
            }

            // Swap token in template body
            templatebody = templatebody.Replace("[@ResetToken]", user.PasswordResetToken.Value.ToString());

            // Swap in front end URL for handling reset request, to build a response with new password form
            string resetRequestTokenPage = this.Config["AuthConfig:ResetTokenPage"];

            if (string.IsNullOrEmpty(resetRequestTokenPage))
            {
                JsonDataError JsonErr = new JsonDataError("Missing token request form url in configuration with AuthConfig:ResetTokenPage.");
                return BadRequest(JsonErr.Serialize());
            }

            templatebody = templatebody.Replace("[@ResetTokenPage]", resetRequestTokenPage);

            // Send user an email with the token and a link to the resolve page with the token in the URL
            string fromAddress = this.Config["AuthConfig:ResetTokenFromEMailAddress"];

            if (string.IsNullOrEmpty(fromAddress))
            {
                JsonDataError JsonErr = new JsonDataError("Missing email to send from in configuration with AuthConfig:ResetTokenFromEMailAddress.");
                return BadRequest(JsonErr.Serialize());
            }

            EMailSender eMailSender = new EMailSender(fromAddress, this.Config);
            string sendError = null;

            if (!eMailSender.MessageTo(email, "Reset Password REquest", templatebody, out sendError)) {
                JsonDataError JsonErr = new JsonDataError(sendError);
                return BadRequest(JsonErr.Serialize());
            }

            // Finished
            JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Message Sent");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Resolve Password Reset
        /// </summary>
        /// <remarks>
        ///     Allows a user to provide a reset token and a new password
        /// </remarks>
        /// <response code="200">JSON object with user with updated JWT and refresh tokens.</response>
        /// <response code="308">Redirect to Save Token URL in JWT Client Record with token and refreshToken querystring params.</response>
        /// <response code="400">JSON object with success = false and message containing error type.</response>
        [AllowAnonymous, HttpGet("resolve-rest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        public IActionResult ApiResolveUserReset([FromBody] AuthResetResolveModel model, [FromQuery] string token)
        {
            Guid tokenAsGuid = Guid.Parse(token);

            // check that the new and retype passwords match
            if (model.newPassword != model.retypeNewPassword)
            {
                JsonDataError JsonErr = new JsonDataError("Password provided does not match");
                return BadRequest(JsonErr.Serialize());
            }

            // check that the User for username exist with
            // 1. A RequestedPasswordReset that is not older than 3 days
            // 2. A PasswordResetToken that matches teh one provided
            // 3. The user in enabled

            User user = _userRepository.FindOneBy(r =>
                r.PasswordResetToken == tokenAsGuid
                && r.RequestedPasswordReset >= DateTime.Now.AddDays(-3)
                && r.enabled != 0
            );

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("Invalid token or expired reset request. Request cannot be older than 3 days.");
                return BadRequest(JsonErr.Serialize());
            }

            // Set the users password to the new password
            user = _userService.CompletePasswordReset(user, tokenAsGuid, model.newPassword);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("Unable to assign new password to user");
                return BadRequest(JsonErr.Serialize());
            }

            // send back a user with valid token
            JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Success");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Gets the current user based on the token in the AUthentication header.
        /// </summary>
        /// <remarks>
        ///     User data is encoded in the JWT token, but this can be used to verify user and updatethe JWT token.
        /// </remarks>
        /// <returns>JSON with user and updated tokens</returns>
        /// <response code="404">User record not found</response>
        /// <response code="200">JSON object for current user with updated JWT token</response>
        [HttpGet("current-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UserByToken()
        {
            _userRepository.SetUser(this.User);
            User user = _userRepository.CurrentUser();

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("The user record couldn't be found.", 404);
                return NotFound(JsonErr.Serialize());
            }

            JsonData<User> JsonObj = new JsonData<User>(user, user.JwtToken, user.RefreshJwtToken, true, "Success");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Used to refresh a JWT token using expired JWT token and valid refresh token
        /// </summary>
        /// <remarks>
        ///     Must send AccessToken (JWT) and RefreshToken string values. Expired JWT is decoded for username that must match the refresh token record.
        /// </remarks>
        /// <param name="tokenApiModel">TokenApi Model, must contain AccessToken and RefreshToken string values</param>
        /// <response code="200">JSON object for user identified with expired JWT and matches refresh token. USer contains new valid JWT and refesh token data</response>
        /// <response code="400">String Invalid client request when invalid data passed or refresh token is invalid</response>
        [AllowAnonymous, HttpPost("refresh-tokens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RefreshWithTokens([FromBody] TokenApiModel tokenApiModel)
        {
            if (tokenApiModel is null)
            {
                JsonDataError JsonErr = new JsonDataError("Invalid client request");
                return NotFound(JsonErr.Serialize());
            }
            string accessToken = tokenApiModel.AccessToken;
            string refreshToken = tokenApiModel.RefreshToken;
            ClaimsPrincipal principal = _userService.GetPrincipalFromExpiredToken(accessToken);

            string username = principal.Identity.Name; //this is mapped to the Name claim by default
            JwtRefreshToken refToken = _jwtRefreshTokenRepository.FindOneBy(r => r.UserName == username && r.RefreshToken == refreshToken);

            if (refToken == null || refToken.ValidUntil <= DateTime.Now)
            {
                JsonDataError JsonErr = new JsonDataError("Invalid client request");
                return NotFound(JsonErr.Serialize());
            }

            User user = _userRepository.FindOneBy(r => r.UserName == username);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("User record not found");
                return NotFound(JsonErr.Serialize());
            }

            User userWithToken = _userService.UserWithToken(user);

            JsonData<User> JsonObj = new JsonData<User>(user, userWithToken.JwtToken, userWithToken.RefreshJwtToken, true, "Success");
            return Ok(JsonObj.Serialize());
        }

        /// <summary>
        ///     Used to revoke the current users refresh tokens.
        /// </summary>
        /// <remarks>
        ///     The purpose of this is to logout a user and cleanup refresh token(s)
        /// </remarks>
        /// <response code="204">No Content</response>
        /// <response code="400">String No user found</response>
        [HttpPost("revoke-tokens")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Revoke()
        {
            _jwtRefreshTokenRepository.SetUser(this.User);
            User user = _jwtRefreshTokenRepository.CurrentUser();

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("No user found");
                return BadRequest(JsonErr.Serialize());
            }

            // cleanup old refresh tokens for user that have expired
            IQueryable<JwtRefreshToken> userRefreshTokens = _jwtRefreshTokenRepository.FindBy(r => r.UserName == user.UserName);
            _jwtRefreshTokenRepository.Delete(userRefreshTokens.ToArray());

            return NoContent();
        }
    }
}
