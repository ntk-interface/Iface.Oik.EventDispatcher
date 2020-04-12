using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher
{
  public class ConfigModel
  {
    public string            Handler { get; set; }
    public ConfigFilterModel Filter  { get; set; }
    public JObject           Options { get; set; }
  }


  public class ConfigFilterModel
  {
    public List<TmEventTypes> Types       { get; set; }
    public List<int>          Importances { get; set; }
    public List<string>       Statuses    { get; set; }
    public List<string>       Analogs     { get; set; }
  }
}