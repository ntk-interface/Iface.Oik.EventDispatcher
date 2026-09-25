using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.EventDispatcher.Workers;

public class HttpWorker : Worker
{
    private readonly HttpClient _httpClient = new();

    private Options _options;

    public override void Configure(WorkerOptions options)
    {
        _options = options.Get<Options>();
        OptionsGuard.ThrowIfNotInEnum(_options.Method, "method");
        OptionsGuard.ThrowIfNullOrEmpty(_options.Url, "url");
    }

    private class Options
    {
        public HttpMethod? Method { get; init; }
        public string Url { get; init; }
        public string Body { get; init; }
    }

    private enum HttpMethod
    {
        Get,
        Post,
    }

    protected override async Task DoWork(
        IReadOnlyCollection<TmEvent> tmEvents,
        CancellationToken stoppingToken
    )
    {
        foreach (var tmEvent in tmEvents)
        {
            var request = new HttpRequestMessage(
                GetHttpMethod(_options.Method),
                GetBody(_options.Url, tmEvent)
            );
            if (_options.Body != null)
            {
                request.Content = new StringContent(GetBody(_options.Body, tmEvent));
            }

            await _httpClient.SendAsync(request, stoppingToken);
        }
    }

    private static System.Net.Http.HttpMethod GetHttpMethod(HttpMethod? method) =>
        method switch
        {
            HttpMethod.Get => System.Net.Http.HttpMethod.Get,
            HttpMethod.Post => System.Net.Http.HttpMethod.Post,
            _ => throw new Exception("Неизвестный HTTP-метод"),
        };
}
