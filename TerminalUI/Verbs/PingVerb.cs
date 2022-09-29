using Actors.DTO;
using Akka.Util.Internal;
using CommandLine;
using System.Text;
using TerminalUI.Tools;

namespace TerminalUI.Verbs
{
    [Verb("ping", HelpText = "Effettua un ping per leggere lo stato corrente della missione.")]
    internal class PingVerb : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Option('v', "verbose", HelpText = "Stampa tutti i dettagli", Default =false)]
        public bool Verbose { get; set; }

        [Option('s', "short", HelpText = "Stampa solo una versione sintetica", Default = false)]
        public bool Short { get; set; }

        [Option('l', "log", HelpText = "Salva il ping sul log della missione", Default = false)]
        public bool Log { get; set; }

        [Option('f', "force", HelpText = "Forza il ping anche se la missione risulta terminata", Default = false)]
        public bool ForcePing { get; set; }


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

            if (missionInfo.IsTerminated)
            {
                if (!ForcePing) 
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("Attenzione, la missione risulta terminata.");
                    Console.Error.WriteLine($"usa il comando log {MissionName} -p{Port} [-h{Host}] per visualizzare" +
                    $"le ultime notifiche ricevute. Se vuoi provare lo stesso a fare un ping, usa l'opzione -f");
                    return env;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Attenzione, la missione risulta terminata (si tenta ugualmente di fare un ping).");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Tentativo di ping in corso...");

            DroneStateDTO pingResult;

            try
            {
                pingResult = missionInfo.API.GetCurrentStatus().Result;
                
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore: il tentativo di ping è fallito. Eccezione rilevata:\n{e}");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Attenzione: potrebbe essere semplicemente che la missione sia terminata " +
                    $"prima che il messaggio di ping sia giunto a destinazione. Usa il comando log {MissionName} " +
                    $"-p{env.Missions[ID].Host.Port} [-h{env.Missions[ID].Host.HostName}] per " +
                    "leggere le notifiche ricevute fin'ora.");
                return env;

            }

            if (Log)
                missionInfo.SafeAddNotification(pingResult);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($@"
ID locale: {MissionName}
Host: {missionInfo.Host}
Missione terminata: {missionInfo.IsTerminated}
Esito del log: {getDTOString(pingResult)}");

            return env;
        }

        private string getDTOString(DroneStateDTO dto)
        {
            var dtoToStringTool = new StringFromDTOOrchestrator(dto);

            if (Short)
               return dtoToStringTool.GetString(1,
                    StringFromDTOOrchestrator.OutputType.MINIMAL);
            else if (Verbose)
                return dtoToStringTool.GetString(1,
                    StringFromDTOOrchestrator.OutputType.COMPLETE);
            else
                return dtoToStringTool.GetString(1,
                    StringFromDTOOrchestrator.OutputType.SMART);
        }
    }
}
