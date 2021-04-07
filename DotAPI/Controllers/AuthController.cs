using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Context.Repositories;
using DORA.DotAPI.Services;
using DORA.DotAPI.Helpers;
using DORA.DotAPI.Models.Controllers.Auth;
using Microsoft.AspNetCore.Http;

namespace DotAPI.Controllers
{
    /// <summary>
    ///     Controller used for registering new users, creating authentication tokens used to access the system, fetching current user, refreshing tokens, and revoking tokens.
    /// </summary>
    [ApiController]
    [Authorize, Route("/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AccessContext _accessContext;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly UserRepository _userRepository;
        private readonly JwtRefreshTokenRepository _jwtRefreshTokenRepository;
        private readonly RoleResourceAccessRepository _roleResourceAccessRepository;

        public AuthController(
            AccessContext accessContext,
            IConfiguration configuration
        )
        {
            _accessContext = accessContext;
            _configuration = configuration;

            _userRepository = new UserRepository(accessContext, configuration);
            _jwtRefreshTokenRepository = new JwtRefreshTokenRepository(accessContext, configuration);
            _roleResourceAccessRepository = new RoleResourceAccessRepository(accessContext, configuration);

            _userService = new UserService(
                accessContext,
                _userRepository,
                _jwtRefreshTokenRepository,
                _roleResourceAccessRepository,
                configuration
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

            User user = _userService.NewUser(newUser.username, newUser.password, newUser.email, newUser.firstname, newUser.lastname, newUser.phonenumber);

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("Failed to register new user");
                return BadRequest(JsonErr.Serialize());
            }

            // Need to check if the current user can create USER-ROLES
            UserRoleRepository userRoleRepository = new UserRoleRepository(_accessContext, _configuration);

            if (userRoleRepository.CreateAccess("USER-ROLE", User.Claims.Where(c => c.Type == ClaimTypes.Role)))
            {
                // Add roles requested to user
                RoleRepository roleRepository = new RoleRepository(_accessContext, _configuration);

                IQueryable<Role> onlyUserRoles = roleRepository.FindBy(new Func<Role, bool>[1] { (r => newUser.RolesRequested.Contains(r.NameCanonical)) });

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
        /// <param name="client_id">(optional) String that matches the JwtClient Id in the database.</param>
        /// <response code="200">JSON object with user with updated JWT and refresh tokens. When client_id not specified or not matched.</response>
        /// <response code="308">Redirect to Save Token URL in JWT Client Record with token and refreshToken querystring params.</response>
        /// <response code="400">JSON object with success = false and message containing error type.</response>
        [AllowAnonymous, HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        public IActionResult ApiLogin([FromBody] AuthModel model, [FromQuery] string client_id)
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

        // change password route
        // request password reset route
        // resolve reset password route

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
            _userRepository.SetUser(this.User);
            User user = _userRepository.CurrentUser();

            if (user == null)
            {
                JsonDataError JsonErr = new JsonDataError("No user found");
                return BadRequest(JsonErr.Serialize());
            }

            // cleanup old refresh tokens for user that have expired
            IQueryable<JwtRefreshToken> userRefreshTokens = _jwtRefreshTokenRepository.FindBy(new Func<JwtRefreshToken, bool>[1] {
                (r => r.UserName == user.UserName)
            });

            foreach (JwtRefreshToken expiredRefreshToken in userRefreshTokens)
            {
                _jwtRefreshTokenRepository.Delete(expiredRefreshToken);
            }

            return NoContent();
        }
    }
}
