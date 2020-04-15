using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Iface.Oik.EventDispatcher.Test.Util;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.EventDispatcher.Test
{
  public class HandlerFilterTest
  {
    public class Constructor
    {
      [Fact]
      public void SetsDefaultValuesWhenNull()
      {
        var filter = new HandlerFilter(null);

        filter.Types.Should().Be(TmEventTypes.Any);
        filter.Importances.Should().Equal(0, 1, 2, 3);
        filter.Statuses.Should().BeEmpty();
        filter.Analogs.Should().BeEmpty();
      }


      [Fact]
      public void SetsAnyTypesWhenNull()
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Types = null});

        filter.Types.Should().Be(TmEventTypes.Any);
      }


      [Theory]
      [InlineData(TmEventTypes.Control,
                  TmEventTypes.Control)]
      [InlineData(TmEventTypes.Control | TmEventTypes.Alarm, TmEventTypes.Control,
                  TmEventTypes.Alarm)]
      [InlineData(TmEventTypes.Control | TmEventTypes.Alarm | TmEventTypes.Acknowledge,
                  TmEventTypes.Control,
                  TmEventTypes.Alarm,
                  TmEventTypes.Acknowledge)]
      public void SetsCorrectTypes(TmEventTypes expected, params TmEventTypes[] types)
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Types = types.ToList()});

        filter.Types.Should().Be(expected);
      }


      [Fact]
      public void SetsAllImportancesWhenNull()
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Importances = null});

        filter.Importances.Should().Equal(0, 1, 2, 3);
      }


      [Theory]
      [InlineData(0)]
      [InlineData(3)]
      [InlineData(1, 2, 3)]
      public void SetsCorrectImportances(params int[] importances)
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Importances = importances.ToList()});

        filter.Importances.Should().Equal(importances);
      }


      [Fact]
      public void SetsEmptyStatusesWhenNull()
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Statuses = null});

        filter.Statuses.Should().BeEmpty();
      }


      [Fact]
      public void SetsEmptyStatusesWhenEmpty()
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Statuses = new List<string>()});

        filter.Statuses.Should().BeEmpty();
      }


      [Fact]
      public void SetsCorrectStatuses()
      {
        var statuses = new List<string> {"0:1:1", "0:1:6", "11:1:2..4", "0:1:9"};

        var filter = new HandlerFilter(new ConfigFilterModel {Statuses = statuses});

        filter.Statuses.Should().Equal(TmAddr.EncodeComplexInteger(0,  1, 1),
                                       TmAddr.EncodeComplexInteger(0,  1, 6),
                                       TmAddr.EncodeComplexInteger(0,  1, 9),
                                       TmAddr.EncodeComplexInteger(11, 1, 2),
                                       TmAddr.EncodeComplexInteger(11, 1, 3),
                                       TmAddr.EncodeComplexInteger(11, 1, 4));
      }


      [Theory]
      [InlineData("abc")]
      [InlineData("255:1:1")]
      [InlineData("0:256:1")]
      [InlineData("0")]
      [InlineData("0:1")]
      [InlineData("0:1:a")]
      [InlineData("0:1:1..b")]
      [InlineData("255:1:1..2")]
      [InlineData("0:256:1..2")]
      public void ThrowsWhenInvalidStatuses(string invalidStatus)
      {
        var statuses = new List<string> {invalidStatus};

        Action act = () => new HandlerFilter(new ConfigFilterModel {Statuses = statuses});

        act.Should().Throw<Exception>();
      }


      [Fact]
      public void SetsEmptyAnalogsWhenEmpty()
      {
        var filter = new HandlerFilter(new ConfigFilterModel {Analogs = new List<string>()});

        filter.Analogs.Should().BeEmpty();
      }


      [Fact]
      public void SetsCorrectAnalogs()
      {
        var statuses = new List<string> {"20:1:1", "10:3:5", "0:1:1..5"};

        var filter = new HandlerFilter(new ConfigFilterModel {Statuses = statuses});

        filter.Statuses.Should().Equal(TmAddr.EncodeComplexInteger(0,  1, 1),
                                       TmAddr.EncodeComplexInteger(0,  1, 2),
                                       TmAddr.EncodeComplexInteger(0,  1, 3),
                                       TmAddr.EncodeComplexInteger(0,  1, 4),
                                       TmAddr.EncodeComplexInteger(0,  1, 5),
                                       TmAddr.EncodeComplexInteger(10, 3, 5),
                                       TmAddr.EncodeComplexInteger(20, 1, 1));
      }


      [Theory]
      [InlineData("abc")]
      [InlineData("255:1:1")]
      [InlineData("0:256:1")]
      [InlineData("0")]
      [InlineData("0:1")]
      [InlineData("0:1:a")]
      [InlineData("0:1:1..b")]
      [InlineData("255:1:1..2")]
      [InlineData("0:256:1..2")]
      public void ThrowsWhenInvalidAnalogs(string invalidAnalog)
      {
        var analogs = new List<string> {invalidAnalog};

        Action act = () => new HandlerFilter(new ConfigFilterModel {Analogs = analogs});

        act.Should().Throw<Exception>();
      }
    }


    public class IsEventSuitableMethod
    {
      [Fact]
      public void ReturnsTrueWhenEmptyFilter()
      {
        var filter   = new HandlerFilter(null);
        var tmEvents = new List<TmEvent>();
        for (var i = 0; i < 100; i++)
        {
          tmEvents.Add(TmEventUtil.CreateRandomValidTmEvent());
        }

        foreach (var tmEvent in tmEvents)
        {
          filter.IsEventSuitable(tmEvent)
                .Should()
                .BeTrue();
        }
      }


      [Theory]
      [InlineData(TmEventTypes.StatusChange,    true)]
      [InlineData(TmEventTypes.Alarm,           true)]
      [InlineData(TmEventTypes.Control,         false)]
      [InlineData(TmEventTypes.ManualAnalogSet, false)]
      public void ReturnsCorrectValueWithTypesFilter(TmEventTypes tmEventType, bool expected)
      {
        var types   = new List<TmEventTypes> {TmEventTypes.StatusChange, TmEventTypes.Alarm};
        var filter  = new HandlerFilter(new ConfigFilterModel {Types = types});
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto => dto.RecType = (short) tmEventType);

        var result = filter.IsEventSuitable(tmEvent);

        result.Should().Be(expected);
      }


      [Theory]
      [InlineData(0, false)]
      [InlineData(1, true)]
      [InlineData(2, true)]
      [InlineData(3, false)]
      public void ReturnsCorrectValueWithImportancesFilter(int importance, bool expected)
      {
        var importances = new List<int> {1, 2};
        var filter      = new HandlerFilter(new ConfigFilterModel {Importances = importances});
        var tmEvent     = TmEventUtil.CreateRandomValidTmEvent(dto => dto.Importance = (short) importance);

        var result = filter.IsEventSuitable(tmEvent);

        result.Should().Be(expected);
      }


      [Theory]
      [InlineData(10, 1, 1, false)]
      [InlineData(10, 1, 2, true)]
      [InlineData(10, 1, 3, true)]
      [InlineData(10, 1, 4, true)]
      [InlineData(10, 1, 5, false)]
      [InlineData(10, 1, 6, true)]
      public void ReturnsCorrectValueWithStatusesFilter(int ch, int rtu, int point, bool expected)
      {
        var statuses   = new List<string> {"10:1:2..4", "10:1:6"};
        var filter     = new HandlerFilter(new ConfigFilterModel {Statuses = statuses});
        var statusType = TmNativeDefs.TmDataTypes.Status;
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) statusType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(ch, rtu, point);
        });

        var result = filter.IsEventSuitable(tmEvent);

        result.Should().Be(expected);
      }


      [Theory]
      [InlineData(20, 3, 1, false)]
      [InlineData(20, 3, 2, true)]
      [InlineData(20, 3, 3, true)]
      [InlineData(20, 3, 4, true)]
      [InlineData(20, 3, 5, false)]
      [InlineData(20, 3, 6, true)]
      public void ReturnsCorrectValueWithAnalogsFilter(int ch, int rtu, int point, bool expected)
      {
        var analogs    = new List<string> {"20:3:2..4", "20:3:6"};
        var filter     = new HandlerFilter(new ConfigFilterModel {Analogs = analogs});
        var analogType = TmNativeDefs.TmDataTypes.Analog;
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) analogType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(ch, rtu, point);
        });

        var result = filter.IsEventSuitable(tmEvent);

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsFalseWithStatusesAnalogsFilterAndZeroAddr()
      {
        var statuses = new List<string> {"10:1:2..4", "10:1:6"};
        var analogs  = new List<string> {"20:3:2..4", "20:3:6"};
        var filter   = new HandlerFilter(new ConfigFilterModel {Statuses = statuses, Analogs = analogs});
        var tmEvent = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = 0;
          dto.Tma    = 0;
        });

        var result = filter.IsEventSuitable(tmEvent);

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseWithStatusesAnalogsFilterAndOverlap()
      {
        var statuses   = new List<string> {"10:1:2..4", "10:1:6"};
        var analogs    = new List<string> {"20:3:2..4", "20:3:6"};
        var filter     = new HandlerFilter(new ConfigFilterModel {Statuses = statuses, Analogs = analogs});
        var statusType = TmNativeDefs.TmDataTypes.Status;
        var analogType = TmNativeDefs.TmDataTypes.Analog;
        var tmEventStatusFalse = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) statusType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(20, 3, 6);
        });
        var tmEventStatusTrue = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) statusType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(10, 1, 6);
        });
        var tmEventAnalogFalse = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) analogType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(10, 1, 6);
        });
        var tmEventAnalogTrue = TmEventUtil.CreateRandomValidTmEvent(dto =>
        {
          dto.TmType = (short) analogType;
          dto.Tma    = (int) TmAddr.EncodeComplexInteger(20, 3, 6);
        });

        filter.IsEventSuitable(tmEventStatusFalse).Should().BeFalse();
        filter.IsEventSuitable(tmEventStatusTrue).Should().BeTrue();
        filter.IsEventSuitable(tmEventAnalogFalse).Should().BeFalse();
        filter.IsEventSuitable(tmEventAnalogTrue).Should().BeTrue();
      }
    }
  }
}