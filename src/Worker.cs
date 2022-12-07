using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher
{
  public abstract class Worker
  {
    private string       _name;
    private WorkerFilter _filter;


    public Worker SetName(string name)
    {
      _name = name;

      return this;
    }


    public Worker SetFilter(WorkerFilter filter)
    {
      _filter = filter;

      return this;
    }


    public async Task FilterAndDoWork(IReadOnlyCollection<TmEvent> tmEvents) // todo unit test
    {
      var suitableEvents = tmEvents.Where(ev => _filter.IsEventSuitable(ev))
                                   .ToList();
      if (suitableEvents.Count == 0)
      {
        Tms.PrintDebug($"Отсутствуют подходящие события для обработчика {_name}");
        return;
      }
      try
      {
        await DoWork(suitableEvents);
        Tms.PrintDebug($"Обработаны события для обработчика {_name}");
      }
      catch (Exception ex)
      {
        Tms.PrintError($"Ошибка при работе обработчика {_name}: {ex.Message}");
      }
    }


    public static string GetBodyOrDefault(string template, TmEvent tmEvent)
    {
      return GetBody(template, tmEvent) ?? GetDefaultBody(tmEvent);
    }


    public static string GetBody(string template, TmEvent tmEvent)
    {
      if (template == null || tmEvent == null)
      {
        return null;
      }

      var stringBuilder = new StringBuilder(template).Replace("{time}", tmEvent.Time.ToString())
                                                     .Replace("{importanceId}", tmEvent.Importance.ToString())
                                                     .Replace("{importance}",   tmEvent.ImportanceAlias)
                                                     .Replace("{name}",         tmEvent.Text)
                                                     .Replace("{state}",        tmEvent.StateString)
                                                     .Replace("{type}",         tmEvent.TypeString)
                                                     .Replace("{username}",     tmEvent.Username)
                                                     .Replace("{tmAddr}",       tmEvent.TmAddrString)
                                                     .Replace("{ref}",          tmEvent.Reference?.ToString() ?? "-")
                                                     .Replace("{defaultBody}",  GetDefaultBody(tmEvent));
      var str = stringBuilder.ToString();

      return Regex.Replace(str,
                           @"{time:(.*)}",
                           match => tmEvent.Time?.ToString(match.Groups[1].Value) ?? string.Empty);
    }


    public static string GetDefaultBody(TmEvent ev)
    {
      if (ev == null)
      {
        return null;
      }
      return $"{ev.Time} | {ev.ImportanceAlias} | {ev.Text} | {ev.StateString} | {ev.TypeString} | {ev.Username}";
    }


    public virtual void Configure(JObject options)
    {
    }


    public virtual Task Initialize()
    {
      return Task.CompletedTask;
    }


    protected abstract Task DoWork(IReadOnlyCollection<TmEvent> tmEvents);
  }
}