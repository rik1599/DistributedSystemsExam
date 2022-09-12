using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Akka.Actor;
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

        public FlyingDroneActor(Mission mission, IActorRef supervisor, Point2D position)
        {
            _mission = mission;
            _supervisor = supervisor;
            _position = position;
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
            var distance = (_mission.Path.Speed * elapsedTime.TotalSeconds) * _mission.Path.PathSegment.Direction;
            
            _position += distance;
            _lastUpdateTime = DateTime.Now;
        }
    }
}
