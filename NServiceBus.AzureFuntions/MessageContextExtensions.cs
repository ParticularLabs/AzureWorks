using Microsoft.Azure.WebJobs;

namespace NServiceBus
{
    public static class MessageContextExtensions
    {
        public static IAsyncCollector<T> GetAsyncCollector<T>(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<T>>();
        }
    }
}