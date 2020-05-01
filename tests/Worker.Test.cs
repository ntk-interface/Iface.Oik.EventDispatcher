using System;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.EventDispatcher.Test.Util;
using Xunit;

namespace Iface.Oik.EventDispatcher.Test
{
  public class WorkerTest
  {
    public class GetDefaultBodyMethod
    {
      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var result = Worker.GetDefaultBody(null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsCorrectString()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetDefaultBody(ev);

        result.Should().Be(
          $"{ev.Time} | {ev.ImportanceAlias} | {ev.Text} | {ev.StateString} | {ev.TypeString} | {ev.Username}");
      }
    }


    public class GetBodyOrDefaultMethod
    {
      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var body = A.Dummy<string>();

        var result = Worker.GetBodyOrDefault(body, null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsDefaultWhenTemplateIsNull()
      {
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetBodyOrDefault(null, tmEvent);

        result.Should().Be(Worker.GetDefaultBody(tmEvent));
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsBody(string body)
      {
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetBodyOrDefault(body, tmEvent);

        result.Should().Be(Worker.GetBody(body, tmEvent));
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTime(DateTime time)
      {
        var body    = "Dummy {time}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Worker.GetBodyOrDefault(body, tmEvent);

        result.Should().Be($"Dummy {time}");
      }
    }


    public class GetBodyMethod
    {
      [Fact]
      public void ReturnsNullWhenTemplateIsNull()
      {
        var ev = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetBody(null, ev);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsNullWhenEventIsNull()
      {
        var body = "Dummy";

        var result = Worker.GetBody(body, null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsSameStringWhenNoSubstitutes()
      {
        var body    = "Dummy no substitutes";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be(body);
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTime(DateTime time)
      {
        var body    = "Dummy {time}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Worker.GetBody(body, tmEvent);

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

        var result = Worker.GetBody(body, tmEvent);

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

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {importance}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedName(string name)
      {
        var body    = "Dummy {name}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Name = name);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {name}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedState(string state)
      {
        var body    = "Dummy {state}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecStateText = state);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {state}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedType(string type)
      {
        var body    = "Dummy {type}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecTypeName = type);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {type}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedUsername(string username)
      {
        var body    = "Dummy {username}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UserName = username);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {username}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedTmAddr(string tmAddr)
      {
        var body    = "Dummy {tmAddr}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.TmaStr = tmAddr);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {tmAddr}");
      }


      [Fact]
      public void ReturnsStringWithSubstitutedDefaultBody()
      {
        var body    = "Dummy {defaultBody}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent();

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {Worker.GetDefaultBody(tmEvent)}");
      }


      [Theory]
      [InlineAutoData]
      public void ReturnsStringWithSubstitutedMultipleFields(DateTime time, string name, string state)
      {
        var body = "Dummy {time} {name} {state} {name}";
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.UpdateTime   = time;
          dto.Name         = name;
          dto.RecStateText = state;
        });

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {time} {name} {state} {name}");
      }


      [Theory]
      [InlineData("dd.MM.yyyy", "12.04.2020")]
      [InlineData("HH:mm",      "09:07")]
      public void ReturnsStringWithSubstitutedTimeFormat(string format, string expected)
      {
        var body    = "Dummy {time:" + format + "}";
        var time    = new DateTime(2020, 04, 12, 09, 07, 00);
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.UpdateTime = time);

        var result = Worker.GetBody(body, tmEvent);

        result.Should().Be($"Dummy {expected}");
      }
    }
  }
}