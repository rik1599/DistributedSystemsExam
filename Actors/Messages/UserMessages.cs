using Actors.DTO;

namespace Actors.Messages.User
{
    /// <summary>
    /// Messaggio di richiesta dello stato corrente
    /// </summary>
    public class GetStatusRequest
    {

    }

    /// <summary>
    /// Stato corrente (ritornato come DTO)
    /// </summary>
    public class GetStatusResponse     {
        public DroneStateDTO StateDTO { get; }

        public GetStatusResponse(DroneStateDTO stateDTO) 
        {  
            StateDTO = stateDTO;
        }
    }

    /// <summary>
    /// Messaggio per indicare al drone 
    /// di annullare la missione.
    /// </summary>
    public class CancelMissionRequest
    {
       
    }

    public class CancelMissionResponse
    {

    }

}
