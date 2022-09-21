using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("create-actor-system")]
    internal class CreateActorSystem : IVerb
    {
        public Environment Run(Environment env)
        {
            Console.WriteLine("Actor system creato");
            return env;
        }
    }
}
