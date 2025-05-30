using FluentValidation;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators;

internal class FileUploadModelValidator : AbstractValidator<FileUploadModel>
{
    public FileUploadModelValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmptyOrNull();

        RuleFor(x => x.ContentType)
            .NotEmptyOrNull();
    }
}
