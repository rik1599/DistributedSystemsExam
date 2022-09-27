using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses
{
    public class DeployPointDetails
    {
        public Host Host { get; }
        public string ActorSystemName { get; }

        public DeployPointDetails(Host host, string actorSystemName)
        {
            Host = host;
            ActorSystemName = actorSystemName;
        }
    }
}
