using Akka.Actor;

namespace DroneSystemAPI.APIClasses.Repository
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
