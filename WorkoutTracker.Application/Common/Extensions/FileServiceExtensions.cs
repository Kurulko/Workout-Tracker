using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Services;

namespace WorkoutTracker.Application.Common.Extensions;

public static class FileServiceExtensions
{
    const long kilobytesInOneMegabyte = 1024;
    const long bytesInOneKilobyte = 1024;
    static long GetBytesInMegabytes(int bytes)
        => bytes * kilobytesInOneMegabyte * bytesInOneKilobyte;

    public static async Task<string?> GetImageAsync(this IFileService fileService, FileUploadModel? file, string directory, int maxFileSizeInMegabytes, bool isUniqueName = true, CancellationToken cancellationToken = default)
    {
        if (file is null)
            return null;

        return await fileService.UploadImageAsync(file, directory, GetBytesInMegabytes(maxFileSizeInMegabytes), isUniqueName, cancellationToken);
    }
}