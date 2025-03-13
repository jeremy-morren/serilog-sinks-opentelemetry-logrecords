using FluentAssertions;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenTelemetryLogs.Formatting;

namespace Serilog.Sinks.OpenTelemetryLogs.Tests;

/// <summary>
/// See https://github.com/serilog/serilog-sinks-opentelemetry/blob/ffbd2327e8cc3bb33eca3c593c9ea48546ba6260/test/Serilog.Sinks.OpenTelemetry.Tests/CleanMessageTemplateFormatterTests.cs
/// </summary>
public class CleanMessageTemplateFormatterTests
{
    [Fact]
    public void FormatsEmbeddedStringsWithoutQuoting()
    {
        var template = new MessageTemplateParser().Parse("Hello, {Name}!");
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Name"] = new ScalarValue("world")
        };

        var actual = CleanMessageTemplateFormatter.Format(template, properties, null);

        // The default formatter would produce "Hello, \"world\"!" here.
        actual.Should().Be("Hello, world!");
    }

    [Fact]
    public void FormatsEmbeddedStructuresAsJson()
    {
        var template = new MessageTemplateParser().Parse("Received {Payload}");
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["Payload"] = new StructureValue([
                // Particulars of the JSON structure are unimportant, this is handed of to Serilog's default
                // JSON value formatter.
                new LogEventProperty("a", new ScalarValue(42))
            ])
        };

        var actual = CleanMessageTemplateFormatter.Format(template, properties, null);

        // The default formatter would produce "Received {a = 42}" here.
        actual.Should().Be("Received {\"a\":42}");
    }
}