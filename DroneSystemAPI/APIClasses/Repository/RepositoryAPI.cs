using Akka.Actor;

namespace DroneSystemAPI.APIClasses.Repository
{
    [Obsolete("Not used any more", true)]
    public class RepositoryAPI
    {
        public IActorRef ActorRef { get; }

        public RepositoryAPI(IActorRef registerRef)
        {
            ActorRef = registerRef;
        }
    }
}
