using Akka.Actor;
using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("cancel-mission", HelpText = "Cancella una missione. Puoi cancellare solo missioni registrate")]
    internal class CancelMission : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Option('k', "kill", HelpText = "Forza la terminazione inviando una poison pill", Default = false)]
        public bool ForceKill { get; set; }

        [Value(0, HelpText = "Nome della missione", Required = true)]
        public string? MissionName { get; set; }

        public Environment Run(Environment env)
        {
            var ID = $"{MissionName}-{Host!}:{Port}";
            if (!env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione non registrata! Devi prima collegarti con connect-to-mission o spawn-mission");
                return env;
            }

            try
            {
                if (!env.Missions[ID].IsTerminated)
                {
                    var missionAPI = env.Missions[ID].API;

                    if (ForceKill)
                        missionAPI.GetDroneRef().Tell(PoisonPill.Instance);
                    else 
                        missionAPI.Cancel().Wait();

                    env.Missions[ID].IsTerminated = true;
                }

                // (non rimuovo, così posso continuare ad untilizzare il log)
                // env.Missions.Remove(ID);

            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("(Possibile) errore nella cancellazione della missione. " +
                    $"Rilevata eccezione:\n{e}");
                return env;
            }

            if (ForceKill)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("Poison pill consegnata alla missione con successo. Non si può però garantire che " +
                    "sia effettivamente terminata.");
                
                Console.WriteLine($"E' ancora possibile usare il comando log {MissionName} " +
                    $"-p{env.Missions[ID].Host.Port} [-h{env.Missions[ID].Host.HostName}] per " +
                    "leggere le notifiche ricevute fin'ora.");

                Console.WriteLine($"Si può anche provare ancora a contattare la missione con il comando ping {MissionName} " +
                    $"-p{env.Missions[ID].Host.Port} [-h{env.Missions[ID].Host.HostName}].");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK: Missione cancellata. ");
                Console.WriteLine($"E' ancora possibile usare il comando log {MissionName} " +
                    $"-p{env.Missions[ID].Host.Port} [-h{env.Missions[ID].Host.HostName}] per " +
                    "leggere le notifiche ricevute fin'ora.");
            }

                

            return env;
        }
    }
}
