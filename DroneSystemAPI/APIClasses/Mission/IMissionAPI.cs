using Actors.DTO;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission
{
    /// <summary>
    /// API per interfacciarsi con una missione remota.
    /// </summary>
    public interface IMissionAPI
    {
        /// <summary>
        /// Riferimento al drone che sta portando a termine la missione remota
        /// </summary>
        /// <returns></returns>
        public IActorRef GetDroneRef();

        /// <summary>
        /// Richiedi lo stato corrente del drone e della missione
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MissionIsUnreachableException">Se l'attore non è raggiungibilie</exception>
        public Task<DroneStateDTO> GetCurrentStatus();

        /// <summary>
        /// Richiedi la cancellazione della missione.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MissionIsUnreachableException">Se l'attore non è raggiungibilie</exception>
        public Task Cancel(); 
    }

    /// <summary>
    /// Strumento per creare istanze di <code>IMissionAPI</code> 
    /// A PARTIRE DA UNA MISSIONE GIA' AVVIATA.
    /// </summary>
    public interface IMissionAPIFactory
    {
        /// <summary>
        /// Crea un'istanza API per interfacciarti ad 
        /// una missione già esistente.
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <returns></returns>
        public IMissionAPI GetMissionAPI(IActorRef nodeRef);
    }

    public class MissionIsUnreachableException : AkkaException
    {

    }
}
