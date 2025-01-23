using Microsoft.AspNetCore.Identity;
using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Data
{
    public class Seeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            //TODO: Check in the future if a employee role is needed
            string[] roleNames = { "Admin", "Vendor", "Supplier", "Customer" };
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
            
            // Create Admin
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "Admin User",
                    EmailConfirmed = true

                };
                var hasher = new PasswordHasher<User>();
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        public static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Test Product 1",
                        Reference = "P1",
                        Description = "Description 1",
                        Price = 10
                    },
                    new Product
                    {
                        Name = "Test Product 2",
                        Reference = "P2",
                        Description = "Test Description 2",
                        Price = 20
                    },
                    new Product
                    {
                        Name = "Test Product 3",
                        Reference = "P3",
                        Description = "Test Description 3",
                        Price = 30
                    }
                };
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
