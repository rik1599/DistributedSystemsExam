using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("ls", HelpText = "Lista tutti gli actor system attivi con la relativa porta")]
    internal class ListCommand : IVerb
    {
        [Option('m', SetName = "missions", HelpText = "Elenca le missioni (invece degli ActorSystems) generate in questa sessione", Default = false)]
        public bool Missions { get; set; }

        public Environment Run(Environment env)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            if (Missions)
            {
                foreach (var mission in env.GeneratedMissions)
                {
                    Console.WriteLine($"{mission.Key}\t\t{mission.Value}");
                }
            }
            else
            {
                foreach (var system in env.ActorSystems)
                {
                    Console.WriteLine($"{system.Key}\t\t{system.Value.Name}");
                }
            }
            return env;
        }
    }
}
