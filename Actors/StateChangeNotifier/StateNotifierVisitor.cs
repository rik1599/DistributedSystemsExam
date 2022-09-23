using Actors.DroneStates;
using Actors.DTO;
using Actors.Messages.StateChangeNotifier;
using Akka.Actor;


namespace Actors.StateChangeNotifier
{
    /// <summary>
    /// Strumento utilizzabile (direttamente dallo stato e 
    /// senza dipendenza diretta, parlando alla classe base) 
    /// per aggiornare il notificatore di un avvenuto 
    /// cambio di stato. 
    /// </summary>
    internal class StateNotifierVisitor : IDroneStateVisitor
    {
        private IActorRef _notificationServiceActor;

        public StateNotifierVisitor(IActorRef notificationServiceActor)
        {
            _notificationServiceActor = notificationServiceActor;
        }

        internal override void Visit(InitState state)
        {
            StateDTOBuilderVisitor stateBuilder = new StateDTOBuilderVisitor();
            stateBuilder.Visit(state);
            DroneStateDTO stateDTO = stateBuilder.StateDTO!;

            _notificationServiceActor.Tell(new StateChangeNotification(stateDTO));
        }

        internal override void Visit(NegotiateState state)
        {
            StateDTOBuilderVisitor stateBuilder = new StateDTOBuilderVisitor();
            stateBuilder.Visit(state);
            DroneStateDTO stateDTO = stateBuilder.StateDTO!;

            _notificationServiceActor.Tell(new StateChangeNotification(stateDTO));
        }

        internal override void Visit(WaitingState state)
        {
            StateDTOBuilderVisitor stateBuilder = new StateDTOBuilderVisitor();
            stateBuilder.Visit(state);
            DroneStateDTO stateDTO = stateBuilder.StateDTO!;

            _notificationServiceActor.Tell(new StateChangeNotification(stateDTO));
        }

        internal override void Visit(FlyingState state)
        {
            StateDTOBuilderVisitor stateBuilder = new StateDTOBuilderVisitor();
            stateBuilder.Visit(state);
            DroneStateDTO stateDTO = stateBuilder.StateDTO!;

            _notificationServiceActor.Tell(new StateChangeNotification(stateDTO));
        }

        internal override void Visit(ExitState state)
        {
            StateDTOBuilderVisitor stateBuilder = new StateDTOBuilderVisitor();
            stateBuilder.Visit(state);
            DroneStateDTO stateDTO = stateBuilder.StateDTO!;

            _notificationServiceActor.Tell(new StateChangeNotification(stateDTO));
        }
    }
}
