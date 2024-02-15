using Microsoft.EntityFrameworkCore;

namespace Check1st.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

}
