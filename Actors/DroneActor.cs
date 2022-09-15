using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DroneStates;
using Actors.Messages.Register;

namespace Actors
{
    public abstract class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler? Timers { get; set; }

        private DroneActorState? _droneState;

        /// <summary>
        /// Algoritmo di schedulazione delle partenze
        /// </summary>
        protected void AlgorithmRunBehaviour(ISet<IActorRef> nodes, MissionPath missionPath)
        {
            // avvio lo stato iniziale
            var droneContext = new DroneActorContext(Context, nodes, new WaitingMission(Self, missionPath, Priority.NullPriority), Timers!);
            _droneState = DroneActorState.CreateInitState(droneContext).RunState();

            ReceiveExternalMessages();
            ReceiveInternalMessage();
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi esterni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveExternalMessages()
        {
            // la modalità di gestione dei messaggi dipende dallo stato del drone
            Receive<ConnectRequest> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ConnectResponse>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<FlyingResponse> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<MetricMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<WaitMeMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ExitMessage>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi interni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveInternalMessage()
        {
            Receive<InternalFlyIsSafeMessage>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<InternalMissionEnded>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        public static Props Props(IActorRef register, MissionPath missionPath)
        {
            return Akka.Actor.Props.Create(() => new RegisterDroneActor(register, missionPath));
        }

        public static Props Props(IReadOnlySet<IActorRef> nodes, MissionPath missionPath)
        {
            return Akka.Actor.Props.Create(() => new SimpleDroneActor(nodes, missionPath));
        }
    }

    /// <summary>
    /// Un attore che per funzionare si appoggia ad un registro di nodi. 
    ///
    /// Quando spawna, comunica il suo ingresso al registro e riceve come output 
    /// una lista di tutti i nodi. 
    /// </summary>
    internal class RegisterDroneActor : DroneActor
    {
        public RegisterDroneActor(IActorRef register, MissionPath missionPath)
        {
            RegisterBehaviour(register, missionPath);
        }

        /// <summary>
        /// Procedura iniziale di connessione ad un repository
        /// per reperire tutti i possibili nodi.
        /// </summary>
        /// <param name="register"></param>
        private void RegisterBehaviour(IActorRef register, MissionPath missionPath)
        {
            try
            {
                Task<RegisterResponse> t = register.Ask<RegisterResponse>(new RegisterRequest(Self), TimeSpan.FromSeconds(10));
                t.Wait();

                AlgorithmRunBehaviour(t.Result.Nodes, missionPath);
            }
            catch (AskTimeoutException)
            {
                Context.GetLogger().Info($"Timeout scaduto. Impossibile comunicare con il repository all'indirizzo {register}");
                Context.Stop(Self);
            }
        }
    }

    /// <summary>
    /// Attore semplice che riceve in input una lista di nodi. 
    /// </summary>
    internal class SimpleDroneActor : DroneActor
    {
        public SimpleDroneActor(IReadOnlySet<IActorRef> nodes, MissionPath missionPath)
        {
            AlgorithmRunBehaviour(nodes.ToHashSet(), missionPath);
        }
    }
}
