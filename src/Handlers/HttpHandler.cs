using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher.Handlers
{
  public class HttpHandler : Handler
  {
    private readonly HttpClient _httpClient;

    private Options _options;


    public HttpHandler()
    {
      _httpClient = new HttpClient();
    }


    public override void Configure(JObject options)
    {
      if (options == null)
      {
        throw new Exception("Не заданы настройки");
      }

      _options = options.ToObject<Options>();
      new OptionsValidator().ValidateAndThrow(_options);
    }


    private class Options
    {
      public HttpMethod? Method { get; set; }
      public string      Url    { get; set; }
      public string      Body   { get; set; }
    }


    private enum HttpMethod
    {
      Get,
      Post,
    }


    private class OptionsValidator : AbstractValidator<Options>
    {
      public OptionsValidator()
      {
        RuleFor(o => o.Method).NotNull().IsInEnum();
        RuleFor(o => o.Url).NotNull().NotEmpty();
      }
    }


    protected override async Task Execute(IReadOnlyCollection<TmEvent> tmEvents)
    {
      foreach (var tmEvent in tmEvents)
      {
        var request = new HttpRequestMessage(GetHttpMethod(_options.Method), 
                                             GetTemplatedString(_options.Url, tmEvent));
        if (_options.Body != null)
        {
          request.Content = new StringContent(GetTemplatedString(_options.Body, tmEvent));
        }

        await _httpClient.SendAsync(request);
      }
    }


    private static System.Net.Http.HttpMethod GetHttpMethod(HttpMethod? method) => method switch
    {
      HttpMethod.Get  => System.Net.Http.HttpMethod.Get,
      HttpMethod.Post => System.Net.Http.HttpMethod.Post,
      _               => throw new Exception("Неизвестный HTTP-метод"),
    };
  }
}