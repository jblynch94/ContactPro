using ContactPro.Services.Interfaces;

namespace ContactPro.Services
{
    public class ImageService : IImageService
    {

        private readonly string _defaultImage = "img/DefaultContactImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData is null)
            {
                return _defaultImage;
            }
            try
            {
                string ImageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension};base64,{ImageBase64Data}");
            }
            catch
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                return byteFile;
            }
            catch
            {
                throw;
            }
        }
    }
}
