using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Bindings.Binders;

public class DateTimeRangeBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var firstDateValue = bindingContext.ValueProvider.GetValue("firstDate").FirstValue;
        var lastDateValue = bindingContext.ValueProvider.GetValue("lastDate").FirstValue;

        if (firstDateValue is null || lastDateValue is null)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }
        else if (DateTime.TryParse(firstDateValue, out var firstDate) &&
            DateTime.TryParse(lastDateValue, out var lastDate))
        {
            var range = new DateTimeRange(firstDate, lastDate);
            bindingContext.Result = ModelBindingResult.Success(range);
        }
        else
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid date format.");
        }

        return Task.CompletedTask;
    }
}