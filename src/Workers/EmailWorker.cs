using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.EventDispatcher.Util;
using Iface.Oik.Tm.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Iface.Oik.EventDispatcher.Workers;

public class EmailWorker : Worker
{
    private const string DefaultSubject = "Новые события ОИК Диспетчер НТ";

    private Options _options;
    private InternetAddressList _addressList;

    public override void Configure(WorkerOptions options)
    {
        _options = options.Get<Options>();
        OptionsGuard.ThrowIfNullOrEmpty(_options.Host, "host");
        OptionsGuard.ThrowIfZero(_options.Port, "port");
        OptionsGuard.ThrowIfNullOrEmpty(_options.From, "from");
        OptionsGuard.ThrowIfNullOrEmpty(_options.FromEmail, "fromEmail");
        OptionsGuard.ThrowIfEmpty(_options.SendTo, "sendTo");
        _addressList = new InternetAddressList(_options.SendTo.Select(MailboxAddress.Parse));
    }

    private class Options
    {
        public string Host { get; init; }
        public int Port { get; init; }
        public bool UseSsl { get; init; }
        public string Login { get; init; }
        public string Password { get; init; }
        public string From { get; init; }
        public string FromEmail { get; init; }
        public string[] SendTo { get; init; }
        public bool IsHtml { get; init; }
        public string Subject { get; init; }
        public string Body { get; init; }
        public bool BatchEvents { get; init; }
    }

    public override async Task Initialize()
    {
        using (var client = new SmtpClient())
        {
            if (_options.UseSsl)
            {
                await client.ConnectAsync(_options.Host, _options.Port, true);
            }
            else
            {
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.None);
            }
            if (IsAuthRequired())
            {
                await client.AuthenticateAsync(_options.Login, _options.Password);
            }
            await client.DisconnectAsync(true);
        }
    }

    protected override async Task DoWork(
        IReadOnlyCollection<TmEvent> tmEvents,
        CancellationToken stoppingToken
    )
    {
        var mimeMessage = new MimeMessage();

        mimeMessage.To.AddRange(_addressList);
        mimeMessage.From.Add(new MailboxAddress(_options.From, _options.FromEmail));

        using (var client = new SmtpClient())
        {
            if (_options.UseSsl)
            {
                await client.ConnectAsync(_options.Host, _options.Port, true, stoppingToken);
            }
            else
            {
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(
                    _options.Host,
                    _options.Port,
                    SecureSocketOptions.None,
                    stoppingToken
                );
            }

            if (IsAuthRequired())
            {
                await client.AuthenticateAsync(_options.Login, _options.Password, stoppingToken);
            }

            if (_options.BatchEvents)
            {
                mimeMessage.Subject = GetSubject();
                mimeMessage.Body = new TextPart(
                    _options.IsHtml ? TextFormat.Html : TextFormat.Plain
                )
                {
                    Text = string.Join(
                        "\n\n",
                        tmEvents.Select(tmEvent => GetBodyOrDefault(_options.Body, tmEvent))
                    ),
                };
                await client.SendAsync(mimeMessage, stoppingToken);
            }
            else
            {
                foreach (var tmEvent in tmEvents)
                {
                    mimeMessage.Subject = GetSubject(tmEvent);
                    mimeMessage.Body = new TextPart(
                        _options.IsHtml ? TextFormat.Html : TextFormat.Plain
                    )
                    {
                        Text = GetBodyOrDefault(_options.Body, tmEvent),
                    };
                    await client.SendAsync(mimeMessage, stoppingToken);
                }
            }

            await client.DisconnectAsync(true, stoppingToken);
        }
    }

    private string GetSubject()
    {
        return _options.Subject ?? DefaultSubject;
    }

    private string GetSubject(TmEvent tmEvent)
    {
        return GetBody(_options.Subject, tmEvent) ?? DefaultSubject;
    }

    private bool IsAuthRequired()
    {
        return !string.IsNullOrEmpty(_options.Login);
    }
}
