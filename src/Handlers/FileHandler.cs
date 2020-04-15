using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher.Handlers
{
  public class FileHandler : Handler
  {
    private Options _options;


    public override void Configure(JObject options)
    {
      if (options == null)
      {
        throw new Exception("Не заданы настройки");
      }

      _options = options.ToObject<Options>();
      new OptionsValidator().ValidateAndThrow(_options);
    }


    private class Options
    {
      public string FilePath { get; set; }
      public string Body     { get; set; }
    }


    private class OptionsValidator : AbstractValidator<Options>
    {
      public OptionsValidator()
      {
        RuleFor(o => o.FilePath).NotNull().NotEmpty();
      }
    } 


    protected override Task Execute(IReadOnlyCollection<TmEvent> tmEvents)
    {
      using (var writer = new StreamWriter(_options.FilePath, append: true))
      {
        foreach (var tmEvent in tmEvents)
        {
          writer.WriteLine(GetBodyOrDefault(_options.Body, tmEvent));
        }
      }

      return Task.CompletedTask;
    }
  }
}