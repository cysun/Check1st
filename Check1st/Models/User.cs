using Microsoft.AspNetCore.Identity;

namespace Check1st.Models;

public class User : IdentityUser
{
    public DateTime? ExpirationDate { get; set; }

    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
}
