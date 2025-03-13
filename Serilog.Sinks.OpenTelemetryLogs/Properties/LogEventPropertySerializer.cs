using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs.Properties;

/// <summary>
/// Serializes <see cref="LogEventPropertyValue"/> to a string
/// </summary>
internal class LogEventPropertySerializer
{
    private readonly IFormatProvider? _formatProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public LogEventPropertySerializer(IFormatProvider? formatProvider)
    {
        _formatProvider = formatProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Converters = { new LogEventPropertyValueConverter(this) }
        };
    }

    /// <summary>
    /// Serialize the structure value to a string
    /// </summary>
    public string? Serialize(StructureValue structure) => JsonSerializer.Serialize(structure, _jsonOptions);

    /// <summary>
    /// Serialize the sequence value to a string
    /// </summary>
    public string? Serialize(SequenceValue sequence) => JsonSerializer.Serialize(sequence, _jsonOptions);

    private void WriteValue(Utf8JsonWriter writer, LogEventPropertyValue? propertyValue)
    {
        switch (propertyValue)
        {
            case ScalarValue scalar:
                WriteScalar(writer, scalar.Value);
                break;

            case StructureValue structure:
                writer.WriteStartObject();
                foreach (var property in structure.Properties)
                {
                    writer.WritePropertyName(property.Name);
                    WriteValue(writer, property.Value);
                }
                writer.WriteEndObject();
                break;

            case SequenceValue sequence:
                writer.WriteStartArray();
                foreach (var element in sequence.Elements)
                    WriteValue(writer, element);
                writer.WriteEndArray();
                break;

            case null:
                writer.WriteNullValue();
                break;

            default:
                Debug.Fail($"Unknown property value type {propertyValue!.GetType()}");
                break;
        }
    }

    /// <summary>
    /// Writes a scalar value to the writer
    /// </summary>
    private void WriteScalar(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;

            case bool b:
                writer.WriteBooleanValue(b);
                break;

            case int i:
                writer.WriteNumberValue(i);
                break;
            case uint i:
                writer.WriteNumberValue(i);
                break;
            case ulong l:
                writer.WriteNumberValue(l);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;

            case float f:
                writer.WriteNumberValue(f);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;

            case decimal d:
                writer.WriteNumberValue(d);
                break;

            case DateTime dateTime:
                writer.WriteStringValue(dateTime);
                break;
            case DateTimeOffset dateTimeOffset:
                writer.WriteStringValue(dateTimeOffset);
                break;

            case Guid guid:
                writer.WriteStringValue(guid);
                break;

            case ReadOnlyMemory<byte> memory:
                writer.WriteBase64StringValue(memory.Span);
                break;
            case byte[] bytes:
                writer.WriteBase64StringValue(bytes);
                break;

            // Handled all primitives, try IFormattable
            case IFormattable formattable:
                writer.WriteStringValue(formattable.ToString(null, _formatProvider));
                break;

            // Unknown scalar type, just write ToString()
            default:
                var str = value?.ToString();
                if (str == null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(str);
                break;
        }
    }

    private class LogEventPropertyValueConverter(LogEventPropertySerializer serializer) : JsonConverter<LogEventPropertyValue>
    {
        public override void Write(Utf8JsonWriter writer, LogEventPropertyValue value, JsonSerializerOptions options)
        {
            serializer.WriteValue(writer, value);
        }

        public override LogEventPropertyValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
}