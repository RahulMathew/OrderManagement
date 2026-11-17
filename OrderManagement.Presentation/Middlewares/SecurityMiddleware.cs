namespace OrderManagement.Presentation.Middlewares
{
    public class SecurityMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
}
