using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Turnit.GenericStore.Api.Infrastructure;

public class MeasureTimeAttribute : ActionFilterAttribute
{
    private Stopwatch _stopwatch;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch.Stop();

        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
        context.HttpContext.Response.Headers.Add("X-Action-Execution-Time", $"{elapsedMilliseconds}ms");
    }
}