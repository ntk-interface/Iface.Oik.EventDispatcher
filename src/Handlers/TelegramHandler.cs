using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Iface.Oik.Tm.Interfaces;
using Newtonsoft.Json.Linq;
using Telegram.Bot;

namespace Iface.Oik.EventDispatcher.Handlers
{
  public class TelegramHandler : Handler
  {
    private Options           _options;
    private TelegramBotClient _bot;


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
      public string BotToken { get; set; }
      public int[]  ChatIds  { get; set; }
      public string Body     { get; set; }
    }


    private class OptionsValidator : AbstractValidator<Options>
    {
      public OptionsValidator()
      {
        RuleFor(o => o.BotToken).NotNull().NotEmpty();
        RuleFor(o => o.ChatIds).NotNull().NotEmpty();
      }
    }


    public override async Task Initialize()
    {
      _bot = new TelegramBotClient(_options.BotToken);

      await _bot.GetMeAsync();
    }


    protected override async Task Execute(IReadOnlyCollection<TmEvent> tmEvents)
    {
      foreach (var tmEvent in tmEvents)
      {
        foreach (var chatId in _options.ChatIds)
        {
          await _bot.SendTextMessageAsync(chatId, GetBodyOrDefault(_options.Body, tmEvent));
        }
      }
    }
  }
}