using Microsoft.Azure.WebJobs;

namespace NServiceBus
{
    using Microsoft.Extensions.Logging;

    static class MessageContextExtensions
    {
        public static IAsyncCollector<T> GetAsyncCollector<T>(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<T>>();
        }

        public static ILogger GetLogger(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<ILogger>();
        }
    }
}