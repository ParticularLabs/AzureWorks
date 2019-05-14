using Microsoft.Azure.WebJobs;
using NServiceBus;

namespace AsqFunctionApp
{
    static class MessageContextExtensions
    {
        public static IAsyncCollector<T> GetAsyncCollector<T>(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<T>>();
        }
    }
}