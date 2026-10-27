using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartMaint.Api.Common.Validation;

/// <summary>
/// Runs the FluentValidation validator of every action argument that has one, before the action executes.
/// So: write a validator next to the DTO and it is applied automatically — no manual calls in controllers.
/// </summary>
internal sealed class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is IValidator validator)
            {
                var result = await validator.ValidateAsync(
                    new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        await next();
    }
}
