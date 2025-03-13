using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetryLogs.Formatting;

namespace Serilog.Sinks.OpenTelemetryLogs.Tests;

public class LogEventHelpersTests
{
    [Fact]
    public void GetEventId()
    {
        var events = new List<LogEvent>();

        var services = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(new LoggerConfiguration()
                .WriteTo.Sink(new DelegateSink(events.Add))
                .CreateLogger()))
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILogger<LogEventHelpersTests>>();
        logger.LogInformation(new EventId(1), "1");
        logger.LogInformation(new EventId(2, "event2"), "2");
        logger.LogInformation("3");

        events.Should().HaveCount(3);
        events[0].GetEventId().Should().Be(new EventId(1));
        events[1].GetEventId().Should().Be(new EventId(2, "event2"));
        events[2].GetEventId().Should().Be(null);
    }

    [Fact]
    public void GetSourceContext()
    {
        var events = new List<LogEvent>();

        var logger = new LoggerConfiguration()
            .WriteTo.Sink(new DelegateSink(events.Add))
            .CreateLogger();

        var services = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(logger))
            .BuildServiceProvider();

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        // Microsoft.Extensions.Logging tests
        loggerFactory.CreateLogger<LogEventHelpersTests>().LogInformation("0");
        loggerFactory.CreateLogger("Category1").LogInformation("1");
        loggerFactory.CreateLogger(typeof(CleanMessageTemplateFormatter)).LogInformation("2");

        // Serilog tests
        logger.Information("3");
        logger.ForContext<LogEventHelpersTests>().Warning("4");

        events.Should().HaveCount(5);
        events[0].GetSourceContext().Should().Be(typeof(LogEventHelpersTests).FullName);
        events[1].GetSourceContext().Should().Be("Category1");
        events[2].GetSourceContext().Should().Be(typeof(CleanMessageTemplateFormatter).FullName);
        events[3].GetSourceContext().Should().Be(null);
        events[4].GetSourceContext().Should().Be(typeof(LogEventHelpersTests).FullName);
    }

    private class DelegateSink : ILogEventSink
    {
        private readonly Action<LogEvent> _action;

        public DelegateSink(Action<LogEvent> action)
        {
            _action = action;
        }

        public void Emit(LogEvent logEvent)
        {
            _action(logEvent);
        }
    }
}