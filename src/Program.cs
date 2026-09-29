using System;
using Iface.Oik.EventDispatcher.Workers;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.EventDispatcher;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            TmStartup.Connect();
        }
        catch (Exception ex)
        {
            Tms.PrintError(ex.Message);
            Environment.Exit(-1);
        }

        try
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (_, services) =>
                    {
                        // регистрация сервисов ОИК
                        services.AddSingleton<ITmsApi, TmsApi>();
                        services.AddSingleton<IOikSqlApi, OikSqlApi>();
                        services.AddSingleton<IOikDataApi, OikDataApi>();
                        services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
                        services.AddSingleton<ServerService>();
                        services.AddSingleton<ICommonServerService>(provider =>
                            provider.GetRequiredService<ServerService>()
                        );

                        // регистрация фоновых служб
                        services.AddHostedService<TmStartup>();
                        services.AddSingleton<IHostedService>(provider =>
                            provider.GetRequiredService<ServerService>()
                        );
                        services.AddHostedService<Dispatcher>();

                        // регистрация обработчиков событий
                        services.AddKeyedTransient<Worker, EmailWorker>("EmailWorker");
                        services.AddKeyedTransient<Worker, FileWorker>("FileWorker");
                        services.AddKeyedTransient<Worker, HttpWorker>("HttpWorker");
                        services.AddKeyedTransient<Worker, TelegramWorker>("TelegramWorker");
                    }
                )
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Tms.PrintError(ex.Message);
            Environment.Exit(-1);
        }
    }
}
