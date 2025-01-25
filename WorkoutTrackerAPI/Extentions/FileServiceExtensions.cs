using WorkoutTrackerAPI.Services.FileServices;

namespace WorkoutTrackerAPI.Extentions;

public static class FileServiceExtensions
{

    const long kilobytesInOneMegabyte = 1024;
    const long bytesInOneKilobyte = 1024;
    static long GetBytesInMegabytes(int bytes)
        => bytes * kilobytesInOneMegabyte * bytesInOneKilobyte;

    public static async Task<string?> GetImage(this IFileService fileService, IFormFile? file, string directory, int maxFileSizeInMegabytes, bool isUniqueName = true)
    {
        if (file is null)
            return null;

        return await fileService.UploadImageAsync(file, directory, GetBytesInMegabytes(maxFileSizeInMegabytes), isUniqueName);
    }
}