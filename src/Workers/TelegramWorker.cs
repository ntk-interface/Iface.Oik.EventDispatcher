using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Interfaces;
using Telegram.Bot;

namespace Iface.Oik.EventDispatcher.Workers;

public class TelegramWorker : Worker
{
    private Options _options;
    private TelegramBotClient _bot;

    public override void Configure(WorkerOptions options)
    {
        _options = options.Get<Options>();
        OptionsGuard.ThrowIfNullOrEmpty(_options.BotToken, "botToken");
        OptionsGuard.ThrowIfEmpty(_options.ChatIds, "chatIds");
    }

    private class Options
    {
        public string BotToken { get; init; }
        public string[] ChatIds { get; init; }
        public string Body { get; init; }
    }

    public override async Task Initialize()
    {
        _bot = new TelegramBotClient(_options.BotToken);

        await _bot.GetMe();
    }

    protected override async Task DoWork(
        IReadOnlyCollection<TmEvent> tmEvents,
        CancellationToken stoppingToken
    )
    {
        foreach (var tmEvent in tmEvents)
        {
            foreach (var chatId in _options.ChatIds)
            {
                await _bot.SendMessage(
                    chatId,
                    GetBodyOrDefault(_options.Body, tmEvent),
                    cancellationToken: stoppingToken
                );
            }
        }
    }
}
