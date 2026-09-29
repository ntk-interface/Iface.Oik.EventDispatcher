using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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
                    await Dispatcher.CreateWorker(
                        CreateServiceProvider(),
                        A.Dummy<string>(),
                        "{NOT VALID JSON]"
                    );

                act.Should().Throw<Exception>();
            }

            [Fact]
            public void ThrowsWhenNotFoundWorker()
            {
                Func<Task> act = async () =>
                    await Dispatcher.CreateWorker(
                        CreateServiceProvider(),
                        A.Dummy<string>(),
                        GetDummyConfig("Totally not found worker")
                    );

                act.Should().Throw<Exception>();
            }

            [Fact]
            public void ThrowsWhenExceptionInsideConfigure()
            {
                Func<Task> act = async () =>
                    await Dispatcher.CreateWorker(
                        CreateServiceProvider(),
                        A.Dummy<string>(),
                        GetDummyConfig(nameof(ThrowsInsideConfigureDummyWorker))
                    );

                act.Should().Throw<Exception>();
            }

            [Fact]
            public void ThrowsWhenExceptionInsideInitialize()
            {
                Func<Task> act = async () =>
                    await Dispatcher.CreateWorker(
                        CreateServiceProvider(),
                        A.Dummy<string>(),
                        GetDummyConfig(nameof(ThrowsInsideInitializeDummyWorker))
                    );

                act.Should().Throw<Exception>();
            }

            [Theory]
            [InlineData(nameof(DummyWorker), typeof(DummyWorker))]
            [InlineData(nameof(AnotherDummyWorker), typeof(AnotherDummyWorker))]
            public async void ReturnsCorrectWorker(string workerName, Type expectedWorkerType)
            {
                var result = await Dispatcher.CreateWorker(
                    CreateServiceProvider(),
                    A.Dummy<string>(),
                    GetDummyConfig(workerName)
                );

                result.Should().BeOfType(expectedWorkerType);
            }
        }

        private class DummyWorker : Worker
        {
            protected override Task DoWork(
                IReadOnlyCollection<TmEvent> tmEvents,
                CancellationToken stoppingToken
            )
            {
                return Task.CompletedTask;
            }
        }

        private class AnotherDummyWorker : DummyWorker { }

        private class ThrowsInsideConfigureDummyWorker : DummyWorker
        {
            public override void Configure(WorkerOptions options)
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

        private static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<Worker, DummyWorker>(nameof(DummyWorker));
            services.AddKeyedTransient<Worker, AnotherDummyWorker>(nameof(AnotherDummyWorker));
            services.AddKeyedTransient<Worker, ThrowsInsideConfigureDummyWorker>(
                nameof(ThrowsInsideConfigureDummyWorker)
            );
            services.AddKeyedTransient<Worker, ThrowsInsideInitializeDummyWorker>(
                nameof(ThrowsInsideInitializeDummyWorker)
            );

            return services.BuildServiceProvider();
        }

        private static string GetDummyConfig(string workerName)
        {
            return JsonSerializer.Serialize(new { Worker = workerName }, JsonSettings.Options);
        }
    }
}
