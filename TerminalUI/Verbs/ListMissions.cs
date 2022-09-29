using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("ls", HelpText = "Rapida panoramica della sessione, degli actor-system gestiti e delle missioni.")]
    internal class ListCommand : IVerb
    {
        [Option('m', "missions", HelpText = "Elenca le missioni generate o osservate in questa sessione", Default = false)]
        public bool ShowMissions { get; set; }

        [Option('s', "actorsystems", HelpText = "Elenca gli actor system gestiti da questa sessione", Default = false)]
        public bool ShowActorSystem { get; set; }

        [Option('r', "register", HelpText = "Mostra il registro impostato", Default = false)]
        public bool ShowRegister { get; set; }

        [Option('a', "all", HelpText = "Elenca tutte le informazioni note.", Default = false)]
        public bool ShowAll { get; set; }

        public Environment Run(Environment env)
        {
            if (!ShowRegister && !ShowMissions && !ShowActorSystem)
            {
                ShowAll = true;
            }

            if (ShowActorSystem || ShowAll)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"ActorSystem:");

                Console.ForegroundColor = ConsoleColor.White;

                if (env.ActorSystems.Count > 0)
                {
                    foreach (var system in env.ActorSystems)
                    {
                        Console.WriteLine($"{system.Key}\t\t{system.Value}");
                    }
                }
                else Console.WriteLine($"Nessun actor system.");

            }

            if (ShowMissions || ShowAll)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Missioni:");

                Console.ForegroundColor = ConsoleColor.White;

                if (env.Missions.Count > 0)
                {
                    foreach (var mission in env.Missions)
                    {
                        Console.WriteLine($"ID: {mission.Key.Split('-')[0]}\t\t{mission.Value}");
                    }
                }
                else Console.WriteLine($"Nessuna missione.");
                    
            }

            if (ShowRegister || ShowAll)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Registro (repository):");

                Console.ForegroundColor = ConsoleColor.White;
                if (env.DroneDeliverySystemAPI.HasRepository()) 
                    Console.WriteLine(env.DroneDeliverySystemAPI.RepositoryAddress);
                else
                    Console.WriteLine("Non impostato.");
            }
            
            return env;
        }
    }
}
