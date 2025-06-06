﻿using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.Interfaces.Services;

public interface IFileService : IBaseService
{
    Task<string> UploadFileAsync(FileUploadModel file, string directory, string[] allowedExtensions, long maxFileSize, bool isUniqueName = true, CancellationToken cancellationToken = default);
    Task<string> UploadImageAsync(FileUploadModel file, string directory, long maxFileSize, bool isUniqueName = true, CancellationToken cancellationToken = default);

    string DownloadFile(string filePath);
    bool FileExists(string filePath);
    void DeleteFile(string filePath);
}
