using System;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.EventDispatcher.Test.Util;
using Xunit;

namespace Iface.Oik.EventDispatcher.Test
{
  public class HandlerTest
  {
    public class GetDefaultBodyMethod
    {
      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var result = Handler.GetDefaultBody(null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsCorrectString()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetDefaultBody(ev);

        result.Should()
              .Be(
                $"{ev.Time} | {ev.ImportanceAlias} | {ev.Text} | {ev.StateString} | {ev.TypeString} | {ev.Username}");
      }
    }


    public class GetBodyOrDefaultMethod
    {
      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var body = A.Dummy<string>();

        var result = Handler.GetBodyOrDefault(body, null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsDefaultWhenTemplateIsNull()
      {
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetBodyOrDefault(null, tmEvent);

        result.Should().Be(Handler.GetDefaultBody(tmEvent));
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsBody(string body)
      {
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetBodyOrDefault(body, tmEvent);

        result.Should().Be(Handler.GetBody(body, tmEvent));
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTime(DateTime time)
      {
        var body    = "Dummy {time}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Handler.GetBodyOrDefault(body, tmEvent);

        result.Should().Be($"Dummy {time}");
      }
    }


    public class GetBodyMethod
    {
      [Fact]
      public void ReturnsNullWhenTemplateIsNull()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetBody(null, ev);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var body = "Dummy";

        var result = Handler.GetBody(body, null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsSameStringWhenNoSubstitutes()
      {
        var body    = "Dummy no substitutes";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be(body);
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTime(DateTime time)
      {
        var body    = "Dummy {time}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {time}");
      }


      [Theory]
      [InlineData(0)]
      [InlineData(1)]
      [InlineData(2)]
      [InlineData(3)]
      public void ReturnsStringWithSubstitutedImportanceId(short importanceId)
      {
        var body    = "Dummy {importanceId}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Importance = importanceId);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {importanceId}");
      }


      [Theory]
      [InlineData(0, "ОС")]
      [InlineData(1, "ПС2")]
      [InlineData(2, "ПС1")]
      [InlineData(3, "АС")]
      public void ReturnsStringWithSubstitutedImportance(short importanceId, string importance)
      {
        var body    = "Dummy {importance}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Importance = importanceId);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {importance}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedText(string text)
      {
        var body    = "Dummy {text}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Name = text);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {text}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedState(string state)
      {
        var body    = "Dummy {state}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecStateText = state);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {state}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedType(string type)
      {
        var body    = "Dummy {type}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecTypeName = type);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {type}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedUsername(string username)
      {
        var body    = "Dummy {username}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UserName = username);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {username}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTmAddr(string tmAddr)
      {
        var body    = "Dummy {tmAddr}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.TmaStr = tmAddr);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {tmAddr}");
      }


      [Fact]
      public void ReturnsStringWithSubstitutedDefaultBody()
      {
        var body    = "Dummy {defaultBody}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {Handler.GetDefaultBody(tmEvent)}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedMultipleFields(DateTime time, string text, string state)
      {
        var body = "Dummy {time} {text} {state} {text}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.UpdateTime   = time;
          dto.Name         = text;
          dto.RecStateText = state;
        });

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {time} {text} {state} {text}");
      }


      [Theory]
      [InlineData("dd.MM.yyyy", "12.04.2020")]
      [InlineData("HH:mm",      "09:07")]
      public void ReturnsStringWithSubstitutedTimeFormat(string format, string expected)
      {
        var body    = "Dummy {time:" + format + "}";
        var time    = new DateTime(2020, 04, 12, 09, 07, 00);
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Handler.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {expected}");
      }
    }
  }
}