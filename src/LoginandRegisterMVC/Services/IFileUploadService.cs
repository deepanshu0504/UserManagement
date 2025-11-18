namespace LoginandRegisterMVC.Services;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(IFormFile file, string folderPath);
    bool DeleteImage(string imagePath);
    bool ValidateImage(IFormFile file, out string errorMessage);
}

