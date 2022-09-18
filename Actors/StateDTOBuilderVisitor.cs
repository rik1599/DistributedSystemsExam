using Actors.DroneStates;
using Actors.DTO;

namespace Actors
{
    internal class StateDTOBuilderVisitor : IDroneStateVisitor
    {
        public DroneStateDTO? StateDTO { get; private set; }

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
            StateDTO = new WaitingStateDTO(
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
