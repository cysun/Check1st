using Check1st.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Check1st.Services;

public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
{
    public AppUserClaimsPrincipalFactory(UserManager<User> userManager,
        IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        // Claims in AspNetUserClaims are added here.
        var identity = await base.GenerateClaimsAsync(user);

        // Add the claims based on User properties. 
        if (user.IsAdmin)
            identity.AddClaim(new Claim("Admin", "Admin"));

        return identity;
    }
}