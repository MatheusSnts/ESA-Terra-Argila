using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

public static class Seeder
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
        var tags = new List<Tag>();
        for (int i = 1; i <= 5; i++)
        {
            tags.Add(new Tag
            {
                Name = $"Tag {i}",
                Reference = $"TTAG{i}",
                IsPublic = true,
                UserId = i % 2 == 0 ? vendor.Id : supplier.Id
            });
        }
        context.Tags.AddRange(tags);

        // CATEGORIAS
        var categories = new List<Category>();
        for (int i = 1; i <= 5; i++)
        {
            categories.Add(new Category
            {
                Name = $"Categoria {i}",
                Reference = $"TCAT{i}",
                UserId = i % 2 == 0 ? vendor.Id : supplier.Id
            });
        }
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // GET FAKE PRODUCTS
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://fakestoreapi.com/products?limit=50");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var fakeProducts = JsonSerializer.Deserialize<List<FakeProduct>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var rand = new Random();

        // MATERIAIS (primeiros 50 do fakeProducts)
        for (int i = 0; i < 50; i++)
        {
            var product = fakeProducts[i % fakeProducts.Count];
            var mat = new Material
            {
                Name = product.Title,
                Reference = $"TMAT{i + 1}",
                Description = product.Description.Length > 500 ? product.Description.Substring(0, 500) : product.Description,
                Price = (float)product.Price,
                Stock = rand.Next(0, 100),
                Unit = UnitsHelper.GetUnitsSelectList().ElementAt(rand.Next(UnitsHelper.GetUnitsSelectList().Count())).Value,
                CategoryId = categories[rand.Next(categories.Count)].Id,
                UserId = supplier.Id,
                IsSustainable = rand.Next(0, 2) == 1,
                Tags = new List<Tag> { tags[rand.Next(tags.Count)] }
            };
            context.Items.Add(mat);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, mat.Id, product.Image);
        }

        // PRODUTOS (reusando os mesmos dados)
        for (int i = 0; i < 50; i++)
        {
            var product = fakeProducts[i % fakeProducts.Count];
            var prod = new Product
            {
                Name = product.Title,
                Reference = $"TPROD{i + 1}",
                Description = product.Description.Length > 500 ? product.Description.Substring(0, 500) : product.Description,
                Price = (float)(product.Price + rand.NextDouble() * 50),
                Stock = rand.Next(0, 50),
                Unit = UnitsHelper.GetUnitsSelectList().ElementAt(rand.Next(UnitsHelper.GetUnitsSelectList().Count())).Value,
                CategoryId = categories[rand.Next(categories.Count)].Id,
                UserId = vendor.Id,
                IsSustainable = rand.Next(0, 2) == 1,
                Tags = new List<Tag> { tags[rand.Next(tags.Count)] }
            };
            context.Items.Add(prod);
            await context.SaveChangesAsync();
            await SeederUtils.DownloadAndSaveImage(context, prod.Id, product.Image);
        }
    }

    private class FakeProduct
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
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
