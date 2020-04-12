using System;
using AutoFixture.Xunit2;
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
      public void ReturnsCorrectString()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetDefaultBody(ev);

        var expected =
          $"{ev.Time} | {ev.ImportanceAlias} | {ev.Text} | {ev.StateString} | {ev.TypeString} | {ev.Username}";
        result.Should().Be(expected);
      }
    }


    public class GetDefaultStringMethod
    {
      [Fact]
      public void ReturnsNullWhenTemplateIsNull()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetTemplatedString(null, ev);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var template = "Dummy";

        var result = Handler.GetTemplatedString(template, null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsSameStringWhenNoSubstitutes()
      {
        var template = "Dummy no substitutes";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be(template);
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTime(DateTime time)
      {
        var template = "Dummy {time}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {time}");
      }


      [Theory]
      [InlineData(0)]
      [InlineData(1)]
      [InlineData(2)]
      [InlineData(3)]
      public void ReturnsStringWithSubstitutedImportanceId(short importanceId)
      {
        var template = "Dummy {importanceId}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Importance = importanceId);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {importanceId}");
      }


      [Theory]
      [InlineData(0, "ОС")]
      [InlineData(1, "ПС2")]
      [InlineData(2, "ПС1")]
      [InlineData(3, "АС")]
      public void ReturnsStringWithSubstitutedImportance(short importanceId, string importance)
      {
        var template = "Dummy {importance}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Importance = importanceId);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {importance}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedText(string text)
      {
        var template = "Dummy {text}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Name = text);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {text}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedState(string state)
      {
        var template = "Dummy {state}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecStateText = state);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {state}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedType(string type)
      {
        var template = "Dummy {type}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecTypeName = type);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {type}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedUsername(string username)
      {
        var template = "Dummy {username}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UserName = username);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {username}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTmAddr(string tmAddr)
      {
        var template = "Dummy {tmAddr}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.TmaStr = tmAddr);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {tmAddr}");
      }


      [Fact]
      public void ReturnsStringWithSubstitutedDefaultBody()
      {
        var template = "Dummy {defaultBody}";
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent();

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {Handler.GetDefaultBody(tmEvent)}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedMultipleFields(DateTime time, string text, string state)
      {
        var template = "Dummy {time} {text} {state} {text}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.UpdateTime   = time;
          dto.Name         = text;
          dto.RecStateText = state;
        });

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {time} {text} {state} {text}");
      }


      [Theory]
      [InlineData("dd.MM.yyyy", "12.04.2020")]
      [InlineData("HH:mm", "09:07")]
      public void ReturnsStringWithSubstitutedTimeFormat(string format, string expected)
      {
        var template = "Dummy {time:" + format + "}";
        var time     = new DateTime(2020, 04, 12, 09, 07, 00);
        var tmEvent  = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Handler.GetTemplatedString(template, tmEvent);

        result.Should().Be($"Dummy {expected}");
      }
    }
  }
}