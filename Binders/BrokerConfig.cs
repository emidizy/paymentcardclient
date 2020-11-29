using System;
using System.Collections.Generic;
using System.Text;

namespace Reciever.Binders
{
    
    public class BrokerConfig
    {
        public string ServiceName { get; set; }
        public Server Server { get; set; }
        public ClientQueue ClientQueue { get; set; }
    }

    public class Server
    {
        public bool isLocal { get; set; }
        public string IP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Exchange { get; set; }
    }

    public class ClientQueue
    {
        public string QueueId { get; set; }
        public string RoutingKey { get; set; }
        public ushort MaxQueueCount { get; set; }
    }

}
