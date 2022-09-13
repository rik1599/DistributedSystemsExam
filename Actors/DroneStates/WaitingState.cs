using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class WaitingState : DroneActorState
    {
        private readonly Priority _priority;

        public WaitingState(DroneActorState precedentState, Priority priority): base(precedentState)
        {
            _priority = priority;
        }

        internal override DroneActorState RunState()
        {
            foreach (var node in ConflictSet.GetSmallerPriorityMissions(_priority).Keys)
            {
                node.Tell(new WaitMeMessage());
            }
            return this;
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            //TODO: errore
            return this;
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            return CreateNegotiateState(this).RunState().OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            //TODO: errore
            return this;
        }

        internal override DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        internal override DroneActorState OnReceive(InternalFlyIsSafeMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);

            // controllo se grazie al termine del volo mi si è 
            // liberato qualcosa.
            return NextState();
        }

        private DroneActorState NextState()
        {
            if (FlyingMissionsMonitor.GetFlyingMissions().Count == 0 && ConflictSet.GetGreaterPriorityMissions(_priority).Count == 0)
                return CreateFlyingState(this).RunState();
            else return this;
        }
    }
}
