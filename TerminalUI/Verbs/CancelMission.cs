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

        [Option('p', HelpText = "Forza la terminazione inviando una poison pill", Required = false, Default = false)]
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
                }

                env.Missions.Remove(ID);

            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore nella cancellazione della missione. " +
                    $"Rilevata eccezione:\n{e}");
                return env;
            }
            

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK: Missione cancellata");

            return env;
        }
    }
}
