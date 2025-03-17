using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Helpers
{
    public static class ImageHelper
    {
        public static readonly string ProductImagesFolder = "/uploads/products/";
        public static readonly string MaterialImagesFolder = "/uploads/materials/";
        public static async Task<ProductImage> SaveProductImage(IFormFile? file, int productId, string imagesFolder)
        {
            if (file == null)
            {
                return default;
            }   
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(imagesFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new ProductImage
            {
                ProductId = productId,
                Name = uniqueFileName,
                Path = $"{ProductImagesFolder}{productId}/{uniqueFileName}"
            };
        }

        public static async Task<MaterialImage> SaveMaterialImage(IFormFile? file, int materialId, string imagesFolder)
        {
            if (file == null)
            {
                return default;
            }
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(imagesFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new MaterialImage
            {
                MaterialId = materialId,
                Name = uniqueFileName,
                Path = $"{MaterialImagesFolder}{materialId}/{uniqueFileName}"
            };
        }
    }
}
