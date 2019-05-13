using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AsqFunctionApp
{
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Queue;

    public static class Sales
    {
        [FunctionName("Sales")]
        public static Task Run([QueueTrigger("sales", Connection = "")]CloudQueueMessage message, ILogger log)
        {
            log.LogInformation($"Incoming message: {message.Id}");

            return Task.CompletedTask;
        }
    }
}
