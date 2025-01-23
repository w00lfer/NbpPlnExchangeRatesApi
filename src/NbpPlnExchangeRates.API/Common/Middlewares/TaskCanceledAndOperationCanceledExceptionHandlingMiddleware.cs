namespace NbpPlnExchangeRates.Api.Common.Middlewares;

public class TaskCanceledAndOperationCanceledExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public TaskCanceledAndOperationCanceledExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            context.Response.StatusCode = 409;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation canceled.");
            context.Response.StatusCode = 409;
        }
    }
}