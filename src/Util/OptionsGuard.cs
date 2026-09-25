using System;
using System.Collections;

namespace Iface.Oik.EventDispatcher.Util;

public static class OptionsGuard
{
    public static void ThrowIfNull(object? value, string name)
    {
        if (value is null)
        {
            throw new Exception($"Не задан обязательный параметр \"{name}\"");
        }
    }

    public static void ThrowIfNullOrEmpty(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Не задан обязательный параметр \"{name}\"");
        }
    }

    public static void ThrowIfEmpty(ICollection? value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new Exception($"Не задан обязательный параметр \"{name}\"");
        }
    }

    public static void ThrowIfZero(int value, string name)
    {
        if (value == 0)
        {
            throw new Exception($"Параметр \"{name}\" не может быть равен 0");
        }
    }

    public static void ThrowIfNotInEnum(Enum? value, string name)
    {
        if (value is null || !Enum.IsDefined(value.GetType(), value))
        {
            throw new Exception($"Недопустимое значение параметра \"{name}\"");
        }
    }
}
