using NServiceBus;

namespace Demo.ASQ.Sagas.Sales
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using NServiceBus.AzureFuntions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Demo.ASQ.Sagas.Messages;

    public static class ShippingFunctionsApi
    {
        static ShippingFunctionsApi()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            endpoint.Routing.RegisterPublisher(typeof(OrderBooked), "booking");
            endpoint.Routing.RegisterPublisher(typeof(OrderBilled), "billing");
        }
    
        [FunctionName("shippingApi-subscribe")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "shipping/subscribe")] HttpRequest req,
             ILogger logger,
             ExecutionContext context)
        {
            //workaround to start and endpoint instance so that it can subscribe
            await endpoint.Subscribe(logger, context);
                

            return new AcceptedResult();
        }

        static FunctionsAwareStorageQueueEndpoint endpoint;

        const string endpointName = "shipping";
    }
}