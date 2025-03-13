using System.Diagnostics;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs.Properties;

/// <summary>
/// Converts serilog values to CLR types (Dictionary/List)
/// </summary>
internal static class LogEventPropertyConverter
{
    public static Dictionary<string, object?> ConvertToDictionary(StructureValue value)
    {
        // capacity: +1 for the type tag
        var dictionary = new Dictionary<string, object?>(capacity: value.Properties.Count + 1);
        if (value.TypeTag != null)
            dictionary["$type"] = value.TypeTag;
        foreach (var property in value.Properties)
            dictionary[property.Name] = ConvertToClrType(property.Value);
        return dictionary;
    }

    public static List<object?> ConvertToList(SequenceValue value)
    {
        return value.Elements.Select(ConvertToClrType).ToList();
    }

    public static object? ConvertToClrType(LogEventPropertyValue? value)
    {
        switch (value)
        {
            case null:
                return null;

            case ScalarValue scalar:
                return scalar.Value;

            case StructureValue structure:
                return ConvertToDictionary(structure);

            case SequenceValue sequence:
                return ConvertToList(sequence);

            default:
                Debug.Fail($"Unknown LogEventPropertyValue type {value.GetType()}");
                return null;
        }
    }
}