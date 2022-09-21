using Akka.Configuration;

namespace DroneSystemAPI
{
    public abstract class SystemConfigs
    {
        public abstract string SystemName { get; set; }
        public abstract string ActorName { get; set; }
        public virtual string HostName { get; set; } = "localhost";
        public abstract int Port { get; set; }
        protected virtual string HoconConfig { get; set; } = @"
akka {
    loglevel = WARNING
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = {0}
            hostname = {1}
        }
    }
}";

        public Config Config
        {
            get
            {
                return ConfigurationFactory.ParseString(HoconConfig);
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

}
