using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using carniceriaApp.Models;

namespace carniceriaApp
{
    public class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario, IdentityRole>
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            if (!string.IsNullOrEmpty(user.NombreCompleto))
            {
                identity.AddClaim(new Claim("NombreCompleto", user.NombreCompleto));
            }
            return identity;
        }
    }
}