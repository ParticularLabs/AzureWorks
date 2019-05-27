using Demo.ASQ.Sagas.Shipping;
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
        [FunctionName("shippingApi-subscribe")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "shipping/subscribe")] HttpRequest req,
             ILogger logger,
             ExecutionContext context)
        {
            //workaround to start and endpoint instance so that it can subscribe
            await ShippingFunctionsHost.Endpoint.Value.Subscribe(logger, context);
                

            return new AcceptedResult();
        }
    }
}