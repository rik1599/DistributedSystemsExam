using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Register
{
    public class RegisterAPI
    {
        public IActorRef ActorRef { get; }

        public RegisterAPI(IActorRef registerRef)
        {
            ActorRef = registerRef;
        }
    }
}
