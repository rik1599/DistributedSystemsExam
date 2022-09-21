using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("ls", HelpText = "Lista tutti gli actor system attivi con la relativa porta")]
    internal class ListMissions : IVerb
    {
        public Environment Run(Environment env)
        {
            foreach (var system in env.ActorSystems)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Porta: {system.Key}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return env;
        }
    }
}
