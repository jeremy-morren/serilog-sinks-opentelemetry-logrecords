using System.Collections;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs.Properties;

/// <summary>
/// Wraps <see cref="SequenceValue"/>
/// </summary>
internal class SequenceValueWrapper : IReadOnlyList<object?>
{
    private readonly SequenceValue _value;
    private readonly LogEventPropertySerializer _serializer;

    public SequenceValueWrapper(SequenceValue value, LogEventPropertySerializer serializer)
    {
        _value = value;
        _serializer = serializer;
    }

    /// <summary>
    /// <see cref="StructureValueWrapper.ToString"/>
    /// </summary>
    public override string? ToString() => _serializer.Serialize(_value);

    public int Count => _value.Elements.Count;
    private IEnumerable<object?> Elements => _value.Elements.Select(LogEventPropertyConverter.ConvertToClrType);

    public IEnumerator<object?> GetEnumerator() => Elements.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();

    public object? this[int index] => LogEventPropertyConverter.ConvertToClrType(_value.Elements[index]);
}