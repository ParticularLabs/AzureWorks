using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using NServiceBus;

namespace FunctionApp
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;


    //TODO: create a collector transport?
    public static class HttpConnectedToAsbFunction
    {
        static HttpConnectedToAsbFunction()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName, connectionStringName);

            endpoint.Routing.RouteToEndpoint(typeof(PlaceOrder), endpointName); //route to our self just to demo
        }

        [FunctionName("placeOrder")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
             ILogger logger,
             ExecutionContext context)
        {
            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            await endpoint.Send(new PlaceOrder(), logger, context);

            return (ActionResult)new AcceptedResult();
        }

        static FunctionsAwareServiceBusEndpoint endpoint;

        const string endpointName = "sales";
        const string connectionStringName = "my-sb-connstring";
    }
}