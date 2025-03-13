using System.Collections;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs.Properties;

/// <summary>
/// Wraps a <see cref="StructureValue"/>
/// </summary>
internal class StructureValueWrapper : IReadOnlyDictionary<string, object?>
{
    private readonly StructureValue _value;
    private readonly LogEventPropertySerializer _serializer;

    public StructureValueWrapper(StructureValue value, LogEventPropertySerializer serializer)
    {
        _value = value;
        _serializer = serializer;
    }

    // Delay creation of the dictionary until it is needed.
    private Dictionary<string, object?>? _dictionary;
    private Dictionary<string, object?> Dictionary => _dictionary ??= LogEventPropertyConverter.ConvertToDictionary(_value);

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For exporters that simply use the ToString() method,
    /// we can avoid allocating Dictionary and serialize the value directly.
    /// </para>
    /// <para>
    /// Azure monitor: see https://github.com/Azure/azure-sdk-for-net/blob/57687718299f6f7e3ccf121807e316387438f4bc/sdk/monitor/Azure.Monitor.OpenTelemetry.Exporter/src/Internals/LogsHelper.cs#L114
    /// </para>
    /// </remarks>
    public override string? ToString() => _serializer.Serialize(_value);

    #region Implementation of IReadOnlyDictionary<string,object?>

    public int Count => Dictionary.Count;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(string key) => Dictionary.ContainsKey(key);

    public bool TryGetValue(string key, out object? value) => Dictionary.TryGetValue(key, out value);

    public object? this[string key] => Dictionary[key];

    public IEnumerable<string> Keys => Dictionary.Keys;
    public IEnumerable<object?> Values => Dictionary.Values;

    #endregion
}