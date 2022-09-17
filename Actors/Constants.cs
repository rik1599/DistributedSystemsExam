using Akka.Configuration;

namespace Actors
{
    public class DroneSystemConfig
    {
        public string DroneSystemName { get; set; } = "DroneActorSystem";
        public string DroneActorName { get; set; } = "Drone";

        public string RegisterSystemName { get; set; } = "RegisterActorSystem";
        public string RegisterActorName { get; set; } = "Register";

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
