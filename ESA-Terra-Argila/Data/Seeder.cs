using Microsoft.AspNetCore.Identity;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Helpers;

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
                string password = $"{roleCapitalized}@1-.2/3.-6";
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, password);

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, roleCapitalized);
            }
        }

        public static async Task SeedItems(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment env)
        {
            if (context.Categories.Any() || context.Tags.Any() || context.Items.Any())
                return;

            var supplier = await userManager.FindByEmailAsync("supplier@test.com");
            var vendor = await userManager.FindByEmailAsync("vendor@test.com");

            // TAGS
            var tag1 = new Tag { Name = "Durável", Reference = "TAG001", IsPublic = true, UserId = supplier.Id };
            var tag2 = new Tag { Name = "Ecológico", Reference = "TAG002", IsPublic = true, UserId = vendor.Id };
            context.Tags.AddRange(tag1, tag2);

            // CATEGORIAS
            var cat1 = new Category { Name = "Madeiras", Reference = "CAT001", UserId = supplier.Id };
            var cat2 = new Category { Name = "Ferramentas", Reference = "CAT002", UserId = vendor.Id };
            context.Categories.AddRange(cat1, cat2);

            await context.SaveChangesAsync();

            // MATERIAL 1
            var mat1 = new Material
            {
                Name = "Tábuas de Pinho",
                Reference = "MAT001",
                Description = "Madeira leve e resistente ideal para construções rústicas.",
                Price = 12.5f,
                Stock = 50,
                Unit = "m²",
                CategoryId = cat1.Id,
                UserId = supplier.Id,
                IsSustainable = true,
                Tags = new List<Tag> { tag1 }
            };
            context.Items.Add(mat1);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, mat1.Id, "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg");

            // MATERIAL 2
            var mat2 = new Material
            {
                Name = "Argila Vermelha",
                Reference = "MAT002",
                Description = "Material natural para revestimentos sustentáveis.",
                Price = 7.99f,
                Stock = 30,
                Unit = "kg",
                CategoryId = cat1.Id,
                UserId = supplier.Id,
                IsSustainable = true,
                Tags = new List<Tag> { tag2 }
            };
            context.Items.Add(mat2);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, mat2.Id, "https://fakestoreapi.com/img/71li-ujtlUL._AC_UX679_.jpg");

            // PRODUTO 1
            var prod1 = new Product
            {
                Name = "Martelo de Madeira",
                Reference = "PROD001",
                Description = "Ideal para trabalhos manuais sem danificar a madeira.",
                Price = 15.99f,
                Stock = 20,
                Unit = "unidade",
                CategoryId = cat2.Id,
                UserId = vendor.Id,
                IsSustainable = false,
                Tags = new List<Tag> { tag1 }
            };
            context.Items.Add(prod1);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, prod1.Id, "https://fakestoreapi.com/img/71YXzeOuslL._AC_UY879_.jpg");

            // PRODUTO 2
            var prod2 = new Product
            {
                Name = "Pá de Jardinagem",
                Reference = "PROD002",
                Description = "Ferramenta leve para uso em hortas e pequenos jardins.",
                Price = 10.50f,
                Stock = 40,
                Unit = "unidade",
                CategoryId = cat2.Id,
                UserId = vendor.Id,
                IsSustainable = true,
                Tags = new List<Tag> { tag2 }
            };
            context.Items.Add(prod2);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, prod2.Id, "https://fakestoreapi.com/img/51Y5NI-I5jL._AC_UX679_.jpg");
        }
    }

    public static class SeederUtils
    {
        public static async Task<IFormFile?> DownloadImageAsFormFileAsync(string imageUrl, string fileName = "image.jpg")
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl);

            if (!response.IsSuccessStatusCode) return null;

            var stream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        public static async Task<bool> DownloadAndSaveImage(ApplicationDbContext context, int itemId, string imageUrl)
        {
            var formFile = await DownloadImageAsFormFileAsync(imageUrl);
            if (formFile == null) return false;

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads/items/{itemId}");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            var image = await ImageHelper.SaveItemImage(formFile, itemId, imagesFolder);
            context.ItemImages.Add(image);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
