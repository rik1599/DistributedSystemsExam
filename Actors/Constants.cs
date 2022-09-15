using Akka.Configuration;

namespace Actors
{
    public class Config
    {
        public string ActorSystemName { get; set; } = "DroneActorSystem";

        public string HoconConfig { get; set; } = @"
akka {  
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
        public Akka.Configuration.Config DroneConfig
        {
            get { return ConfigurationFactory.ParseString(HoconConfig); }
        }
    }
}
