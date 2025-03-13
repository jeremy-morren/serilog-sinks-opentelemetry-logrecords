using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Serilog.Sinks.OpenTelemetryLogs;

internal static class LogEventHelpers
{
    /// <summary>
    /// Convert a <see cref="LogEventLevel"/> to a <see cref="LogLevel"/>
    /// </summary>
    public static LogLevel ToLogLevel(this LogEventLevel logEventLevel) =>
        logEventLevel switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(logEventLevel), logEventLevel, null)
        };

    /// <summary>
    /// Get the <see cref="EventId"/> from the <see cref="LogEvent"/>
    /// </summary>
    public static EventId? GetEventId(this LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("EventId", out var property)
            && property is StructureValue obj
            && obj.Properties.FirstOrDefault(p => p.Name == "Id") is { Value: ScalarValue { Value: int id } })
        {
            var nameProperty = obj.Properties.FirstOrDefault(p => p.Name == "Name");
            var name = (nameProperty?.Value as ScalarValue)?.Value as string;
            return new EventId(id, name);
        }

        return null;
    }

    /// <summary>
    /// Get the source context from the <see cref="LogEvent"/>
    /// </summary>
    public static string? GetSourceContext(this LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var property)
            && property is ScalarValue { Value: string sourceContext })
        {
            return sourceContext;
        }

        return null;
    }
}