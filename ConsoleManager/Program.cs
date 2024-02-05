// See https://aka.ms/new-console-template for more information
using Check1st.Security;
using Check1st.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

await (new ConsoleManager()).MainControllerAsync();

partial class ConsoleManager
{
    readonly IConfiguration configuration;
    readonly ServiceProvider serviceProvider;

    UserManager<IdentityUser> userManager => serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    RoleManager<IdentityRole> roleManager => serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    public ConsoleManager()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Check1st"))
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        services.AddOptions().AddLogging();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication();

        serviceProvider = services.BuildServiceProvider();
    }

    public async Task MainControllerAsync()
    {
        // Create roles if they don't exist
        foreach (var role in Enum.GetValues<Constants.Role>())
        {
            if (!await roleManager.RoleExistsAsync(role.ToString()))
                await roleManager.CreateAsync(new IdentityRole(role.ToString()));
        }

        var done = false;
        do
        {
            var cmd = MainView();
            switch (cmd)
            {
                case "u":
                    await UsersControllerAsync();
                    break;
                case "x":
                    done = true;
                    break;
            }
        } while (!done);

        serviceProvider.Dispose();
    }

    public string MainView()
    {
        var validChoices = new HashSet<string>() { "u", "x" };
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("\t Main Menu \n");
            Console.WriteLine("\t u) User Management");
            Console.WriteLine("\t x) Exit");
            Console.Write("\n  Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLowerInvariant();
        } while (!validChoices.Contains(choice));

        return choice;
    }

}