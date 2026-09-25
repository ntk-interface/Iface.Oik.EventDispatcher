using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.EventDispatcher.Workers;

public class FileWorker : Worker
{
    private Options _options;

    public override void Configure(WorkerOptions options)
    {
        _options = options.Get<Options>();
        OptionsGuard.ThrowIfNullOrEmpty(_options.FilePath, "filePath");
    }

    private class Options
    {
        public string FilePath { get; init; }
        public string Body { get; init; }
    }

    protected override Task DoWork(
        IReadOnlyCollection<TmEvent> tmEvents,
        CancellationToken stoppingToken
    )
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
