using Microsoft.AspNetCore.Hosting;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Infrastructure.Validators.Services;

namespace WorkoutTracker.Infrastructure.Services;

internal class FileService : IFileService
{
    readonly IWebHostEnvironment environment;
    readonly FileServiceValidator fileServiceValidator;
    public FileService(
        IWebHostEnvironment environment,
        FileServiceValidator fileServiceValidator
    )
    {
        this.environment = environment;
        this.fileServiceValidator = fileServiceValidator;
    }

    readonly string sourcePath = Path.Combine("Data", "Source");

    public async Task<string> UploadFileAsync(FileUploadModel file, string directory, string[] allowedExtensions, long maxFileSize, bool isUniqueName, CancellationToken cancellationToken)
    {
        fileServiceValidator.ValidateUpload(file, allowedExtensions, maxFileSize);

        var extension = Path.GetExtension(file.FileName).ToLower();
        string fileName = isUniqueName ? $"{Guid.NewGuid()}{extension}" : file.FileName;

        string contentPath = environment.ContentRootPath;
        string filePath = Path.Combine(directory, fileName);
        string fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        string fileDirectory = Path.GetDirectoryName(fullFilePath)!;
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        using var fileStream = new FileStream(fullFilePath, FileMode.Create);
        await file.Content.CopyToAsync(fileStream, cancellationToken);

        return filePath;
    }

    readonly string[] imageExtensions = [".jpg", ".jpeg", ".png"];
    public async Task<string> UploadImageAsync(FileUploadModel file, string directory, long maxFileSize, bool isUniqueName, CancellationToken cancellationToken)
        => await UploadFileAsync(file, directory, imageExtensions, maxFileSize, isUniqueName, cancellationToken);

    public string DownloadFile(string filePath)
    {
        var fileExists = FileExists(filePath, out string fullFilePath);

        if (!fileExists)
            throw new FileNotFoundException("File not found", filePath);

        return fullFilePath;
    }

    public void DeleteFile(string filePath)
    {
        var fileExists = FileExists(filePath, out string fullFilePath);

        if (!fileExists)
            throw new FileNotFoundException("File not found", filePath);

        File.Delete(fullFilePath);
    }

    public bool FileExists(string filePath)
        => FileExists(filePath, out string _);

    bool FileExists(string filePath, out string fullFilePath)
    {
        fileServiceValidator.ValidateFilePath(filePath);

        string contentPath = environment.ContentRootPath;
        fullFilePath = Path.Combine(contentPath, sourcePath, filePath);

        return File.Exists(fullFilePath);
    }
}