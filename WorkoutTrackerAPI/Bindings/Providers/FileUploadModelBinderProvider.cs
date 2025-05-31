using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkoutTracker.API.Bindings.Binders;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Bindings.Providers;

public class FileUploadModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(FileUploadModel))
        {
            return new FileUploadModelBinder();
        }

        return null;
    }
}