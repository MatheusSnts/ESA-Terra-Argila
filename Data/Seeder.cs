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
                    EmailConfirmed = true
                };
                string password = $"{roleCapitalized}@123";
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, password);

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, roleCapitalized);
            }
        }

        public static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (!context.Products.Any() && !context.Categories.Any() && !context.Materials.Any() && !context.Tags.Any())
            {

                var vendor = context.Users.FirstOrDefault(u => u.Email == "vendor@test.com");
                var supplier = context.Users.FirstOrDefault(u => u.Email == "supplier@test.com");

                var categories = new List<Category>
                {
                    new Category { Name = "Categoria Teste 1" },
                    new Category { Name = "Categoria Teste 2" },
                    new Category { Name = "Categoria Teste 3" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                var materials = new List<Material>
                {
                    new Material
                    {
                        Name = "Material Teste 1",
                        UserId = supplier.Id,
                        Reference = "M1",
                        Description = "Descrição Material Teste 1",
                        Price = 150.0f,
                        Unit = "un.",
                        CategoryId = categories[0].Id
                    },
                    new Material
                    {
                        Name = "Material Teste 2",
                        UserId = supplier.Id,
                        Reference = "M2",
                        Description = "Descrição Material Teste 2",
                        Price = 200.0f,
                        Unit = "kg",
                        CategoryId = categories[2].Id
                    },
                    new Material
                    {
                        Name = "Material Teste 3",
                        UserId = supplier.Id,
                        Reference = "M3",
                        Description = "Descrição Material Teste 3",
                        Price = 80.0f,
                        Unit = "cm",
                        CategoryId = categories[1].Id
                    }
                };
                await context.Materials.AddRangeAsync(materials);
                await context.SaveChangesAsync();

                var tags = new List<Tag>
                {
                    new Tag { Name = "Premium" },
                    new Tag { Name = "Sustentável" },
                    new Tag { Name = "Black-friday" },
                    new Tag { Name = "Promoção" }
                };
                await context.Tags.AddRangeAsync(tags);
                await context.SaveChangesAsync();

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Produto Teste 1",
                        UserId = vendor.Id,
                        Reference = "P1",
                        Description = "Descrição Produto Teste 1",
                        Price = 1299.99f,
                        Unit = "un.",
                        CategoryId = categories[0].Id,
                    },
                    new Product
                    {
                        Name = "Produto Teste 2",
                        UserId = vendor.Id,
                        Reference = "P2",
                        Description = "Descrição Produto Teste 1",
                        Price = 549.99f,
                        Unit = "kg",
                        CategoryId = categories[2].Id,
                    },
                    new Product
                    {
                        Name = "Produto Teste 3",
                        UserId = vendor.Id,
                        Reference = "P3",
                        Description = "Descrição Produto Teste 3",
                        Price = 349.99f,
                        Unit = "pack x3",
                        CategoryId = categories[1].Id,
                    }
                };
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                var productMaterials = new List<ProductMaterial>
                {
                    new ProductMaterial
                    {
                        ProductId = products[0].Id,
                        MaterialId = materials[0].Id
                    },
                    new ProductMaterial
                    {
                        ProductId = products[1].Id,
                        MaterialId = materials[1].Id
                    },
                    new ProductMaterial
                    {
                        ProductId = products[1].Id,
                        MaterialId = materials[2].Id
                    },
                    new ProductMaterial
                    {
                        ProductId = products[2].Id,
                        MaterialId = materials[2].Id
                    },
                    new ProductMaterial
                    {
                        ProductId = products[2].Id,
                        MaterialId = materials[0].Id
                    }
                };
                await context.ProductMaterials.AddRangeAsync(productMaterials);
                await context.SaveChangesAsync();


                var productTags = new List<ProductTag>
                {
                    new ProductTag
                    {
                        ProductId = products[0].Id,
                        TagId = tags[0].Id
                    },
                    new ProductTag
                    {
                        ProductId = products[0].Id,
                        TagId = tags[2].Id
                    },
                    new ProductTag
                    {
                        ProductId = products[1].Id,
                        TagId = tags[2].Id
                    },
                    new ProductTag
                    {
                        ProductId = products[1].Id,
                        TagId = tags[1].Id
                    },
                    new ProductTag
                    {
                        ProductId = products[2].Id,
                        TagId = tags[3].Id
                    }
                };
                await context.ProductTags.AddRangeAsync(productTags);

                var materialTags = new List<MaterialTag>
                {
                    new MaterialTag
                    {
                        MaterialId = materials[0].Id,
                        TagId = tags[0].Id
                    },
                    new MaterialTag
                    {
                        MaterialId = materials[0].Id,
                        TagId = tags[1].Id
                    },
                    new MaterialTag
                    {
                        MaterialId = materials[1].Id,
                        TagId = tags[0].Id
                    },
                    new MaterialTag
                    {
                        MaterialId = materials[2].Id,
                        TagId = tags[2].Id
                    }
                };
                await context.MaterialTags.AddRangeAsync(materialTags);

                await context.SaveChangesAsync();

                Console.WriteLine("Banco de dados populado com sucesso com dados de teste!");
            }
            else
            {
                Console.WriteLine("Banco de dados já contém dados. Seed não foi executado.");
            }
        }
    }
}
