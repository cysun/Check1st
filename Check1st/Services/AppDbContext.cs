using Check1st.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Check1st.Services;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.File> Files { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Consultation> Consultations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Models.File>().HasOne(f => f.Content).WithOne().HasForeignKey<FileContent>();
        builder.Entity<Consultation>().HasMany(c => c.Files).WithMany().UsingEntity("ConsultationFiles"); ;
    }
}
