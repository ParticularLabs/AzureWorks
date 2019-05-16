namespace FunctionApp
{
    public static class GenericFunctionNotConnectedToATransport

    {
        static GenericFunctionNotConnectedToATransport()
        {
            //endpoint = new FunctionsAwareServiceBusEndpoint(endpointName, connectionStringName);
        }

        //[FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        //public static Task Run([ServiceBusTrigger(endpointName, Connection = connectionStringName)]Message message,
        //    [ServiceBus("some-queue", Connection = "my-sb-connstring")]IAsyncCollector<string> collector,
        //    ILogger logger,
        //    ExecutionContext context)
        //{
        //    return endpoint.Invoke(message, logger, collector, context);
        //}

        //static FunctionsAwareServiceBusEndpoint endpoint;
    }
}