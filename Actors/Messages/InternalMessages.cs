using Akka.Actor;

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
        public IActorRef SafeMissionNodeRef { get; set; }

        public InternalFlyIsSafeMessage(IActorRef safeMissionNodeRef)
        {
            SafeMissionNodeRef = safeMissionNodeRef;
        }
    }
}
