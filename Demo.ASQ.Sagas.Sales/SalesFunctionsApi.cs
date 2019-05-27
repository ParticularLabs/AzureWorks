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

    public static class SalesFunctionsApi
    {
        [FunctionName("salesApi-placeOrder")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sales/placeOrder/{orderId}")] HttpRequest req,
             string orderId,
             ILogger logger,
             ExecutionContext context)
        {
            await SalesFunctionsHost.Endpoint.Value.Send(new PlaceOrder()
            {
                OrderId = orderId
            }, logger, context);

            return new AcceptedResult();
        }
    }
}