namespace Talabat.PL.Helper
{
    public class AddPicFile
    {
        public static string AddPic(IFormFile file, string folderName)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/", folderName, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return $"Images/{folderName}/{fileName}";
        }

        // delete image
        public static void DeletePic(string filePath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
