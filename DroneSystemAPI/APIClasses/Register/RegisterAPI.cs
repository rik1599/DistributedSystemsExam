using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Register
{
    public class RepositoryAPI
    {
        public IActorRef ActorRef { get; }

        public RepositoryAPI(IActorRef registerRef)
        {
            ActorRef = registerRef;
        }
    }
}
