namespace Graduation.Helpers
{
    public static class FileHelpers
    {
        public static async Task<string> SaveAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine("files", fileName);
            var fullPath = GetFullPath(filePath);

            using var fs = File.Create(fullPath);
            await file.CopyToAsync(fs);

            return filePath;
        }
        
        public static async Task SaveAsync(IFormFile file, string filePath)
        {
            var fullPath = GetFullPath(filePath);

            using var fs = File.Create(fullPath);
            await file.CopyToAsync(fs);
        }

        public static void Remove(string filePath)
            => File.Delete(GetFullPath(filePath));

        private static string GetFullPath(string relativePath)
             => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
    }
}
