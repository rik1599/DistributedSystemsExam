using Akka.Configuration;

namespace DroneSystemAPI
{
    public abstract class SystemConfigs
    {
        public abstract string SystemName { get; set; }
        public abstract string ActorName { get; set; }
        public virtual string HostName { get; set; } = "localhost";
        public abstract int Port { get; set; }

        public Config Config
        {
            get
            {
                var hocon = @$"
akka {{
    loglevel = WARNING
    actor {{
        provider = remote
    }}
    remote {{
        dot-netty.tcp {{
            port = {Port}
            hostname = {HostName}
        }}
    }}
}}";
                return ConfigurationFactory.ParseString(hocon);
            }
        }

        public static SystemConfigs RepositoryConfig
        {
            get
            {
                return new DroneRepositoryConfig();
            }
        }

        public static SystemConfigs DroneConfig
        {
            get
            {
                return new DroneSystemConfig();
            }
        }

        public static SystemConfigs SimpleConfig(string systemName, string actorName, int port)
            => new SimpleConfig(systemName, actorName, port);
    }

    internal class DroneSystemConfig : SystemConfigs
    {
        public override string SystemName { get; set; } = "DroneActorSystem";
        public override string ActorName { get; set; } = "Drone";
        public override int Port { get; set; } = 0;
    }

    internal class DroneRepositoryConfig : SystemConfigs
    {
        public override string SystemName { get; set; } = "RepositoryActorSystem";
        public override string ActorName { get; set; } = "Repository";
        public override int Port { get; set; } = 8080;
    }

    internal class SimpleConfig : SystemConfigs
    {
        public override string SystemName { get; set; }
        public override string ActorName { get; set; }
        public override int Port { get; set; }

        public SimpleConfig(string systemName, string actorName, int port)
        {
            SystemName = systemName;
            ActorName = actorName;
            Port = port;
        }
    }

}
