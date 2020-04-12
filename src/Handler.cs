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
  public abstract class Handler
  {
    private string        _name;
    private HandlerFilter _filter;


    public Handler SetName(string name)
    {
      _name = name;

      return this;
    }


    public Handler SetFilter(HandlerFilter filter)
    {
      _filter = filter;

      return this;
    }


    public async Task FilterAndExecute(IReadOnlyCollection<TmEvent> tmEvents) // todo unit test
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
        await Execute(suitableEvents);
        Tms.PrintDebug($"Обработаны события для обработчика {_name}");
      }
      catch (Exception ex)
      {
        Tms.PrintError($"Ошибка при работе обработчика {_name}: {ex.Message}");
      }
    }


    public static string GetTemplatedString(string template, TmEvent ev)
    {
      if (template == null || ev == null)
      {
        return null;
      }

      var stringBuilder = new StringBuilder(template).Replace("{time}", ev.Time.ToString())
                                                     .Replace("{importanceId}", ev.Importance.ToString())
                                                     .Replace("{importance}",   ev.ImportanceAlias)
                                                     .Replace("{text}",         ev.Text)
                                                     .Replace("{state}",        ev.StateString)
                                                     .Replace("{type}",         ev.TypeString)
                                                     .Replace("{username}",     ev.Username)
                                                     .Replace("{tmAddr}",       ev.TmAddrString)
                                                     .Replace("{defaultBody}",  GetDefaultBody(ev));
      var str = stringBuilder.ToString();

      return Regex.Replace(str,
                           @"{time:(.*)}",
                           match => ev.Time?.ToString(match.Groups[1].Value) ?? string.Empty);
    }


    public static string GetDefaultBody(TmEvent ev)
    {
      return $"{ev.Time} | {ev.ImportanceAlias} | {ev.Text} | {ev.StateString} | {ev.TypeString} | {ev.Username}";
    }


    public virtual void Configure(JObject options)
    {
    }


    public virtual Task Initialize()
    {
      return Task.CompletedTask;
    }


    protected abstract Task Execute(IReadOnlyCollection<TmEvent> tmEvents);
  }
}