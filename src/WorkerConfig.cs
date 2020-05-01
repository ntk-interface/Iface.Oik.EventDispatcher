using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher
{
  public class WorkerConfig
  {
    public string             Worker  { get; set; }
    public WorkerFilterConfig Filter  { get; set; }
    public JObject            Options { get; set; }
  }


  public class WorkerFilterConfig
  {
    public List<TmEventTypes> Types       { get; set; }
    public List<int>          Importances { get; set; }
    public List<string>       Statuses    { get; set; }
    public List<string>       Analogs     { get; set; }
  }
}