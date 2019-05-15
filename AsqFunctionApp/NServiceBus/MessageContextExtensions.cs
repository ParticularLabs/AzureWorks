using Microsoft.Azure.WebJobs;

namespace NServiceBus
{
    using Microsoft.Extensions.Logging;

    static class MessageContextExtensions
    {
        public static IAsyncCollector<string> GetAsyncCollector(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<string>>();
        }

        public static ILogger GetLogger(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<ILogger>();
        }
    }
}