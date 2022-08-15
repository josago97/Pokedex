using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Pokedex.WebAPI.Middlewares
{
    public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static readonly string HEADER_KEY = HeaderNames.Authorization;
        private static readonly string TOKEN = Environment.GetEnvironmentVariable("AUTH_TOKEN");

        public AuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticateResult result;

            if (!Request.Headers.ContainsKey(HEADER_KEY))
            {
                result = AuthenticateResult.Fail("Header Not Found.");
            }
            else
            {
                string authorizationToken = Request.Headers[HEADER_KEY];

                if (authorizationToken == TOKEN)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authorizationToken),
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new System.Security.Principal.GenericPrincipal(identity, null);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    result = AuthenticateResult.Success(ticket);
                }
                else
                {
                    result = AuthenticateResult.Fail($"Unauthorized token: {authorizationToken}, request: {Request.Path.ToUriComponent()}");
                }
            }

            return Task.FromResult(result);
        }
    }
}
