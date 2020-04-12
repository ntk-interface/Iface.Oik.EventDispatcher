using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iface.Oik.EventDispatcher.Test
{
  public class DispatcherTest
  {
    public class CreateHandlerMethod
    {
      [Fact]
      public void ThrowsWhenNotValidJson()
      {
        Func<Task> act = async ()
          => await Dispatcher.CreateHandler(AllHandlers,
                                            A.Dummy<string>(),
                                            "{NOT VALID JSON]");

        act.Should().Throw<JsonException>();
      }


      [Fact]
      public void ThrowsWhenNotFoundHandler()
      {
        Func<Task> act = async ()
          => await Dispatcher.CreateHandler(AllHandlers,
                                            A.Dummy<string>(),
                                            GetDummyConfig("Totally not found handler"));

        act.Should().Throw<Exception>();
      }


      [Fact]
      public void ThrowsWhenExceptionInsideConfigure()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateHandler(AllHandlers,
                                         A.Dummy<string>(),
                                         GetDummyConfig(nameof(ThrowsInsideConfigureDummyHandler)));

        act.Should().Throw<Exception>();
      }


      [Fact]
      public void ThrowsWhenExceptionInsideInitialize()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateHandler(AllHandlers,
                                         A.Dummy<string>(),
                                         GetDummyConfig(nameof(ThrowsInsideInitializeDummyHandler)));

        act.Should().Throw<Exception>();
      }


      [Theory]
      [InlineData(nameof(DummyHandler),        typeof(DummyHandler))]
      [InlineData(nameof(AnotherDummyHandler), typeof(AnotherDummyHandler))]
      public async void ReturnsCorrectHandler(string handlerName, Type expectedHandlerType)
      {
        var result = await Dispatcher.CreateHandler(AllHandlers,
                                                    A.Dummy<string>(),
                                                    GetDummyConfig(handlerName));

        result.Should().BeOfType(expectedHandlerType);
      }
    }


    private class DummyHandler : Handler
    {
      protected override Task Execute(IReadOnlyCollection<TmEvent> tmEvents)
      {
        return Task.CompletedTask;
      }
    }


    private class AnotherDummyHandler : DummyHandler
    {
    }


    private class ThrowsInsideConfigureDummyHandler : DummyHandler
    {
      public override void Configure(JObject options)
      {
        throw new Exception();
      }
    }


    private class ThrowsInsideInitializeDummyHandler : DummyHandler
    {
      public override Task Initialize()
      {
        throw new Exception();
      }
    }


    private static readonly List<Type> AllHandlers = new List<Type>
    {
      typeof(DummyHandler),
      typeof(AnotherDummyHandler),
      typeof(ThrowsInsideConfigureDummyHandler),
      typeof(ThrowsInsideInitializeDummyHandler),
    };


    private static string GetDummyConfig(string handlerName)
    {
      return JsonConvert.SerializeObject(new {Handler = handlerName});
    }
  }
}