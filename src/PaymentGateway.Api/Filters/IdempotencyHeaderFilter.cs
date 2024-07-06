using System.Collections.Concurrent;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PaymentGateway.Api.Filters;

public class IdempotencyHeaderFilter : ActionFilterAttribute
{
    private readonly string _headerName;

    //Cache here will be a dictionary, but would be Elasticache or similar
    //Needs to have multiple replicas so this isn't a single point of failure
    //Moving data out of memory ensures state isn't tied to specific pod
    private readonly ConcurrentDictionary<string, string> _cache;

    public IdempotencyHeaderFilter(string headerName)
    {
        _headerName = headerName;
        _cache = [];
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out Microsoft.Extensions.Primitives.StringValues value))
        {
            context.Result = new BadRequestObjectResult($"Missing required header: {_headerName}");
            return;
        }

        string headerValue = value!;

        if (_cache.TryGetValue(headerValue, out var _))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
            return;
        }
        else
        {
            _cache.TryAdd(headerValue, "");
        }
    }
}