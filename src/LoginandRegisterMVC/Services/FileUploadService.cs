namespace LoginandRegisterMVC.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool ValidateImage(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "Please select an image file.";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            errorMessage = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.";
            return false;
        }

        if (file.Length > MaxFileSize)
        {
            errorMessage = "File size cannot exceed 5MB.";
            return false;
        }

        return true;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folderPath)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, folderPath);
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return Path.Combine(folderPath, uniqueFileName).Replace("\\", "/");
    }

    public bool DeleteImage(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, imagePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}

