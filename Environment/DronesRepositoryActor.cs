using Akka.Actor;
using Actors.Utils;
using Akka.Event;
using Actors.Messages.Register;

namespace Environment
{
    /// <summary>
    /// Attore ambiente, si occupa si spawnare e 
    /// supervisionare una serie di attori drone.
    /// 
    /// Può ricevere richeste di spawn di attori (a cui 
    /// risponde con l'indirizzo) e varie altre richieste
    /// di utility.
    /// </summary>
    public class DronesRepositoryActor : ReceiveActor
    {
        private readonly ISet<IActorRef> _nodes;
        private readonly DebugLog _logger;

        public DronesRepositoryActor()
        {
            _logger = new(Context.GetLogger());
            _nodes = new HashSet<IActorRef>();

            Receive<RegisterRequest>(msg => OnReceive(msg));
            Receive<Terminated>(msg => OnReceive(msg));
        }

        private void OnReceive(Terminated msg)
        {
            _logger.Info($"Missione del drone {Sender} terminata. ExistenceConfirmed= {msg.ExistenceConfirmed}");
            _nodes.Remove(Sender);
        }

        private void OnReceive(RegisterRequest msg)
        {
            _logger.Info($"Registrazione del nodo {msg.Actor} nel registro");
            Sender.Tell(new RegisterResponse(_nodes.ToHashSet()), Self);

            Context.Watch(msg.Actor);
            _nodes.Add(msg.Actor);
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new DronesRepositoryActor());
        }
    }
}
