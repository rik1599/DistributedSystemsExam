using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Messages
{
    public class RegisterRequest {}

    public class RegisterResponse
    {
        public ISet<IActorRef> Nodes { get; private set; }

        public RegisterResponse(ISet<IActorRef> nodes)
        {
            Nodes = nodes;
        }
    }
}
