public class MiddleWares
{
    public static async Task F1(HttpContext context)
    {
        await context.Response.WriteAsync($"Foo=>{DateTime.Now}");
    }


    public static RequestDelegate FooMiddleware(RequestDelegate next)
    => async context => {
            //await context.Response.WriteAsync($"Foo=>{DateTime.Now}");
            await F1(context);
            await next(context);
        };

    public static RequestDelegate BarMiddleware(RequestDelegate next)
    => async context => {
            await context.Response.WriteAsync("Bar=>");
            await next(context);
        };

    public static RequestDelegate BazMiddleware(RequestDelegate next)
    {
        return async delegate (HttpContext context)
        {
            await context.Response.WriteAsync("Baz");
            await next(context);
        };
    }
}