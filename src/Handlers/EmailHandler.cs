using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Iface.Oik.Tm.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json.Linq;

namespace Iface.Oik.EventDispatcher.Handlers
{
  public class EmailHandler : Handler
  {
    private const string DefaultSubject = "Новые события ОИК Диспетчер НТ";

    private Options             _options;
    private InternetAddressList _addressList;


    public override void Configure(JObject options)
    {
      if (options == null)
      {
        throw new Exception("Не заданы настройки");
      }

      _options = options.ToObject<Options>();
      new OptionsValidator().ValidateAndThrow(_options);
      _addressList = new InternetAddressList(_options.SendTo.Select(MailboxAddress.Parse));
    }


    private class Options
    {
      public string   Host        { get; set; }
      public int      Port        { get; set; }
      public bool     UseSsl      { get; set; }
      public string   Login       { get; set; }
      public string   Password    { get; set; }
      public string   From        { get; set; }
      public string   FromEmail   { get; set; }
      public string[] SendTo      { get; set; }
      public bool     IsHtml      { get; set; }
      public string   Subject     { get; set; }
      public string   Body        { get; set; }
      public bool     BatchEvents { get; set; }
    }


    private class OptionsValidator : AbstractValidator<Options>
    {
      public OptionsValidator()
      {
        RuleFor(x => x.Host).NotNull().NotEmpty();
        RuleFor(x => x.Port).NotEqual(0);
        RuleFor(x => x.From).NotNull().NotEmpty();
        RuleFor(x => x.FromEmail).NotNull().NotEmpty();
        RuleFor(x => x.SendTo).NotNull().NotEmpty();
      }
    }


    public override async Task Initialize()
    {
      using (var client = new SmtpClient())
      {
        await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl);
        if (IsAuthRequired())
        {
          await client.AuthenticateAsync(_options.Login, _options.Password);
        }
        await client.DisconnectAsync(true);
      }
    }


    protected override async Task Execute(IReadOnlyCollection<TmEvent> tmEvents)
    {
      var mimeMessage = new MimeMessage();

      mimeMessage.To.AddRange(_addressList);
      mimeMessage.From.Add(new MailboxAddress(_options.From, _options.FromEmail));

      using (var client = new SmtpClient())
      {
        await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl);
        if (IsAuthRequired())
        {
          await client.AuthenticateAsync(_options.Login, _options.Password);
        }

        if (_options.BatchEvents)
        {
          mimeMessage.Subject = GetSubject();
          mimeMessage.Body = new TextPart(_options.IsHtml ? TextFormat.Html : TextFormat.Plain)
          {
            Text = string.Join("\n\n", tmEvents.Select(tmEvent => GetBodyOrDefault(_options.Body, tmEvent)))
          };
        }
        else
        {
          foreach (var tmEvent in tmEvents)
          {
            mimeMessage.Subject = GetSubject(tmEvent);
            mimeMessage.Body = new TextPart(_options.IsHtml ? TextFormat.Html : TextFormat.Plain)
            {
              Text = GetBodyOrDefault(_options.Body, tmEvent)
            };
            await client.SendAsync(mimeMessage);
          }
        }

        await client.DisconnectAsync(true);
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
}