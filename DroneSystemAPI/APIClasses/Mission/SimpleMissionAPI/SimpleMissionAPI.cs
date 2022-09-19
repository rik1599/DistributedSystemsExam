using Actors.DTO;
using Actors.Messages.User;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI
{
    public class SimpleMissionAPI : IMissionAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);
        private readonly TimeSpan _timeout = DEFAULT_TIMEOUT;
        private readonly IActorRef _nodeRef;

        public SimpleMissionAPI(IActorRef nodeRef)
        {
            _nodeRef = nodeRef;
        }

        public SimpleMissionAPI(IActorRef nodeRef, TimeSpan timeout) : this(nodeRef)
        {
            _timeout = timeout;
        }

        public IActorRef GetDroneRef() => _nodeRef;

        public async Task<DroneStateDTO> GetCurrentStatus()
        {
            try
            {
                var t = await GetDroneRef()
                    .Ask<GetStatusResponse>(new GetStatusRequest(), _timeout);
                return t.StateDTO;
            }
            catch (Exception)
            {
                throw new MissionIsUnreachableException();
            }
        }

        public Task Cancel()
        {
            return _nodeRef.Ask<CancelMissionResponse>(new CancelMissionRequest());
        }

        public static IMissionAPIFactory Factory() => new SimpleMissionAPIFactory();
    }

    internal class SimpleMissionAPIFactory : IMissionAPIFactory
    {
        public IMissionAPI GetMissionAPI(IActorRef nodeRef)
        {
            return new SimpleMissionAPI(nodeRef);
        }
    }
}
