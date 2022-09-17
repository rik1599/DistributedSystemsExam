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
        public IActorRef RegisterRef { get; }

        public RegisterAPI(IActorRef registerRef)
        {
            RegisterRef = registerRef;
        }
    }
}
