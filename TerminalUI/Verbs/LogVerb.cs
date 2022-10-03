using CommandLine;
using System.Text;
using TerminalUI.Tools;

namespace TerminalUI.Verbs
{
    [Verb("log", HelpText = "Stampa tutte le informazioni su una missione")]
    internal class LogVerb : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Option('v', "verbose", HelpText = "Stampa tutti i dettagli", Default =false)]
        public bool Verbose { get; set; }

        [Option('s', "short", HelpText = "Stampa solo una versione sintetica", Default = false)]
        public bool Short { get; set; }

        [Value(0, HelpText = "Nome della missione", Required = true)]
        public string? MissionName { get; set; }

        public Environment Run(Environment env)
        {            
            if (Verbose && Short)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! non puoi stampare sia una versione sintetica che completa... deciditi :)");
                return env;
            }
            
            var ID = $"{MissionName}-{Host!}:{Port}";
            if (!env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione non registrata! Devi prima collegarti con connect-mission o spawn-mission");
                return env;
            }

            var missionInfo = env.Missions[ID];

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($@"
ID locale: {MissionName}
Host: {missionInfo.Host}
Attore drone: {missionInfo.API.GetDroneRef()}
Missione terminata: {missionInfo.IsTerminated}
Notifiche: [
    {PrintNotifications(missionInfo)}
]");

            return env;
        }

        private void _tryPing(MissionInfo mission)
        {
            if (!mission.IsTerminated)
            {
                try
                {
                    var currentState = mission.API.GetCurrentStatus().Result;
                    mission.SafeAddNotification(currentState);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Error.WriteLine("Attenzione: il ping è fallito " +
                        "(potrebbe essere semplicemente che la missione è terminata). " +
                        $"Eccezione:\n{e}");
                }
            }         
        }

        private string PrintNotifications(MissionInfo mission)
        {
            var notifications = mission.Notifications.ToArray().Reverse();
            var stringBuilder = new StringBuilder();
            foreach (var notification in notifications)
            {
                var dtoToStringTool = new StringFromDTOOrchestrator(notification);

                if (Short)
                {
                    stringBuilder.Append(
                        dtoToStringTool.GetString(1, 
                        StringFromDTOOrchestrator.OutputType.MINIMAL));
                }
                else if (Verbose)
                {
                    stringBuilder.Append(
                        dtoToStringTool.GetString(1,
                        StringFromDTOOrchestrator.OutputType.COMPLETE));
                }
                else
                {
                    stringBuilder.Append(
                        dtoToStringTool.GetString(1,
                        StringFromDTOOrchestrator.OutputType.SMART));
                }

                stringBuilder.Append(",\n");
            }
            return stringBuilder.ToString();
        }
    }
}
