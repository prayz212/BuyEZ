using ClientManagementAPI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClientManagementAPI.Apis.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilterAttribute()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
        };
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
            return;
        }

        HandleUnknownException(context);
    }

    private void HandleValidationException(ExceptionContext context)
    {
        ValidationException? exception = context.Exception as ValidationException;
        ValidationProblemDetails details = new(exception!.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleInvalidModelStateException(ExceptionContext context)
    {
        ValidationProblemDetails details = new(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleNotFoundException(ExceptionContext context)
    {
        NotFoundException? exception = context.Exception as NotFoundException;
        ProblemDetails details = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception?.Message
        };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleUnknownException(ExceptionContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status500InternalServerError };
        context.ExceptionHandled = true;
    }
}