using Akka.Configuration;

namespace DroneSystemAPI
{
    public abstract class SystemConfigs
    {
        public abstract string SystemName { get; set; }
        public abstract string ActorName { get; set; }
        public abstract string HoconConfig { get; set; }

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

        public override string HoconConfig { get; set; } = @"
akka {
    loglevel = WARNING
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 0
            hostname = localhost
        }
    }
}";
    }

    internal class DroneRepositoryConfig : SystemConfigs
    {
        public override string SystemName { get; set; } = "RepositoryActorSystem";
        public override string ActorName { get; set; } = "Repository";
        public override string HoconConfig { get; set; } = @"
akka {
    loglevel = WARNING
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8080
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
";
    }

}
