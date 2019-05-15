using Microsoft.Azure.WebJobs;

namespace NServiceBus
{
    static class MessageContextExtensions
    {
        public static IAsyncCollector<string> GetAsyncCollector(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<string>>();
        }
    }
}