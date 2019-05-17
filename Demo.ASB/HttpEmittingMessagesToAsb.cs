using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace FunctionApp
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;


    //TODO: create a collector transport?
    public static class HttpEmittingMessagesToAsb
    {
        static HttpEmittingMessagesToAsb()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(PlaceOrder), endpointName); //route to our self just to demo
        }

        //TODO: Demos:
        //  Show a saga
        //  Show outbox deduplication via http header
        //  Show error queue usage to not loose http requests
        //  Show exposing a "http api" that does "1 to many"
        [FunctionName("placeOrder")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
             ILogger logger,
             ExecutionContext context)
        {
            await endpoint.Send(new PlaceOrder(), logger, context);

            return new AcceptedResult();
        }

        static FunctionsAwareServiceBusEndpoint endpoint;

        const string endpointName = "sales";
    }
}