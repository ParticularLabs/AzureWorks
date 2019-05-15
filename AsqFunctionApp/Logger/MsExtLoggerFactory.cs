using System;
using Microsoft.Extensions.Logging;
using NServiceBus.Logging;
using ILoggerFactory = NServiceBus.Logging.ILoggerFactory;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace NServiceBus
{
    public class MsExtLoggerFactory : ILoggerFactory
    {
        private readonly ILogger logger;

        public MsExtLoggerFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public ILog GetLogger(Type type)
        {
            return new MsExtLogger(logger, type);
        }

        public ILog GetLogger(string name)
        {
            return new MsExtLogger(logger, name);
        }
    }

    public class MsExtLogger : ILog
    {
        private readonly ILogger logger;

        public MsExtLogger(ILogger logger, Type type)
        {
            this.logger = logger;
        }

        public MsExtLogger(ILogger logger, string name)
        {
            this.logger = logger;
        }

        public bool IsDebugEnabled => logger.IsEnabled(LogLevel.Debug);
        public bool IsInfoEnabled => logger.IsEnabled(LogLevel.Information);
        public bool IsWarnEnabled => logger.IsEnabled(LogLevel.Warning);
        public bool IsErrorEnabled => logger.IsEnabled(LogLevel.Error);
        public bool IsFatalEnabled => logger.IsEnabled(LogLevel.Critical);
        public void Debug(string message)
        {
            logger.LogDebug(message);
        }

        public void Debug(string message, Exception exception)
        {
            logger.LogDebug(exception, message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            logger.LogDebug(format, args);
        }

        public void Info(string message)
        {
            logger.LogInformation(message);
        }

        public void Info(string message, Exception exception)
        {
            logger.LogInformation(exception, message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            logger.LogInformation(format, args);
        }

        public void Warn(string message)
        {
            logger.LogWarning(message);
        }

        public void Warn(string message, Exception exception)
        {
            logger.LogWarning(exception, message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            logger.LogWarning(format, args);
        }

        public void Error(string message)
        {
            logger.LogError(message);
        }

        public void Error(string message, Exception exception)
        {
            logger.LogError(exception, message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            logger.LogError(format, args);
        }

        public void Fatal(string message)
        {
            logger.LogCritical(message);
        }

        public void Fatal(string message, Exception exception)
        {
            logger.LogCritical(exception, message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            logger.LogCritical(format, args);
        }
    }
}