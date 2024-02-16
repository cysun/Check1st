using Check1st.Models;
using Check1st.Security;

partial class ConsoleManager
{
    private async Task UsersControllerAsync()
    {
        var done = false;
        do
        {
            var cmd = UsersView();
            switch (cmd)
            {
                case "a":
                    await AddUserAsync();
                    break;
                case "i":
                    await ImportUsersAsync();
                    break;
                case "b":
                    done = true;
                    break;
            }
        } while (!done);
    }

    private string UsersView()
    {
        var validChoices = new HashSet<string>() { "a", "i", "b" };

        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("\t User Management \n");
            Console.WriteLine("\t a) Add a user");
            Console.WriteLine("\t i) Import users");
            Console.WriteLine("\t b) Back to Main Menu\n");
            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLowerInvariant();
        } while (!validChoices.Contains(choice));
        return choice;
    }

    private async Task AddUserAsync()
    {
        Console.Clear();
        Console.WriteLine("\t Add User \n");
        Console.Write("\t Username: ");
        var username = Console.ReadLine();
        Console.Write("\t Password: ");
        var password = Console.ReadLine();
        Console.Write("\t Is Teacher? [y|n]: ");
        var isTeacher = Console.ReadLine().ToLowerInvariant() == "y";
        Console.Write("\t Admin? [y|n]: ");
        var isAdmin = Console.ReadLine().ToLowerInvariant() == "y";
        Console.Write("\t Save or Cancel? [s|c] ");
        var cmd = Console.ReadLine();
        if (cmd.ToLower() == "s")
        {
            var user = new User
            {
                UserName = username,
                Email = $"{username}@localhost"
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                Console.WriteLine("\n\t Failed to create the user");
                foreach (var error in result.Errors)
                    Console.WriteLine($"\t {error.Description}");
                Console.Write("\n\n\t Press [Enter] key to continue");
                Console.ReadLine();
            }
            else
            {
                if (isTeacher)
                {
                    await userManager.AddToRoleAsync(user, Constants.Role.Teacher.ToString());
                }
                if (isAdmin)
                {
                    await userManager.AddToRoleAsync(user, Constants.Role.Admin.ToString());
                }
            }
        }
    }

    private async Task ImportUsersAsync()
    {
        Console.Clear();
        Console.WriteLine("\t Import Users \n");
        Console.Write("\t Path to accounts file: ");
        var file = Console.ReadLine();
        if (!System.IO.File.Exists(file))
        {
            Console.WriteLine("\n\t File not found. Press [Enter] key to continue.");
            Console.ReadLine();
            return;
        }
        Console.Write("\t Accounts expire in days [100]: ");
        var input = Console.ReadLine();
        var expireInDays = input.Length > 0 ? int.Parse(input) : 100;

        var count = 0;
        var lines = System.IO.File.ReadAllLines(file);
        foreach (var line in lines)
        {
            var credentials = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (credentials.Length == 2)
            {
                var username = credentials[0];
                var password = credentials[1];
                var user = new User
                {
                    UserName = username,
                    Email = $"{username}@localhost",
                    ExpirationDate = DateTime.UtcNow.AddDays(expireInDays)
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    Console.WriteLine($"\n\t Failed to create the user {username}");
                    foreach (var error in result.Errors)
                        Console.WriteLine($"\t {error.Description}");
                }
                else
                {
                    ++count;
                }
            }
        }
        Console.WriteLine($"\n\n\t {count} users created. Press [Enter] key to continue.");
        Console.ReadLine();
    }
}