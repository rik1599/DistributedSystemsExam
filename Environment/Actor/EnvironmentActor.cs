using Akka.Actor;
using Actors;
using Actors.MissionPathPriority;
using Environment.PhisicalHost;
using Actors.Utils;
using Akka.Event;

namespace Environment.Actor
{
    /// <summary>
    /// Attore ambiente, si occupa si spawnare e 
    /// supervisionare una serie di attori drone.
    /// 
    /// Può ricevere richeste di spawn di attori (a cui 
    /// risponde con l'indirizzo) e varie altre richieste
    /// di utility.
    /// </summary>
    public class EnvironmentActor : ReceiveActor
    {
        private readonly ISet<IActorRef> _nodes;
        private DebugLog _logger;

        public EnvironmentActor()
        {
            _logger = new DebugLog(Context.GetLogger());

            _nodes = new HashSet<IActorRef>();

            Receive<SpawnMissionRequest>(msg => OnReceive(msg, Sender));
            Receive<Terminated>(msg => OnReceive(msg, Sender));
        }

        private void OnReceive(SpawnMissionRequest msg, IActorRef sender)
        {
            IActorRef droneRef = SpawnMissionActor(msg.MissionPath, msg.DroneHost);
            Context.Watch(droneRef);
            _nodes.Add(droneRef);

            _logger.Info($"Spawnata missione su {droneRef} su richiesta di {sender}.");

            sender.Tell(new SpawnMissionResponse(droneRef));
        }

        private void OnReceive(Terminated msg, IActorRef sender)
        {
            _logger.Info($"Missione del drone {sender} terminata.");
            _nodes.Remove(sender);
        }
            
        /// <summary>
        /// Crea l'attore che gestirà una certa missione.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        private IActorRef SpawnMissionActor(MissionPath path, Host host)
        {
            return Context.System.ActorOf(
                Props
                    .Create(() => new DroneActor(_nodes.ToHashSet(), path))
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(host.Parse())))
                );
        }
    }
}
