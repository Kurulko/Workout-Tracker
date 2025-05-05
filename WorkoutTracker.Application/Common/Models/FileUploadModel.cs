namespace WorkoutTracker.Application.Common.Models;

public class FileUploadModel
{
    public Stream Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
