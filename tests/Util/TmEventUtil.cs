using System;
using AutoFixture;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.EventDispatcher.Test.Util
{
  public class TmEventUtil
  {
    public static TmEvent CreateRandomValidTmEvent(Action<TmEventDto> extraDtoAction = null)
    {
      var dto = new Fixture().Create<TmEventDto>();
      FixRandomTmEventDtoToBeValid(dto);
      extraDtoAction?.Invoke(dto);
      return TmEvent.CreateFromDto(dto);
    }
    
    
    private static void FixRandomTmEventDtoToBeValid(TmEventDto dto)
    {
      dto.Elix       = new byte[16];
      dto.Importance = (short) new Random().Next(0, 3);
    }
  }
}