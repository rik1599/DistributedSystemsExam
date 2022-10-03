﻿using CommandLine;
using DroneSystemAPI.APIClasses;

namespace TerminalUI.Verbs
{
    [Verb("set-repository", HelpText = "Imposta il repository per le missioni spawnate da questo terminale")]
    internal class SetRepository : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Option('f', HelpText = "Forza l'impostazione del nuovo repository anche se c'è già uno.", Default = false)]
        public bool Force { get; set; }

        public Environment Run(Environment env)
        {
            if (Port == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile impostare un repository senza impostare un host.");
                return env;
            }

            var host = new Host(Host!, Port);

            if (env.DroneDeliverySystemAPI.HasRepository())
            {
                if (!Force)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Errore: allo stato attuale esiste già un repository: " +
                        $"{env.DroneDeliverySystemAPI.RepositoryAddress}. Usa l'opzione -f per forzare l'operazione.");
                    return env;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Attenzione: il repository {env.DroneDeliverySystemAPI.RepositoryAddress} verrà sovrascritto.");
                    Console.WriteLine($"E' possibile re-impostarlo con il comando set-repository -pPorta [-hNomeHost] -f");
                }
            }

            try
            {
                env.DroneDeliverySystemAPI.SetRepository(host);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore durante il collegamento con il repository\n{e}");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Repository impostato correttamente: {env.DroneDeliverySystemAPI.RepositoryAddress}");


            return env;
        }
    }
}
