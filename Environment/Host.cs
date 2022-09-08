using Akka.Actor;

namespace Environment
{
    public abstract class Host
    {
        protected string HostName { get; private set; }
        protected int Port { get; private set; }

        protected Host(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        public abstract Address Parse();

        protected Address Parse(string protocol)
        {
            return Address.Parse($"akka.{protocol}://{Actors.Constants.ActorSystemName}@{HostName}:{Port}");
        }
    }

    public class TcpHost : Host
    {
        public TcpHost(string hostName, int port) : base(hostName, port)
        {
        }

        public override Address Parse()
        {
            return Parse("tcp");
        }
    }

    public class UdpHost : Host
    {
        public UdpHost(string hostName, int port) : base(hostName, port)
        {
        }

        public override Address Parse()
        {
            return Parse("udp");
        }

    }
}
