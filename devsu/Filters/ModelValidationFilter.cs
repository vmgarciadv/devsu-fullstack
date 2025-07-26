using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using devsu.DTOs;

namespace devsu.Filters
{
    public class ModelValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var response = new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    "Error de validación",
                    400,
                    "Uno o más campos de validación fallaron",
                    context.HttpContext.Request.Path
                );

                foreach (var key in context.ModelState.Keys)
                {
                    var errors = context.ModelState[key].Errors.Select(e => e.ErrorMessage).ToList();
                    if (errors.Any())
                    {
                        response.Errors[key] = errors;
                    }
                }

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}