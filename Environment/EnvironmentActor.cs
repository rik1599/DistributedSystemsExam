using Akka.Actor;
using Actors;

namespace Environment
{
    public class EnvironmentActor : ReceiveActor
    {
        private readonly ISet<IActorRef> _nodes;

        public EnvironmentActor()
        {
            _nodes = new HashSet<IActorRef>();

            Receive<Terminated>(t => OnReceive(t));
            Receive<MissionMessage>(msg => OnReceive(msg));
        }

        private void OnReceive(Terminated terminated)
        {
            _nodes.Remove(terminated.ActorRef);
        }

        private void OnReceive(MissionMessage missionMessage)
        {
            var remoteActor = Context.ActorOf(
                Props
                    .Create(() => new EchoActor())
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(missionMessage.Host)))
                );
            Context.ActorOf(Props.Create(() => new HelloActor(remoteActor)));
            _nodes.Add(remoteActor);
            Context.Watch(remoteActor);
        }
    }
}
