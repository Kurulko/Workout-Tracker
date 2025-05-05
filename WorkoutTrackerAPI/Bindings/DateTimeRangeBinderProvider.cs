using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkoutTracker.API.Binders;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Bindings;

public class DateTimeRangeBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext bindingContext)
    {
        if (bindingContext.Metadata.ModelType == typeof(DateTimeRange)) 
        {
            return new DateTimeRangeBinder();
        }

        return null;
    }
}