using Actors.DroneStates;
using Actors.DTO;
using Actors.MissionPathPriority;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors
{
    internal class StateDTOBuilderVisitor : DroneStateVisitor
    {
        public DroneStateDTO? StateDTO { get; private set; }

        internal override void Visit(DroneActorState state)
        {
            StateDTO = new DroneStateDTO(
                state.ActorContext, 
                state.ConflictSet.GetMissions().ToHashSet(),
                state.FlyingMissionsMonitor.GetFlyingMissions().ToHashSet(),
                state.LastNegotiationRound, 
                Priority.NullPriority
                );
        }

        internal override void Visit(InitState state)
        {
            StateDTO = new InitStateDTO(
                state.ActorContext,
                state.GetMissingConnectResponses(),
                state.ConflictSet.GetMissions().ToHashSet(),
                state.FlyingMissionsMonitor.GetFlyingMissions().ToHashSet()
                );
        }

        internal override void Visit(NegotiateState state)
        {
            StateDTO = new NegotiateStateDTO(
                state.ActorContext,
                state.ConflictSet.GetMissions().ToHashSet(),
                state.FlyingMissionsMonitor.GetFlyingMissions().ToHashSet(),
                state.LastNegotiationRound,
                state.GetPriority()
                );
        }

        internal override void Visit(WaitingState state)
        {
            StateDTO = new NegotiateStateDTO(
                state.ActorContext,
                state.ConflictSet.GetMissions().ToHashSet(),
                state.FlyingMissionsMonitor.GetFlyingMissions().ToHashSet(),
                state.LastNegotiationRound,
                state.GetPriority()
                );
        }

        internal override void Visit(FlyingState state)
        {
            StateDTO = new FlyingStateDTO(
                state.ActorContext,
                state.LastNegotiationRound,
                state.GetCurrentPosition(),
                state.DoneFlyTime(),
                state.RemainingFlyTime()
                );
        }

        internal override void Visit(ExitState state)
        {
            StateDTOBuilderVisitor precStateVisitor = new StateDTOBuilderVisitor();
            state.PrecedentState.PerformVisit(precStateVisitor);

            StateDTO = new ExitStateDTO(
                state.ActorContext,
                state.LastNegotiationRound,
                state.IsMissionAccomplished, 
                state.Motivation,
                precStateVisitor.StateDTO!
                );
        }
    }
}
