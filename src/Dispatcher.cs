using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.EventDispatcher
{
    public class Dispatcher : BackgroundService
    {
        private static readonly string ConfigsPath = Path.Combine(
            AppContext.BaseDirectory,
            "configs"
        );

        private readonly IOikDataApi _api;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<Worker> _workers = new List<Worker>();
        private TmEventElix _currentElix = null!;

        public Dispatcher(IOikDataApi api, IServiceProvider serviceProvider)
        {
            _api = api;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await LoadWorkers();

            _currentElix = await _api.GetCurrentEventsElix();

            await base.StartAsync(cancellationToken);
        }

        private async Task LoadWorkers()
        {
            if (!Directory.Exists(ConfigsPath))
            {
                throw new Exception("Не найден каталог с файлами обработчиков событий");
            }

            foreach (var file in Directory.GetFiles(ConfigsPath, "*.json"))
            {
                var name = Path.GetFileName(file);
                _workers.Add(await CreateWorker(_serviceProvider, name, File.ReadAllText(file)));
            }

            if (_workers.Count == 0)
            {
                throw new Exception("Не найдено ни одного обработчика событий");
            }

            Tms.PrintMessage($"Всего обработчиков событий: {_workers.Count}");
        }

        public static async Task<Worker> CreateWorker(
            IServiceProvider serviceProvider,
            string name,
            string configText
        )
        {
            var config = ReadConfig(name, configText);

            var workerName = config.Worker;
            if (string.IsNullOrWhiteSpace(workerName))
            {
                throw new Exception($"Не задан обработчик в файле {name}");
            }

            try
            {
                var worker = serviceProvider.GetRequiredKeyedService<Worker>(workerName);
                worker
                    .SetName(name)
                    .SetFilter(new WorkerFilter(config.Filter))
                    .Configure(new WorkerOptions(config.Options));
                await worker.Initialize();

                return worker;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Ошибка обработчика {workerName} в файле {name}: {ex.Message}",
                    ex
                );
            }
        }

        private static WorkerConfig ReadConfig(string name, string configText)
        {
            try
            {
                return JsonSerializer.Deserialize<WorkerConfig>(configText, JsonSettings.Options)
                    ?? throw new Exception("Пустой файл конфигурации");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при разборе файла {name}: {ex.Message}", ex);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2500, stoppingToken);
                await Dispatch(stoppingToken);
            }
        }

        private async Task Dispatch(CancellationToken stoppingToken)
        {
            if (!await IsElixUpdated())
            {
                return;
            }

            var (newEvents, newElix) = await _api.GetCurrentEvents(_currentElix);
            if (newEvents == null)
            {
                return;
            }
            Tms.PrintDebug($"Обнаружены новые события: {newEvents.Count}. Начинается обработка");

            await Task.WhenAll(_workers.Select(h => h.FilterAndDoWork(newEvents, stoppingToken)));

            _currentElix = newElix;
        }

        private async Task<bool> IsElixUpdated()
        {
            var newElix = await _api.GetCurrentEventsElix();
            if (newElix == null) // вероятно ошибка связи
            {
                return false;
            }
            return _currentElix != newElix;
        }
    }
}
