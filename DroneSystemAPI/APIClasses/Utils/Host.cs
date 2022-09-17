namespace DroneSystemAPI.APIClasses.Utils
{
    public class AkkaProtocol
    {
        public static readonly AkkaProtocol TCP = new ("akka.tcp");
        public static readonly AkkaProtocol UDP = new ("akka.udp");
        public static readonly AkkaProtocol LOCAL = new ("akka");

        public string ProtocolName { get; }

        private AkkaProtocol(string protocolName)
        {
            ProtocolName = protocolName;
        }

        public override string ToString()
        {
            return ProtocolName;
        } 
    }
    
    public class Host
    {
        public string HostName { get; }
        public int Port { get; }
        public AkkaProtocol Protocol { get; } = AkkaProtocol.TCP;

        public Host(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        public Host(string hostName, int port, AkkaProtocol protocol)
        {
            HostName = hostName;
            Port = port;
            Protocol = protocol;
        }

        public virtual string GetSystemAddress(string systemName)
        {
            return $"{Protocol}://{systemName}@{HostName}:{Port}";
        }

        public string GetActorAddress(string systemName, string actorName, string actorSpace = "user")
        {
            return $"{GetSystemAddress(systemName)}/{actorSpace}/{actorName}"; 
        }

        public static Host GetTestHost() => new TestHost();
    }

    internal class TestHost : Host
    {
        public TestHost() : base("", -1, AkkaProtocol.LOCAL)
        {
        }

        public override string GetSystemAddress(string systemName)
        {
            return $"{Protocol}://{systemName}";
        }
    }
}
