using Actors.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
