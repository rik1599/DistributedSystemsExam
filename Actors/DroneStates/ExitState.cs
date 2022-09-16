using Actors.Messages.External;
using Actors.MissionSets;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.DroneStates
{
    internal class ExitState : DroneActorState
    {
        public ExitState(DroneActorState prececentState)
            : base(prececentState)
        {
        }

        internal override DroneActorState RunState()
        {
            // comunico la mia uscita a tutti i nodi noti 
            foreach (var node in ActorContext.Nodes)
            {
                node.Tell(new MissionFinishedMessage());
            }

            ActorContext.Log.Error("Mission ENDED! Killing myself");
            ActorRef.Tell(PoisonPill.Instance, ActorRefs.NoSender);

            return this;
        }


        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            return this;
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            return this;
        }
    }
}
