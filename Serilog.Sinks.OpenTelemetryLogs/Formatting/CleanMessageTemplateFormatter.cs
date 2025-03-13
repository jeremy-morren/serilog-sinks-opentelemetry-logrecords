using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace Serilog.Sinks.OpenTelemetryLogs.Formatting;

/// <summary>
/// Matches the `:lj` clean formatting style now employed by Serilog.Expressions, Serilog.Sinks.Console, and elsewhere.
/// In this mode, strings embedded in message templates are unquoted, and structured data is rendered as JSON.
/// </summary>
/// <remarks>
/// <para>This implementation is derived from the Serilog.Expressions one, sans theming support, and avoiding the
/// extra dependency. In time there should be core Serilog support for this.
/// </para>
/// <para>
/// See https://github.com/serilog/serilog-sinks-opentelemetry/blob/ffbd2327e8cc3bb33eca3c593c9ea48546ba6260/src/Serilog.Sinks.OpenTelemetry/Sinks/OpenTelemetry/Formatting/CleanMessageTemplateFormatter.cs
/// </para>
/// </remarks>
internal static class CleanMessageTemplateFormatter
{
    private static readonly JsonValueFormatter SharedJsonValueFormatter = new("$type");

    public static string Format(MessageTemplate messageTemplate, IReadOnlyDictionary<string, LogEventPropertyValue> properties, IFormatProvider? formatProvider)
    {
        var output = new StringWriter();

        foreach (var token in messageTemplate.Tokens)
        {
            switch (token)
            {
                case TextToken tt:
                    {
                        output.Write(tt.Text);
                        break;
                    }
                case PropertyToken pt:
                    {
                        RenderPropertyToken(properties, pt, output, formatProvider);
                        break;
                    }
                default:
                    {
                        output.Write(token);
                        break;
                    }
            }
        }

        return output.ToString();
    }

    static void RenderPropertyToken(IReadOnlyDictionary<string, LogEventPropertyValue> properties, PropertyToken pt, TextWriter output, IFormatProvider? formatProvider)
    {
        if (!properties.TryGetValue(pt.PropertyName, out var value))
        {
            output.Write(pt.ToString());
            return;
        }

        if (pt.Alignment is null)
        {
            RenderPropertyValueUnaligned(value, output, pt.Format, formatProvider);
            return;
        }

        var buffer = new StringWriter();

        RenderPropertyValueUnaligned(value, buffer, pt.Format, formatProvider);

        var result = buffer.ToString();

        if (result.Length >= pt.Alignment.Value.Width)
            output.Write(result);
        else
            Padding.Apply(output, result, pt.Alignment.Value);
    }

    static void RenderPropertyValueUnaligned(LogEventPropertyValue propertyValue, TextWriter output, string? format, IFormatProvider? formatProvider)
    {
        if (propertyValue is not ScalarValue scalar)
        {
            SharedJsonValueFormatter.Format(propertyValue, output);
            return;
        }

        var value = scalar.Value;

        if (value == null)
        {
            output.Write("null");
            return;
        }

        if (value is string str)
        {
            output.Write(str);
            return;
        }

        if (value is ValueType)
        {
            if (value is int or uint or long or ulong or decimal or byte or sbyte or short or ushort)
            {
                output.Write(((IFormattable)value).ToString(format, formatProvider));
                return;
            }

            if (value is double d)
            {
                output.Write(d.ToString(format, formatProvider));
                return;
            }

            if (value is float f)
            {
                output.Write(f.ToString(format, formatProvider));
                return;
            }

            if (value is bool b)
            {
                output.Write(b);
                return;
            }
        }

        if (value is IFormattable formattable)
        {
            output.Write(formattable.ToString(format, formatProvider));
            return;
        }

        output.Write(value);
    }
}