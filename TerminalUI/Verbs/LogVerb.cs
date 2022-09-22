using Akka.Util.Internal;
using CommandLine;
using System.Text;

namespace TerminalUI.Verbs
{
    [Verb("log", HelpText = "Stampa tutte le informazioni su una missione")]
    internal class LogVerb : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Value(0, HelpText = "ID della missione", Required = true)]
        public int Mission { get; set; }

        public Environment Run(Environment env)
        {
            var ID = $"{Mission}-{Host!}:{Port}";
            if (!env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione non registrata! Devi prima collegarti con connect-mission o spawn-mission");
                return env;
            }

            var missionInfo = env.Missions[ID];
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($@"
ID locale: {Mission}
Host: {missionInfo.Host}
Missione terminata: {missionInfo.IsTerminated}
Notifiche: {{
    {PrintNotifications(missionInfo)}
}}
");

            return env;
        }

        private static string PrintNotifications(MissionInfo mission)
        {
            var notifications = mission.Notifications.ToArray().Reverse();
            var stringBuilder = new StringBuilder();
            foreach (var notification in notifications)
            {
                stringBuilder.Append($@"
    {{
        Type: {notification.GetType()}
        Timestamp: {notification.DroneTimestamp}
        Age: {notification.Age}
        Posizione attuale: {notification.CurrentPosition()}
        Missione terminata con successo: {notification.IsMissionAccomplished()}
    }}
");
            }
            return stringBuilder.ToString();
        }
    }
}
