namespace MyCompany.MinimalApi.Middlewares;

public class RequestLogMiddleware
{
    private readonly RequestDelegate _next;
    public RequestLogMiddleware(RequestDelegate next)
    {
        this._next = next;       
    }

    public async Task InvokeAsync(HttpContext context)
    {
        System.Console.WriteLine($"The Request Path: {context.Request.Path}");
        await _next(context);
        Console.WriteLine($"The Response Status: {context.Response.StatusCode}");
    }
}