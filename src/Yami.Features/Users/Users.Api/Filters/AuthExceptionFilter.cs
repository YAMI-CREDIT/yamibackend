using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// Registered globally in DependencyInjection.cs so controllers don't need to catch
// AuthException themselves - they just let it bubble up and it becomes a ProblemDetails
// response with the right status code.
public class AuthExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AuthException ex)
        {
            return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Title = "Request failed",
            Detail = ex.Message,
            Status = ex.StatusCode
        })
        {
            StatusCode = ex.StatusCode
        };

        context.ExceptionHandled = true;
    }
}
