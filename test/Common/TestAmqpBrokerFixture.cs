namespace Test.Microsoft.Azure.Amqp
{
    using System;
    using TestAmqpBroker;

    public class TestAmqpBrokerFixture : IDisposable
    {
        const string address = "amqp://localhost:15672";
        const string wsAddress = "ws://localhost:15678";
        readonly static object syncRoot = new object();
        static TestAmqpBroker broker;
        static int refs;

        public static Uri Address = new Uri(address);

        public static Uri WsAddress = new Uri(wsAddress);

        static TestAmqpBrokerFixture()
        {
        }

        public TestAmqpBrokerFixture()
        {
            lock (syncRoot)
            {
                if (++refs == 1)
                {
                    broker = new TestAmqpBroker(new string[] { address, wsAddress }, "guest:guest", null, null);
                    broker.Start();
                }
            }
        }

        public TestAmqpBroker Broker => broker;

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (--refs == 0)
                {
                    broker.Stop();
                    broker = null;
                }
            }
        }
    }
}
