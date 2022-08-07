public class MiddleWares
{
    public static RequestDelegate FooMiddleware(RequestDelegate next)
    => async context => {
        await context.Response.WriteAsync($"Foo=>{DateTime.Now}");
        await next(context);
    };

    public static RequestDelegate BarMiddleware(RequestDelegate next)
    => async context => {
        await context.Response.WriteAsync("Bar=>");
        await next(context);
    };

    public static RequestDelegate BazMiddleware(RequestDelegate next)
    => context => context.Response.WriteAsync("Baz");
}