using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Iface.Oik.EventDispatcher.Util;

namespace Iface.Oik.EventDispatcher;

public sealed class WorkerOptions
{
    private readonly JsonNode? _node;

    public WorkerOptions(JsonNode? node)
    {
        _node = node;
    }

    public T Get<T>()
    {
        if (_node is null)
        {
            throw new Exception("Не заданы настройки");
        }

        return _node.Deserialize<T>(JsonSettings.Options)
            ?? throw new Exception("Не удалось разобрать настройки");
    }
}
