using System.Collections;
using System.Net;
using System.Security.Claims;
using System.Text;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AirdropArchitect.Functions.Tests.TestDoubles;

internal static class FunctionHttpTestFactory
{
    public static TestHttpRequestData CreateHttpRequest(
        string body = "",
        Dictionary<string, string>? headers = null,
        string method = "POST")
    {
        var context = CreateFunctionContext();
        var request = new TestHttpRequestData(context, body, method);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        return request;
    }

    public static string ReadBodyAsString(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private static TestFunctionContext CreateFunctionContext()
    {
        var services = new ServiceCollection();
        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer();
        });

        return new TestFunctionContext(services.BuildServiceProvider());
    }
}

internal sealed class TestFunctionContext : FunctionContext
{
    public TestFunctionContext(IServiceProvider instanceServices)
    {
        InstanceServices = instanceServices;
        Items = new Dictionary<object, object>();
        Features = new TestInvocationFeatures();
    }

    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId { get; } = "test-function";
    public override TraceContext TraceContext { get; } = null!;
    public override BindingContext BindingContext { get; } = null!;
    public override RetryContext RetryContext { get; } = null!;
    public override IServiceProvider InstanceServices { get; set; }
    public override FunctionDefinition FunctionDefinition { get; } = null!;
    public override IDictionary<object, object> Items { get; set; }
    public override IInvocationFeatures Features { get; }
}

internal sealed class TestInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = new();

    public T Get<T>()
    {
        return _features.TryGetValue(typeof(T), out var feature)
            ? (T)feature
            : default!;
    }

    public void Set<T>(T instance)
    {
        if (instance is null)
        {
            _features.Remove(typeof(T));
            return;
        }

        _features[typeof(T)] = instance;
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
    {
        return _features.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal sealed class TestHttpRequestData : HttpRequestData
{
    private readonly Stream _body;

    public TestHttpRequestData(FunctionContext functionContext, string body = "", string method = "POST")
        : base(functionContext)
    {
        _body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        Headers = new HttpHeadersCollection();
        Cookies = Array.Empty<IHttpCookie>();
        Url = new Uri("https://localhost/api/test");
        Identities = Array.Empty<ClaimsIdentity>();
        Method = method;
    }

    public override Stream Body => _body;
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; }
    public override string Method { get; }

    public override HttpResponseData CreateResponse()
    {
        return new TestHttpResponseData(FunctionContext);
    }
}

internal sealed class TestHttpResponseData : HttpResponseData
{
    public TestHttpResponseData(FunctionContext functionContext)
        : base(functionContext)
    {
        StatusCode = HttpStatusCode.OK;
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
        Cookies = new TestHttpCookies();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies { get; }
}

internal sealed class TestHttpCookies : HttpCookies
{
    public override void Append(string name, string value)
    {
    }

    public override void Append(IHttpCookie cookie)
    {
    }

    public override IHttpCookie CreateNew()
    {
        return new TestHttpCookie();
    }
}

internal sealed class TestHttpCookie : IHttpCookie
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? Path { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public bool? HttpOnly { get; set; }
    public double? MaxAge { get; set; }
    public bool? Secure { get; set; }
    public SameSite SameSite { get; set; }
}
