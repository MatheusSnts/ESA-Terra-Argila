using Microsoft.AspNetCore.Identity;
using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Data
{
    public class Seeder
    {
        private static readonly string[] roleNames = { "Admin", "Vendor", "Supplier", "Customer" };
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            //TODO: Check in the future if a employee role is needed
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            foreach (var roleName in roleNames)
            {
                await AddTestUser(userManager, roleName);
            }
        }

        private static async Task AddTestUser(UserManager<User> userManager, string role)
        {
            string roleLower = role.ToLower();
            string roleCapitalized = char.ToUpper(roleLower[0]) + roleLower.Substring(1).ToLower();
            string email = $"{roleLower}@test.com";
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    FullName = $"{roleCapitalized} User",
                    EmailConfirmed = true,
                    AcceptedByAdmin = true

                };
                string password = $"{roleCapitalized}@123";
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, password);

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, roleCapitalized);
            }
        }

        
    }
}
