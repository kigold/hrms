using Auth.API.Data.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;

        public AuthorizationController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            IOpenIddictApplicationManager applicationManager,
            IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            ClaimsPrincipal claimsPrincipal;

            if (request.IsPasswordGrantType())
                return await TokensForPasswordGrantType(request);

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? "") ??
                    throw new InvalidOperationException("The application cannot be found.");

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

                // Use the client_id as the subject identifier.
                identity.AddClaim(new Claim(Claims.Subject,
                    await _applicationManager.GetClientIdAsync(application) ?? "",
                    Destinations.AccessToken, Destinations.IdentityToken));

                identity.AddClaim(new Claim(Claims.Name,
                    await _applicationManager.GetDisplayNameAsync(application) ?? "",
                    Destinations.AccessToken, Destinations.IdentityToken));

                claimsPrincipal = new ClaimsPrincipal(identity);
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal ?? new ClaimsPrincipal();
            }
            else if (request.IsRefreshTokenGrantType())
            {
                return await RefreshTokenGrantType();
            }
            else
            {
                throw new NotImplementedException("The specified grant is not implemented.");
            }

            //return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<IActionResult> RefreshTokenGrantType()
        {
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var user = await _userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            return await SignIn(user);
        }

        private async Task<IActionResult> TokensForPasswordGrantType(OpenIddictRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username ?? "");
            if (user == null)
                return Unauthorized();

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password ?? "", false);
            if (signInResult.Succeeded)
            {
                return await SignIn(user);
            }
            else
                return Unauthorized();
        }

        private async Task<IActionResult> SignIn(User? user)
        {
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            AddUserClaims(user, identity);
            // Add more claims if necessary

            foreach (var userRole in await _userManager.GetRolesAsync(user))
            {
                identity.AddClaim(OpenIddictConstants.Claims.Role, userRole, OpenIddictConstants.Destinations.AccessToken);

                //Add Permissions for Role
                //Improve this by reducing calls to the DB
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role == null)
                    continue;
                var roleClaims = (await _roleManager.GetClaimsAsync(role));
                foreach (var claim in roleClaims)
                {
                    if (claim == null)
                        continue;
                    identity.AddClaim("Permission", claim.Value, OpenIddictConstants.Destinations.AccessToken);
                }
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            claimsPrincipal.SetScopes(new string[]
            {
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    "Permission",
                    "api"
            });

            claimsPrincipal.SetDestinations(static claim => claim.Type switch
            {
                // If the "profile" scope was granted, allow the "name" claim to be
                // added to the access and identity tokens derived from the principal.
                Claims.Name when claim.Subject.HasScope(Scopes.Profile) => new[]
                {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },

                // Never add the "secret_value" claim to access or identity tokens.
                // In this case, it will only be added to authorization codes,
                // refresh tokens and user/device codes, that are always encrypted.
                "secret_value" => Array.Empty<string>(),

                // Otherwise, add the claim to the access tokens only.
                _ => new[]
                {
                        OpenIddictConstants.Destinations.AccessToken
                    }
            });

            //Set Resource
            claimsPrincipal.SetAudiences("asset_mgt", "auth", "employee");

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private static void AddUserClaims(User user, ClaimsIdentity identity)
        {
            identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
            identity.SetClaim(OpenIddictConstants.Claims.Username, user.UserName, OpenIddictConstants.Destinations.AccessToken);
            identity.SetClaim(OpenIddictConstants.Claims.Name, user.FullName, OpenIddictConstants.Destinations.AccessToken);
            identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email, OpenIddictConstants.Destinations.AccessToken);
            identity.SetClaim("avatar", user.Avatar ?? "", OpenIddictConstants.Destinations.AccessToken);
        }
    }
}
