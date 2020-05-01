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
    public class CreateWorkerMethod
    {
      [Fact]
      public void ThrowsWhenNotValidJson()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateWorker(AllWorkers,
                                        A.Dummy<string>(),
                                        "{NOT VALID JSON]");

        act.Should().Throw<JsonException>();
      }


      [Fact]
      public void ThrowsWhenNotFoundWorker()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateWorker(AllWorkers,
                                        A.Dummy<string>(),
                                        GetDummyConfig("Totally not found worker"));

        act.Should().Throw<Exception>();
      }


      [Fact]
      public void ThrowsWhenExceptionInsideConfigure()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateWorker(AllWorkers,
                                        A.Dummy<string>(),
                                        GetDummyConfig(nameof(ThrowsInsideConfigureDummyWorker)));

        act.Should().Throw<Exception>();
      }


      [Fact]
      public void ThrowsWhenExceptionInsideInitialize()
      {
        Func<Task> act = async () =>
          await Dispatcher.CreateWorker(AllWorkers,
                                        A.Dummy<string>(),
                                        GetDummyConfig(nameof(ThrowsInsideInitializeDummyWorker)));

        act.Should().Throw<Exception>();
      }


      [Theory]
      [InlineData(nameof(DummyWorker),        typeof(DummyWorker))]
      [InlineData(nameof(AnotherDummyWorker), typeof(AnotherDummyWorker))]
      public async void ReturnsCorrectWorker(string workerName, Type expectedWorkerType)
      {
        var result = await Dispatcher.CreateWorker(AllWorkers,
                                                   A.Dummy<string>(),
                                                   GetDummyConfig(workerName));

        result.Should().BeOfType(expectedWorkerType);
      }
    }


    private class DummyWorker : Worker
    {
      protected override Task DoWork(IReadOnlyCollection<TmEvent> tmEvents)
      {
        return Task.CompletedTask;
      }
    }


    private class AnotherDummyWorker : DummyWorker
    {
    }


    private class ThrowsInsideConfigureDummyWorker : DummyWorker
    {
      public override void Configure(JObject options)
      {
        throw new Exception();
      }
    }


    private class ThrowsInsideInitializeDummyWorker : DummyWorker
    {
      public override Task Initialize()
      {
        throw new Exception();
      }
    }


    private static readonly List<Type> AllWorkers = new List<Type>
    {
      typeof(DummyWorker),
      typeof(AnotherDummyWorker),
      typeof(ThrowsInsideConfigureDummyWorker),
      typeof(ThrowsInsideInitializeDummyWorker),
    };


    private static string GetDummyConfig(string workerName)
    {
      return JsonConvert.SerializeObject(new {Worker = workerName});
    }
  }
}