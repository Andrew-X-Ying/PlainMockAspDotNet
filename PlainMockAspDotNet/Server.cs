using System.Net;
public class HttpListenerServer
{
    private readonly HttpListener _httpListener;
    private readonly string[] _urls;
    public HttpListenerServer(params string[] urls)
    {
        _httpListener = new HttpListener();
        _urls = urls.Any() ? urls : new string[] { "http://localhost:5000/" };
    }

    public async Task StartAsync(RequestDelegate handler)
    {
        Array.ForEach(_urls, url => _httpListener.Prefixes.Add(url));
        _httpListener.Start();
        Console.WriteLine("Server started and is listening on: {0}", string.Join(';', _urls));
        while (true)
        {
            HttpListenerContext listenerContext = await _httpListener.GetContextAsync();
            var feature = new HttpListenerFeature(listenerContext);
            var features = new FeatureCollection()
                .Set<IHttpRequestFeature>(feature)
                .Set<IHttpResponseFeature>(feature);
            var httpContext = new HttpContext(features);
            await handler(httpContext);
            listenerContext.Response.Close();
        }
    }
}

