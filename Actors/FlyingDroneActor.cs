using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.Utils;
using Akka.Actor;
using Akka.Event;
using MathNet.Spatial.Euclidean;

namespace Actors
{
    internal class FlyingDroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        private readonly Mission _mission;
        private readonly IActorRef _supervisor;
        private Point2D _position;
        private DateTime _lastUpdateTime;

        private readonly DebugLog _logger = new(Context.GetLogger()); 

        public FlyingDroneActor(Mission mission, IActorRef supervisor)
        {
            _mission = mission;
            _supervisor = supervisor;
            _position = mission.Path.StartPoint;
            _lastUpdateTime = DateTime.Now;

            Receive<InternalPositionRequest>(msg => OnReceive(msg));
            Receive<InternalUpdatePosition> (msg => OnReceive(msg));

            Timers!.StartPeriodicTimer("updatePosition", new InternalUpdatePosition(), TimeSpan.FromSeconds(1));
        }

        private void OnReceive(InternalPositionRequest msg)
        {
            Sender.Tell(new InternalPositionResponse(_position));
        }

        private void OnReceive(InternalUpdatePosition msg)
        {
            var elapsedTime = DateTime.Now - _lastUpdateTime;
            var distanceTraveled = (_mission.Path.Speed * elapsedTime.TotalSeconds) * _mission.Path.PathSegment().Direction;
            
            _position += distanceTraveled;
            _lastUpdateTime = DateTime.Now;

            //Vettore punto di arrivo - punto attuale
            var distanceToEnd = _mission.Path.PathSegment().EndPoint - _position;
            if (distanceToEnd.Normalize().Equals(-_mission.Path.PathSegment().Direction, 1e-3) || distanceToEnd.Length == 0)
            {
                _supervisor.Tell(new InternalMissionEnded());
                _logger.Warning("Viaggio finito");
                Self.Tell(PoisonPill.Instance);
            }
        }

        public static Props Props(Mission mission, IActorRef supervisor)
        {
            return Akka.Actor.Props.Create(() => new FlyingDroneActor(mission, supervisor));
        }
    }
}
