using Serilog.Parsing;

namespace Serilog.Sinks.OpenTelemetryLogs.Formatting;

/// <summary>
/// See https://github.com/serilog/serilog-sinks-opentelemetry/blob/ffbd2327e8cc3bb33eca3c593c9ea48546ba6260/src/Serilog.Sinks.OpenTelemetry/Sinks/OpenTelemetry/Formatting/Padding.cs
/// </summary>
internal static class Padding
{
    static readonly char[] PaddingChars = Enumerable.Repeat(' ', 80).ToArray();

    /// <summary>
    /// Writes the provided value to the output, applying direction-based padding when <paramref name="alignment"/> is provided.
    /// </summary>
    public static void Apply(TextWriter output, string value, Alignment alignment)
    {
        if (value.Length >= alignment.Width)
        {
            output.Write(value);
            return;
        }

        var pad = alignment.Width - value.Length;

        if (alignment.Direction == AlignmentDirection.Left)
            output.Write(value);

        if (pad <= PaddingChars.Length)
        {
            output.Write(PaddingChars, 0, pad);
        }
        else
        {
            output.Write(new string(' ', pad));
        }

        if (alignment.Direction == AlignmentDirection.Right)
            output.Write(value);
    }
}