using System.Collections.Specialized;
using System.Text;
using System.Net;

public class FeatureCollection : Dictionary<Type, object>
{ 
    public FeatureCollection Set<T>(T feature)
    {
        this[typeof(T)] = feature;
        return this;
    }

    public T Get<T>() => this.TryGetValue(typeof(T), out var value) ? (T)value : default(T);
}

public interface IHttpRequestFeature
{
    Uri Url { get; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}

public interface IHttpResponseFeature
{
    int StatusCode { get; set; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}

public class HttpRequest
{
    private readonly IHttpRequestFeature _feature;
    public Uri Url => _feature.Url;
    public NameValueCollection Headers => _feature.Headers;
    public Stream Body => _feature.Body;
    public HttpRequest(FeatureCollection features) => _feature = features.Get<IHttpRequestFeature>();
}

public class HttpResponse
{
    private readonly IHttpResponseFeature _feature;
    public NameValueCollection Headers => _feature.Headers;
    public Stream Body => _feature.Body;
    public int StatusCode { get => _feature.StatusCode; set => _feature.StatusCode = value; }
    public HttpResponse(FeatureCollection features) => _feature = features.Get<IHttpResponseFeature>();

    public Task WriteAsync(string contents)
    {
        var buffer = Encoding.UTF8.GetBytes(contents);
        return this.Body.WriteAsync(buffer, 0, buffer.Length);
    }
}

public class HttpContext
{
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }
    public HttpContext(FeatureCollection features)
    {
        Request = new HttpRequest(features);
        Response = new HttpResponse(features);
    }
}


public class HttpListenerFeature : IHttpRequestFeature, IHttpResponseFeature
{
    private readonly HttpListenerContext _context;
    public HttpListenerFeature(HttpListenerContext context) => _context = context;
    Uri IHttpRequestFeature.Url => _context.Request.Url;
    NameValueCollection IHttpRequestFeature.Headers => _context.Request.Headers;
    Stream IHttpRequestFeature.Body => _context.Request.InputStream;

    NameValueCollection IHttpResponseFeature.Headers => _context.Response.Headers;
    Stream IHttpResponseFeature.Body => _context.Response.OutputStream;
    int IHttpResponseFeature.StatusCode
    {
        get { return _context.Response.StatusCode; }
        set { _context.Response.StatusCode = value; }
    }
}