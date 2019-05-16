using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace FunctionApp
{
    public static class GenericFunctionNotTriggeredByATransport
    {

        static GenericFunctionNotTriggeredByATransport()
        {
            endpoint = new FunctionsAwareServiceBusEndpointNoTransport(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), endpointName); //route to our self just to demo
        }

        //post <PlaceOrder/> to have that "message" be invoked
        [FunctionName(endpointName)] //this is the "one function to all many handler for different messages"
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)]
            HttpRequest request,
            ILogger logger,
            ExecutionContext context)
        {
            await endpoint.Invoke(request, logger, null, context);

            return new AcceptedResult();
        }

        const string endpointName = "sales-http";

        static readonly FunctionsAwareServiceBusEndpointNoTransport endpoint;
    }
}