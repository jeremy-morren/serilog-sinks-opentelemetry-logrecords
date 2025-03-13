using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetryLogs.Formatting;
using Serilog.Sinks.OpenTelemetryLogs.Logs;
using Serilog.Sinks.OpenTelemetryLogs.Properties;

namespace Serilog.Sinks.OpenTelemetryLogs;

/// <summary>
/// A log event sink that writes <see cref="LogRecord"/>
/// to processor registered with <see cref="OpenTelemetryLoggerOptions"/>
/// </summary>
internal class OpenTelemetryLogRecordEventSink : ILogEventSink
{
    private readonly ILogRecordPool _logRecordPool;
    private readonly IFormatProvider? _formatProvider;
    private readonly LogEventPropertySerializer _propertySerializer;

    public OpenTelemetryLogRecordEventSink(
        ILogRecordPool logRecordPool,
        IFormatProvider? formatProvider = null)
    {
        _logRecordPool = logRecordPool;
        _formatProvider = formatProvider;
        _propertySerializer = new LogEventPropertySerializer(formatProvider);
    }

    public void Emit(LogEvent logEvent)
    {
        var record = _logRecordPool.Rent();
        record.Timestamp = logEvent.Timestamp.UtcDateTime;
        record.LogLevel = logEvent.Level.ToLogLevel();
        record.TraceId = logEvent.TraceId ?? default;
        record.SpanId = logEvent.SpanId ?? default;

        record.CategoryName = logEvent.GetSourceContext();
        record.EventId = logEvent.GetEventId() ?? default;

        record.Body = logEvent.MessageTemplate.Text;
        record.FormattedMessage = CleanMessageTemplateFormatter.Format(logEvent.MessageTemplate, logEvent.Properties, _formatProvider);

        record.Attributes = new PropertiesListWrapper(logEvent.Properties, _propertySerializer);
        record.Exception = logEvent.Exception;

        record.State = null;

        //TODO: Acquire these values somehow
        record.TraceFlags = ActivityTraceFlags.Recorded;
        record.TraceState = null; //TODO: Acquire this somehow

    }
}