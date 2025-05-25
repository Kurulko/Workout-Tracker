using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WorkoutTracker.API.Filters;

public class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            var errorMessage = string.Join("; ", errors);
            context.Result = new BadRequestObjectResult(errorMessage);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}