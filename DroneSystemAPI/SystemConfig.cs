using Akka.Configuration;

namespace DroneSystemAPI
{
    [Obsolete("Not used any more", true)]
    public abstract class SystemConfigs
    {
        public virtual string SystemName { get; set; } = "DroneDeliverySystem";
        public abstract string ActorName { get; set; }

        public string SpawnerActor { get; } = "spawner";

        public virtual string HostName { get; set; } = "localhost";
        public virtual int Port { get; set; } = 0;

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

        public static SystemConfigs GenericConfig
        {
            get
            {
                return new GenericConfig();
            }
        }
    }

    [Obsolete("Not used any more", true)]
    internal class DroneSystemConfig : SystemConfigs
    {
        public override string SystemName { get; set; } = "DroneActorSystem";
        public override string ActorName { get; set; } = "Drone";
    }

    [Obsolete("Not used any more", true)]
    internal class DroneRepositoryConfig : SystemConfigs
    {
        public override string SystemName { get; set; } = "RepositoryActorSystem";
        public override string ActorName { get; set; } = "Repository";
        public override int Port { get; set; } = 8080;
    }

    [Obsolete("Not used any more", true)]
    internal class GenericConfig : SystemConfigs
    {
        public override string ActorName { get; set; } = string.Empty;
    }
}
