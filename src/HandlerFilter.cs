using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.EventDispatcher
{
  public class HandlerFilter
  {
    public TmEventTypes Types       { get; private set; }
    public List<int>    Importances { get; } = new List<int>();
    public List<uint>   Statuses    { get; } = new List<uint>();
    public List<uint>   Analogs     { get; } = new List<uint>();


    public HandlerFilter(ConfigFilterModel configFilter)
    {
      SetTypes(configFilter?.Types);
      SetImportances(configFilter?.Importances);
      SetStatuses(configFilter?.Statuses);
      SetAnalogs(configFilter?.Analogs);
    }


    private void SetTypes(List<TmEventTypes> types)
    {
      if (types == null)
      {
        Types = TmEventTypes.Any;
        return;
      }
      types.ForEach(t => Types |= t);
    }


    private void SetImportances(List<int> importances)
    {
      Importances.AddRange(importances ?? new List<int> {0, 1, 2, 3});
    }


    private void SetStatuses(List<string> statuses)
    {
      if (statuses       == null ||
          statuses.Count == 0)
      {
        return;
      }

      var addrGroupRegex = new Regex(@"^(\d+):(\d+):(\d+)..(\d+)$");
      foreach (var status in statuses)
      {
        if (TmAddr.TryParse(status, out var tmAddr, TmType.Status))
        {
          Statuses.Add(tmAddr.ToComplexInteger());
          continue;
        }
        var groupMatch = addrGroupRegex.Match(status);
        if (groupMatch.Success)
        {
          var ch         = int.Parse(groupMatch.Groups[1].Value);
          var rtu        = int.Parse(groupMatch.Groups[2].Value);
          var firstPoint = int.Parse(groupMatch.Groups[3].Value);
          var lastPoint  = int.Parse(groupMatch.Groups[4].Value);
          for (var point = firstPoint; point <= lastPoint; point++)
          {
            try
            {
              Statuses.Add(new TmAddr(TmType.Status, ch, rtu, point).ToComplexInteger());
            }
            catch (Exception)
            {
              throw new Exception($"Некорректная группа сигналов: {status}");
            }
          }
          continue;
        }
        throw new Exception($"Некорректный адрес сигнала: {status}");
      }
      
      Statuses.Sort();
    }


    private void SetAnalogs(List<string> analogs)
    {
      if (analogs       == null ||
          analogs.Count == 0)
      {
        return;
      }

      var addrGroupRegex = new Regex(@"^(\d+):(\d+):(\d+)..(\d+)$");
      foreach (var analog in analogs)
      {
        if (TmAddr.TryParse(analog, out var tmAddr, TmType.Analog))
        {
          Analogs.Add(tmAddr.ToComplexInteger());
          continue;
        }
        var groupMatch = addrGroupRegex.Match(analog);
        if (groupMatch.Success)
        {
          var ch         = int.Parse(groupMatch.Groups[1].Value);
          var rtu        = int.Parse(groupMatch.Groups[2].Value);
          var firstPoint = int.Parse(groupMatch.Groups[3].Value);
          var lastPoint  = int.Parse(groupMatch.Groups[4].Value);
          for (var point = firstPoint; point <= lastPoint; point++)
          {
            try
            {
              Analogs.Add(new TmAddr(TmType.Analog, ch, rtu, point).ToComplexInteger());
            }
            catch (Exception)
            {
              throw new Exception($"Некорректная группа сигналов: {analog}");
            }
          }
          continue;
        }
        throw new Exception($"Некорректный адрес измерения: {analog}");
      }
      
      Analogs.Sort();
    }


    public bool IsEventSuitable(TmEvent tmEvent)
    {
      return IsEventSuitableForTypes(tmEvent)       &&
             IsEventSuitableForImportances(tmEvent) &&
             IsEventSuitableForStatusesAndAnalogs(tmEvent);
    }


    private bool IsEventSuitableForTypes(TmEvent tmEvent)
    {
      return Types.HasFlag(tmEvent.Type);
    }


    private bool IsEventSuitableForImportances(TmEvent tmEvent)
    {
      return Importances.Contains(tmEvent.Importance);
    }


    private bool IsEventSuitableForStatusesAndAnalogs(TmEvent tmEvent)
    {
      if (Statuses.Count == 0 &&
          Analogs.Count  == 0)
      {
        return true;
      }
      return (tmEvent.HasTmStatus &&
              Statuses.BinarySearch(tmEvent.TmAddrComplexInteger) >= 0)
             ||
             (tmEvent.HasTmAnalog &&
              Analogs.BinarySearch(tmEvent.TmAddrComplexInteger) >= 0);
    }
  }
}