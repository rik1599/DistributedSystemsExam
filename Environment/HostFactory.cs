namespace Environment
{
    public interface IHostFactory
    {
        Host Create(string hostName, int port);
    }

    public class HostFactoryTCP : IHostFactory
    {
        public Host Create(string hostName, int port)
        {
            return new TcpHost(hostName, port);
        }
    }

    public class HostFactoryUDP : IHostFactory
    {
        public Host Create(string hostName, int port)
        {
            return new UdpHost(hostName, port);
        }
    }
}
