public delegate Task RequestDelegate(HttpContext context);

public class Program
{
    public static void Main(string[] args)
    {
        WebHostBuilder whb = new WebHostBuilder().UseHttpListener();
        Action<ApplicationBuilder> configure = 
            app => app.Use(MiddleWares.FooMiddleware)
                .Use(MiddleWares.BarMiddleware)
                .Use(MiddleWares.BazMiddleware);
        WebHost wh = whb.Configure(configure).Build();
        wh.StartAsync();            
    }
}

public class ApplicationBuilder
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();
    public RequestDelegate Build()
    {
        _middlewares.Reverse();
        return httpContext =>
        {
            RequestDelegate next = _ => { _.Response.StatusCode = 404; return Task.CompletedTask; };

            foreach (var middleware in _middlewares)
            {
                next = middleware(next);
            }
            return next(httpContext);
        };
    }

    public ApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }
}


public class WebHostBuilder
{
    private HttpListenerServer _server;
    private readonly List<Action<ApplicationBuilder>> _configures = new List<Action<ApplicationBuilder>>();

    public WebHostBuilder Configure(Action<ApplicationBuilder> configure)
    {
        _configures.Add(configure);
        return this;
    }

    public WebHostBuilder UseHttpListener(params string[] urls)
    {
        _server = new HttpListenerServer(urls);
        return this;
    }

    public WebHost Build()
    {
        var builder = new ApplicationBuilder();
        foreach (var configure in _configures)
        {
            configure(builder);
        }
        return new WebHost(_server, builder.Build());
    }
}

public class WebHost
{
    private readonly HttpListenerServer _server;
    private readonly RequestDelegate _handler;
    public WebHost(HttpListenerServer server, RequestDelegate handler)
    {
        _server = server;
        _handler = handler;
    }

    public Task StartAsync() => _server.StartAsync(_handler);
}


