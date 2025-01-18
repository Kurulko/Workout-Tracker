using WorkoutTrackerAPI.Exceptions;

namespace WorkoutTrackerAPI.Services.FileServices;

public class FileService : IFileService
{
    readonly IWebHostEnvironment environment;
    public FileService(IWebHostEnvironment environment)
        => this.environment = environment;

    readonly string sourcePath = Path.Combine("Data", "Source");

    public async Task<string> UploadFileAsync(IFormFile file, string directory, string[] allowedExtensions, long maxFileSize)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Only {string.Join(", ", allowedExtensions)} are allowed.");
        }

        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds the maximum limit of {maxFileSize / 1024 / 1024} MB.");
        }

        string fileName = $"{Guid.NewGuid()}{extension}";
        //string fileName = file.FileName;
        string contentPath = environment.ContentRootPath;
        string filePath = Path.Combine(directory, fileName);
        string fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        string fileDirectory = Path.GetDirectoryName(fullFilePath)!;
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return filePath;
    }

    readonly string[] imageExtensions = new string[] { ".jpg", ".jpeg", ".png" };
    public async Task<string> UploadImageAsync(IFormFile file, string directory, long maxFileSize)
        => await UploadFileAsync(file, directory, imageExtensions, maxFileSize);

    public string DownloadFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullOrEmptyException(nameof(filePath));

        string contentPath = environment.ContentRootPath;
        string fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        if (!File.Exists(fullFilePath))
            throw new FileNotFoundException("File not found", fullFilePath);

        return fullFilePath;
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullOrEmptyException(nameof(filePath));

        string contentPath = environment.ContentRootPath;
        string fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        if (!File.Exists(fullFilePath))
            throw new FileNotFoundException("File not found", filePath);

        File.Delete(fullFilePath);
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullOrEmptyException(nameof(filePath));

        string contentPath = environment.ContentRootPath;
        string fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        return File.Exists(fullFilePath);
    }
}