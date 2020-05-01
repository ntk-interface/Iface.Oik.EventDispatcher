using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Iface.Oik.EventDispatcher
{
  public class Dispatcher : BackgroundService
  {
    private static readonly string ConfigsPath = Path.Combine(
      Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
      "..",
      "configs");

    private readonly IOikDataApi              _api;
    private readonly IHostApplicationLifetime _applicationLifetime;

    private readonly List<Worker> _workers = new List<Worker>();
    private          TmEventElix  _currentElix;


    public Dispatcher(IOikDataApi api, IHostApplicationLifetime applicationLifetime)
    {
      _api                 = api;
      _applicationLifetime = applicationLifetime;
    }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
      if (!await LoadWorkers())
      {
        _applicationLifetime.StopApplication();
        return;
      }

      _currentElix = await _api.GetCurrentEventsElix();

      await base.StartAsync(cancellationToken);
    }


    private async Task<bool> LoadWorkers()
    {
      if (!Directory.Exists(ConfigsPath))
      {
        Tms.PrintError("Не найден каталог с файлами обработчиков событий");
        return false;
      }

      var allWorkers = FindAllWorkers();

      foreach (var file in Directory.GetFiles(ConfigsPath, "*.json"))
      {
        var name = Path.GetFileName(file);
        try
        {
          _workers.Add(await CreateWorker(allWorkers, name, File.ReadAllText(file)));
        }
        catch (JsonException ex)
        {
          Tms.PrintError($"Ошибка JSON при разборе файла {name}: {ex.Message}");
        }
        catch (Exception ex)
        {
          Tms.PrintError($"Ошибка при разборе файла {name}: {ex.Message}");
        }
      }

      if (_workers.Count == 0)
      {
        Tms.PrintError("Не найдено ни одного обработчика событий");
        return false;
      }

      Tms.PrintMessage($"Всего обработчиков событий: {_workers.Count}");
      return true;
    }


    private static List<Type> FindAllWorkers()
    {
      return Assembly.GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.IsSubclassOf(typeof(Worker)))
                     .ToList();
    }


    public static async Task<Worker> CreateWorker(IEnumerable<Type> allWorkers, string name, string configText)
    {
      var config = JsonConvert.DeserializeObject<WorkerConfig>(configText);

      var worker = CreateWorkerInstance(allWorkers, config.Worker);
      if (worker == null)
      {
        throw new Exception($"Не найден обработчик {config.Worker}");
      }

      worker.SetName(name)
            .SetFilter(new WorkerFilter(config.Filter))
            .Configure(config.Options);
      await worker.Initialize();

      return worker;
    }


    private static Worker CreateWorkerInstance(IEnumerable<Type> allWorkers, string name)
    {
      var type = allWorkers.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
      if (type == null)
      {
        return null;
      }
      return Activator.CreateInstance(type) as Worker;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(5000, stoppingToken);
        await Dispatch();
      }
    }


    private async Task Dispatch() // todo unit test
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

      await Task.WhenAll(_workers.Select(h => h.FilterAndDoWork(newEvents)));

      _currentElix = newElix;
    }


    private async Task<bool> IsElixUpdated() // todo unit test
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