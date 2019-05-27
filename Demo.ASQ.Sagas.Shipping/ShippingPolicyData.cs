using NServiceBus;

namespace Demo.ASQ.Sagas.Shipping
{
    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }
        public bool OrderBooked { get; set; }
        public bool OrderBilled { get; set; }
    }
}