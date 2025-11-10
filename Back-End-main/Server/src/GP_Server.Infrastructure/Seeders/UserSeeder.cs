using GP_Server.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GP_Server.Infrastructure.Seeders;

public static class UserSeeder
{
    private static async Task CreateAndAssignRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role, string password)
    {
        var existingUser = await userManager.FindByEmailAsync(user.Email);

        // Check if the user already exists
        if (existingUser == null)
        {
            // Initialize properties required by PostgreSQL
            user.ProfilePicturePath = string.Empty;  // Required property initialization
            user.Gender = "Unspecified";  // Required property initialization
            
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new Exception($"Failed to create user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Optionally log that the user already exists
            Console.WriteLine($"User {user.Email} already exists.");
        }
    }
    
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

        // Add SuperAdmin
        var superAdmin = new ApplicationUser
        {
            UserName = "superadmin@example.com",
            Email = "superadmin@example.com",
            FirstName = "Super",
            LastName = "Admin",
            PhoneNumber = "1234767890",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            ProfilePicturePath = string.Empty,
            Gender = "Unspecified"
        };
        await CreateAndAssignRoleAsync(userManager, superAdmin, "SuperAdmin", "Test123@");

        // Add Admins
        var admins = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "murad@example.com",
                    Email = "murad@example.com",
                    FirstName = "Murad",
                    LastName = "Admin",
                    PhoneNumber = "1234557890",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    ProfilePicturePath = string.Empty,
                    Gender = "Unspecified"
                },
                new ApplicationUser
                {
                    UserName = "ziad@example.com",
                    Email = "ziad@example.com",
                    FirstName = "Ziad",
                    LastName = "Admin",
                    PhoneNumber = "1234557891",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    ProfilePicturePath = string.Empty,
                    Gender = "Unspecified"
                },
                new ApplicationUser
                {
                    UserName = "mariam@example.com",
                    Email = "mariam@example.com",
                    FirstName = "Mariam",
                    LastName = "Admin",
                    PhoneNumber = "1234557891",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    ProfilePicturePath = string.Empty,
                    Gender = "Unspecified"
                },
                new ApplicationUser
                {
                    UserName = "youssef@example.com",
                    Email = "youssef@example.com",
                    FirstName = "Youssef",
                    LastName = "Admin",
                    PhoneNumber = "1234557891",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    ProfilePicturePath = string.Empty,
                    Gender = "Unspecified"
                }
            };
        foreach (var admin in admins)
        {
            await CreateAndAssignRoleAsync(userManager, admin, "Admin", "Test123@");
        }
    }
}

