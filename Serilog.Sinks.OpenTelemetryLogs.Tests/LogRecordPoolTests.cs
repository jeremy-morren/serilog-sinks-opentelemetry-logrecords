using FluentAssertions;
using Serilog.Sinks.OpenTelemetryLogs.Logs;

namespace Serilog.Sinks.OpenTelemetryLogs.Tests;

public class LogRecordPoolTests
{
    [Fact]
    public void SharedPool_Rent_Return()
    {
        LogRecordSharedPool.Current.Should().NotBeNull();

        var record = LogRecordSharedPool.Current.Rent();
        record.Should().NotBeNull();
        LogRecordSharedPool.Current.Return(record);
    }
}