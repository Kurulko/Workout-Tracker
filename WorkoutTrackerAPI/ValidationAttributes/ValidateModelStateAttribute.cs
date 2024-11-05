using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WorkoutTrackerAPI.ValidationAttributes;

public class ValidateModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
               .Where(e => e.Value?.Errors.Any() ?? false)
               .ToDictionary(
                   e => e.Key,
                   e => e.Value?.Errors.Select(error => error.ErrorMessage).ToArray() ?? Array.Empty<string>()
               );
            context.Result = new BadRequestObjectResult(new { Errors = errors });
        }
    }
}