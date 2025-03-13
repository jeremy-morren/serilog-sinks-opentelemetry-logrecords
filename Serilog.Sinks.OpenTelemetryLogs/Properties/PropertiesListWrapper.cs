using System.Collections;
using System.Diagnostics;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs.Properties;

/// <summary>
/// Implementation of IReadOnlyList that wraps a log event properties dictionary
/// </summary>
internal class PropertiesListWrapper : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly IReadOnlyDictionary<string, LogEventPropertyValue> _properties;
    private readonly LogEventPropertySerializer _serializer;

    public PropertiesListWrapper(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        LogEventPropertySerializer serializer)
    {
        _properties = properties;
        _serializer = serializer;
    }

    /// <summary>
    /// Convert a log event property value to a publicly usable object
    /// </summary>
    /// <remarks>
    /// LogRecord processors obviously don't know about Serilog's internal property value types, so we need to convert them
    /// </remarks>
    private object? Convert(LogEventPropertyValue? value)
    {
        switch (value)
        {
            case ScalarValue sv:
                return sv.Value;
            case StructureValue sv:
                return new StructureValueWrapper(sv, _serializer);
            case SequenceValue sv:
                return new SequenceValueWrapper(sv, _serializer);
            case null:
                return null;
            default:
                Debug.Fail($"Unexpected property value type {value.GetType()}");
                return null;
        }
    }

    private List<KeyValuePair<string, object?>>? _list;

    private List<KeyValuePair<string, object?>> List => _list ??=
        _properties.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, Convert(kvp.Value))).ToList();

    public int Count => _properties.Count;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public KeyValuePair<string, object?> this[int index] => List[index];
}