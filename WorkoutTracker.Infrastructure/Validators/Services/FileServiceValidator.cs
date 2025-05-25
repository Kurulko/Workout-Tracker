using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;

namespace WorkoutTracker.Infrastructure.Validators.Services;

public class FileServiceValidator
{
    const int bytesInKilobyte = 1024;
    const int KilobytesInMegabyte = 1024;
    const int bytesInMegabyte = bytesInKilobyte * KilobytesInMegabyte;

    public void ValidateUpload(FileUploadModel file, string[] allowedExtensions, long maxFileSize)
    {
        if (file == null || file.Content == null || file.Content.Length == 0)
            throw new ValidationException("No file provided.");

        ArgumentValidator.ThrowIfCollectionNullOrEmpty(allowedExtensions, nameof(allowedExtensions));

        foreach (var allowedExtension in allowedExtensions)
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(allowedExtension, nameof(allowedExtension));

        ArgumentValidator.ThrowIfValueNegative(maxFileSize, nameof(maxFileSize));

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
            throw new ValidationException($"Only {string.Join(", ", allowedExtensions)} are allowed.");

        if (file.Content.Length > maxFileSize)
            throw new ValidationException($"File size exceeds the maximum limit of {maxFileSize / bytesInMegabyte} MB.");
    }

    public void ValidateFilePath(string filePath)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(filePath, nameof(filePath));
    }
}
