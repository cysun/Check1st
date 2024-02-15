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
                case "b":
                    done = true;
                    break;
            }
        } while (!done);
    }

    private string UsersView()
    {
        var validChoices = new HashSet<string>() { "a", "b" };

        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("\t User Management \n");
            Console.WriteLine("\t a) Add a user");
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
}