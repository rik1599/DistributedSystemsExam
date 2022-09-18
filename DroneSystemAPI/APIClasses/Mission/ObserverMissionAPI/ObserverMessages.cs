using Actors.Messages.StateChangeNotifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI
{
    internal class AskForNotifications
    {
    }

    internal class Notifications
    {
        /// <summary>
        /// Lista ordinata delle nuove notifiche
        /// </summary>
        public IReadOnlyList<OrderedStateChangeNotification> NewNotifications;

        public Notifications(IReadOnlyList<OrderedStateChangeNotification> newNotifications)
        {
            NewNotifications = newNotifications;
        }
    }
}
