using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Helpers
{
    public static class ImageHelper
    {
        public static readonly string ItemImagesFolder = "/uploads/items/";
        public static async Task<ItemImage> SaveItemImage(IFormFile? file, int itemId, string imagesFolder)
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

            return new ItemImage
            {
                ItemId = itemId,
                Name = uniqueFileName,
                Path = $"{ItemImagesFolder}{itemId}/{uniqueFileName}"
            };
        }
    }
}
