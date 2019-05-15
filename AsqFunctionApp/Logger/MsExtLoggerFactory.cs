using System;
using Microsoft.Extensions.Logging;
using NServiceBus.Logging;
using ILoggerFactory = NServiceBus.Logging.ILoggerFactory;

namespace NServiceBus
{
    class MsExtLoggerFactory : ILoggerFactory
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
}