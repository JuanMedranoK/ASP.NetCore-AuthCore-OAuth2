using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace AuthCore.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        [HttpPost("~/connect/token")]
        public IActionResult Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request is null)
            {
                throw new InvalidOperationException(
                    "The OpenID Connect request cannot be retrieved.");
            }

            if (request.IsClientCredentialsGrantType())
            {
                var identity = new ClaimsIdentity(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            }

            throw new InvalidOperationException(
                "The specified grant type is not supported.");
        }
    }
}
