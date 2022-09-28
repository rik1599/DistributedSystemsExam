using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses
{
    /// <summary>
    /// Interfaccia globale al sistema (lato "client").
    /// 
    /// E' un Facade che consente di interfacciarsi con il sistema di 
    /// consegna, quindi:
    /// *   creare o impostare un registro
    /// *   avviare missioni (e generare un'API)
    /// *   generare un'API per connettersi ad una missione esistente
    /// 
    /// </summary>
    public class DroneDeliverySystemAPI
    {
        private readonly ActorSystem _interfaceActorSystem;

        public String SystemName { get; }
        public String RepositoryActorName { get; }

        
        private IActorRef? RepositoryActorRef { get; }

        public DroneDeliverySystemAPI(ActorSystem interfaceActorSystem, string systemName, string repositoryActorName, IActorRef? repositoryActorRef)
        {
            _interfaceActorSystem = interfaceActorSystem;
            SystemName = systemName;
            RepositoryActorName = repositoryActorName;
            RepositoryActorRef = repositoryActorRef;
        }
    }
}
