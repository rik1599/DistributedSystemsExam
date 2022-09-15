using Actors.MissionPathPriority;
using Akka.Actor;
using Environment.PhisicalHost;


namespace Environment.Actor
{
    /// <summary>
    /// Richiesta di avvio di una missione da parte
    /// di un certo drone.
    /// </summary>
    public class SpawnMissionRequest
    {
        public MissionPath MissionPath { get; private set; }

        public Host DroneHost { get; private set; }

        public SpawnMissionRequest(MissionPath missionPath, Host host)
        {
            MissionPath = missionPath;
            DroneHost = host;
        }
    }

    /// <summary>
    /// Risposta (affermativa) ad una richiesta 
    /// di avvio di missione, include l'indirizzo 
    /// del drone.
    /// </summary>
    public class SpawnMissionResponse
    {
        public IActorRef DroneRef { get; private set; }

        public SpawnMissionResponse(IActorRef droneRef)
        {
            DroneRef = droneRef;
        }
    }
}
