using Microsoft.AspNetCore.Identity;

namespace Check1st.Models;

public class User : IdentityUser
{
    public bool IsAdmin { get; set; }
}
