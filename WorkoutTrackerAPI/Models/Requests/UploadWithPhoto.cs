using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Bindings;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Models.Requests;

public class UploadWithPhoto<T>
{
    public T Model { get; set; } = default!;

    [ModelBinder(BinderType = typeof(FileUploadModelBinder))]
    public FileUploadModel? Photo { get; set; }
}