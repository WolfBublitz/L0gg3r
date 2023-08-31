using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using L0gg3r;
using L0gg3r.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerTests.SendersTests;

internal sealed class LogSink : ILogSink
{
    public List<LogMessage> LogMessages { get; } = new();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task FlushAsync() => Task.CompletedTask;

    public Task ProcessAsync(in LogMessage logMessage)
    {
        LogMessages.Add(logMessage);

        return Task.CompletedTask;
    }
}

[TestClass]
public class TheLogger
{
    [TestMethod]
    public async Task ShallAppendItsNameToTheListOfSenders()
    {
        // arrange
        LogSink logSink = new();
        Logger logger = new("TestLogger");


        // act
        using (logger.AttachLogSink(logSink))
        {
            logger.Log(LogLevel.Info, "test");
        }

        // assert
        logSink.LogMessages.Should().HaveCount(1);
        logSink.LogMessages[0].Senders.Should().HaveCount(1);
        logSink.LogMessages[0].Senders.ElementAt(0).Should().Be("TestLogger");
    }
}
