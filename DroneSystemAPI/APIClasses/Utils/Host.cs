namespace DroneSystemAPI.APIClasses.Utils
{
    public class AkkaProtocol
    {
        public static readonly AkkaProtocol TCP = new AkkaProtocol("akka.tcp");
        public static readonly AkkaProtocol UDP = new AkkaProtocol("akka.udp");
        public static readonly AkkaProtocol LOCAL = new AkkaProtocol("akka");

        public String ProtocolName { get; }

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
        public String HostName { get; }
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

        public virtual String GetSystemAddress(String systemName)
        {
            return $"{Protocol}://{systemName}@{HostName}:{Port}";
        }

        public String GetActorAddress(String systemName, String actorName, String actorSpace = "user")
        {
            return $"{GetSystemAddress(systemName)}/{actorSpace}/actorName"; 
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
