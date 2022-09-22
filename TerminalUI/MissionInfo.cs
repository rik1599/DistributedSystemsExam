using Actors.DTO;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;
using DroneSystemAPI.APIClasses;
using Newtonsoft.Json;

namespace TerminalUI
{
    internal class MissionInfo
    {
        public bool IsTerminated { get; private set; }
        public Host Host { get; set; }
        public List<DroneStateDTO> Notifications { get; }
        
        public ObserverMissionAPI API { get; set; }
        
        public Task ObserverTask { get; }

        public MissionInfo(Host host, ObserverMissionAPI api)
        {
            Host = host;
            API = api;
            Notifications = new List<DroneStateDTO>();
            ObserverTask = CollectNotification();
            IsTerminated = false;
        }

        private async Task CollectNotification()
        {
            do
            {
                var newNotifications = await API.AskForUpdates();
                Notifications.AddRange(newNotifications);
            } while (Notifications.Last() is not ExitStateDTO);

            IsTerminated = true;
        }

        public override string ToString()
        {
            return $"{Host}, LastStatus = {Notifications.Last()}, IsTerminated = {IsTerminated}";
        }
    }
}
