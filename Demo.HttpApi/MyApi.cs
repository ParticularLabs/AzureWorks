using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Unicast.Subscriptions;

namespace Demo.HttpApi
{
    public static class MyApi
    {
        static MyApi()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName);

            endpoint.EnablePassThroughRoutingForUnknownMessages(messageType =>
            {
                //route everything to a backend function
                return "my-api-backend";
            });
        }

        [FunctionName(endpointName)] //this is the "one function to all many handler for different messages"
        public static async Task<IActionResult> ApiInputCatchAll(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{messagetype}")]
            HttpRequest request,
            string messagetype,
            ILogger logger,
            ExecutionContext context)
        {
            await endpoint.Invoke(request, messagetype, logger, null, context);

            return new AcceptedResult();
        }

        //do not use messaging for queries
        [FunctionName("GetProducts")]
        public static IActionResult GetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")]HttpRequest request)
        {
           return new ContentResult
           {
               Content = "list of products"
           };
        }

        //do not use messaging for queries
        [FunctionName("GetOrders")]
        public static IActionResult GetOrders(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")]HttpRequest request)
        {
            return new ContentResult
            {
                Content = "list of orders"
            };
        }

        const string endpointName = "my-api";

        static readonly FunctionsAwareServiceBusEndpoint endpoint;
    }
}

class SomeMethod
{
}

class SomeOtherMethod
{
}

class SomeMethodHandler:IHandleMessages<SomeMethod>
{
    public Task Handle(SomeMethod message, IMessageHandlerContext context)
    {
        logger.Info("SomeMethod called");

        return context.SendLocal(new SomeMessage());
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}

class SomeMessage
{
}

class SomeOtherMethodHandler : IHandleMessages<SomeOtherMethod>
{
    public Task Handle(SomeOtherMethod message, IMessageHandlerContext context)
    {
        logger.Info("SomeOtherMethod called");
        return Task.CompletedTask;
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}


class SomeMessageHandler : IHandleMessages<SomeMessage>
{
    public Task Handle(SomeMessage message, IMessageHandlerContext context)
    {
        logger.Info("SomeMessage called");
        return Task.CompletedTask;
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}