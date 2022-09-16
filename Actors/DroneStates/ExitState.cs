using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionSets;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.DroneStates
{
    public class ExitState : DroneActorState
    {
        internal readonly DroneActorState PrecedentState;
        internal bool IsMissionAccomplished { get; }
        internal String Motivation { get; }
        
        public ExitState(DroneActorState prececentState, bool isMissionFinished, String motivation)
            : base(prececentState)
        {
            PrecedentState = prececentState;
            IsMissionAccomplished = isMissionFinished;
            Motivation = motivation;
        }

        internal override DroneActorState RunState()
        {

            ActorContext.Log.Error(Motivation);

            // comunico la mia uscita a tutti i nodi noti 
            foreach (var node in ActorContext.Nodes)
            {
                node.Tell(new MissionFinishedMessage());
            }

            // a seconda se ho terminato correttamente o no, mi invio 
            // una poison pill oppure 
            if (IsMissionAccomplished)
                ActorRef.Tell(PoisonPill.Instance, ActorRefs.NoSender);
            else
                // TODO: e come gestisco in questo caso l'invio dei DTO a eventuali observer?
                ActorContext.Context.Stop(ActorRef);


            return this;
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(InternalFlyIsSafeMessage msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(InternalMissionEnded msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }

        internal override DroneActorState OnReceive(InternalTimeoutEnded msg, IActorRef sender)
        {
            return PrecedentState.OnReceive(msg, sender);
        }
    }
}
