using System;
using System.Threading.Tasks;

using L0gg3r.Base;

namespace L0gg3r;

public sealed class Logger : IAsyncDisposable
{
    private readonly LogMessagePipeline logMessagePipeline = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="Logger"/>.</param>
    public Logger(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the logger.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => logMessagePipeline.DisposeAsync();

    /// <summary>
    /// Logs the <paramref name="message"/> with <paramref name="logLevel"/>.
    /// </summary>
    /// <param name="logLevel">The <see cref="LogLevel"/>.</param>
    /// <param name="message">The message.</param>
    public void Log(LogLevel logLevel, object message)
    {
        LogMessage logMessage = new()
        {
            Payload = message,
            LogLevel = logLevel,
            Senders = new string[] { Name },
        };

        logMessagePipeline.Write(logMessage);
    }

    public Task FlushAsync() => logMessagePipeline.FlushAsync();
}
