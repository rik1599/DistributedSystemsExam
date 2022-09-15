using Akka.Actor;

namespace Actors.Messages.Register
{
    public class RegisterRequest 
    {
        public IActorRef Actor { get; private set; }

        public RegisterRequest(IActorRef actor)
        {
            Actor = actor;
        }
    }

    public class RegisterResponse
    {
        public ISet<IActorRef> Nodes { get; private set; }

        public RegisterResponse(ISet<IActorRef> nodes)
        {
            Nodes = nodes;
        }
    }
}
