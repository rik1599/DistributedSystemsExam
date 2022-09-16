using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.Messages.Internal
{
    /// <summary>
    /// Messaggio che un attore usa per comunicare a se stesso 
    /// che una certa missione (che prima era in volo) ora
    /// ha raggiunto un punto tale che mi permetterebbe una
    /// partenza sicura.
    /// </summary>
    public class InternalFlyIsSafeMessage
    {
        public IActorRef SafeMissionNodeRef { get; }

        public InternalFlyIsSafeMessage(IActorRef safeMissionNodeRef)
        {
            SafeMissionNodeRef = safeMissionNodeRef;
        }
    }

    public class InternalUpdatePosition { }

    public class InternalPositionRequest { }

    public class InternalPositionResponse 
    {
        public Point2D Position { get; }

        public InternalPositionResponse(Point2D position)
        {
            Position = position;
        }
    }

    public class InternalMissionEnded { }

    internal class InternalTimeoutEnded
    {
        public string TimerKey { get; }

        public InternalTimeoutEnded(string timerKey)
        {
            TimerKey = timerKey;
        }
    }
}
