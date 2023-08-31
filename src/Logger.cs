using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using L0gg3r.Base;

namespace L0gg3r;

public sealed class Logger : IAsyncDisposable
{
    private readonly LogMessagePipeline logMessagePipeline;

    private readonly IDisposable parentLoggerOutputHandlerDisposer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    public Logger()
        : this(Assembly.GetCallingAssembly().GetName().Name ?? "Logger")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="Logger"/>.</param>
    public Logger(string name)
    {
        Name = name;

        logMessagePipeline = new()
        {
            Transform = TransformLogMessage,
        };
    }

    internal Logger(string name, Logger parentLogger)
        : this(name)
    {
        ParentLogger = parentLogger;

        parentLoggerOutputHandlerDisposer = logMessagePipeline.AttachOutputHandler(logMessage =>
        {
            parentLogger.Log(logMessage);

            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Gets the name of the logger.
    /// </summary>
    public string Name { get; }

    public Logger ParentLogger { get; }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        parentLoggerOutputHandlerDisposer.Dispose();

        await logMessagePipeline.DisposeAsync().ConfigureAwait(false);
    }

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
        };

        Log(logMessage);
    }

    public Task FlushAsync() => logMessagePipeline.FlushAsync();

    public IDisposable AttachLogSink(ILogSink logSink)
    {
        return logMessagePipeline.AttachOutputHandler(logMessage => logSink.ProcessAsync(logMessage));
    }

    public Logger GetChildLogger(string name) => new Logger(name, this);

    internal void Log(LogMessage logMessage) => logMessagePipeline.Write(logMessage);

    internal void Flush() => FlushAsync().GetAwaiter().GetResult();

    private LogMessage TransformLogMessage(LogMessage logMessage)
    {
        int senderCount = logMessage.Senders.Count;

        string[] senders = new string[senderCount + 1];

        Array.Copy(logMessage.Senders.ToArray(), senders, logMessage.Senders.Count);

        senders[senderCount] = Name;

        return logMessage with
        {
            Senders = senders,
        };
    }
}
