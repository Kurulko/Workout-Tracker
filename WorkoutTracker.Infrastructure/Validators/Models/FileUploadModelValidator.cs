using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;

namespace WorkoutTracker.Infrastructure.Validators.Models;

public class FileUploadModelValidator
{
    public void Validate(FileUploadModel fileUploadModel)
    {
        ArgumentValidator.ThrowIfEntryNull(fileUploadModel.Content, nameof(FileUploadModel.Content));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(fileUploadModel.FileName, nameof(FileUploadModel.FileName));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(fileUploadModel.ContentType, nameof(FileUploadModel.ContentType));
    }
}
