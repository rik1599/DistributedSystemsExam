using Akka.Actor;
using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("terminate-actor-system", HelpText = "Termina un actor system (gestito da questa istanza)")]
    internal class TerminateActorSystem : IVerb
    {
        // [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        // public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem da terminare", Required = true)]
        public int Port { get; set; }


        public Environment Run(Environment env)
        {
            if (!env.ActorSystems.ContainsKey(Port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! L'actor system non esiste, oppure non è gestito da questa istanza del programma");
                return env;
            }

            try
            {
                env.ActorSystems[Port].Terminate();
                env.ActorSystems.Remove(Port);

            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore nella terminazione del sistema. " +
                    $"Rilevata eccezione:\n{e}");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK: sistema terminato.");

            return env;
        }
    }
}
