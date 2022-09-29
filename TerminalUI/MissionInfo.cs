using Actors.DTO;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;
using DroneSystemAPI.APIClasses;
using Newtonsoft.Json;

namespace TerminalUI
{
    internal class MissionInfo
    {
        public bool IsTerminated { get; set; }
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

                lock (_notificationLock)
                {
                    Notifications.AddRange(newNotifications);
                }
            } while (Notifications.Last() is not ExitStateDTO);

            IsTerminated = true;
        }

        private readonly object _notificationLock = new object();

        public void SafeAddNotification(DroneStateDTO droneStateDTO)
        {
            lock (_notificationLock)
            {
                Notifications.Add(droneStateDTO);
            }
        }

        public override string ToString()
        {
            return $"{Host}, LastStatus = {Notifications.Last()}, IsTerminated = {IsTerminated}";
        }
    }
}
