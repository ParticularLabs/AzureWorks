using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus.AzureFuntions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Demo.ASQ
{
    //TODO: create a collector transport?
    public static class HttpEmittingMessagesToAsq
    {
        static HttpEmittingMessagesToAsq()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(PlaceOrder), endpointName); //route to our self just to demo
        }
    
        //TODO: Demos:
        //  Show a saga
        //  Show outbox deduplication via http header
        //  Show error queue usage to not loose http requests
        //  Show exposing a "http api" that does "1 to many"
        [FunctionName("asq-placeOrder")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
             ILogger logger,
             ExecutionContext context)
        {
            await endpoint.Send(new PlaceOrder(), logger, context);

            return new AcceptedResult();
        }

        static FunctionsAwareStorageQueueEndpoint endpoint;

        const string endpointName = "sales";
    }
}