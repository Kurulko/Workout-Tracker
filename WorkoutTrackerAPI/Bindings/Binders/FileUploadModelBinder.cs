using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Bindings.Binders;

public class FileUploadModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var file = bindingContext.HttpContext.Request.Form.Files
            .FirstOrDefault(f => f.Name.ToLower() == bindingContext.FieldName.ToLower());

        if (file is null)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }
        else
        {
            var fileUploadModel = new FileUploadModel
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = file.OpenReadStream()
            };

            bindingContext.Result = ModelBindingResult.Success(fileUploadModel);
        }

        return Task.CompletedTask;
    }
}
