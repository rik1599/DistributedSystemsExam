using Akka.Actor;

namespace Environment.PhisicalHost
{
    /// <summary>
    /// Una rappresentazione di una locazione fisica.
    /// Permette di ricavare un indirizzo a partire da un host 
    /// e da una porta.
    /// </summary>
    public abstract class Host
    {
        /// <summary>
        /// L'indirizzo dell'host
        /// </summary>
        protected string HostName { get; private set; }

        /// <summary>
        /// La porta dell'host (mettere 0 se si vuole dire
        /// che viene scelta automaticamente dal sistema)
        /// </summary>
        protected int Port { get; private set; }

        protected Host(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        /// <summary>
        /// Ricava un indirizzo per il sistema
        /// </summary>
        /// <returns></returns>
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
