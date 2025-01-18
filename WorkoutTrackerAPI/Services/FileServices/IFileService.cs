namespace WorkoutTrackerAPI.Services.FileServices;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string directory, string[] allowedExtensions, long maxFileSize);
    Task<string> UploadImageAsync(IFormFile file, string directory, long maxFileSize);
    string DownloadFile(string filePath);
    bool FileExists(string filePath);
    void DeleteFile(string filePath);
}
