using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtensionGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            var commands = new List<ICommand>(FindAllCommands());

            if (args.Length == 0)
            {
                ShowCommands(commands);
            }
            else
            {
                var commandName = args[0];
                var commandArgs = new string[args.Length - 1];
                Array.Copy(args, 1, commandArgs, 0, args.Length - 1);

                var command = commands.FirstOrDefault(x => string.Compare(x.Name, commandName, true) == 0);
                if (command == null)
                {
                    Console.WriteLine($"\nCommand [{commandName}] not found.");
                    ShowCommands(commands);
                }
                else
                {
                    return command.Run(commandArgs);
                }
            }

            return -1;
        }

        private static void ShowCommands(IEnumerable<ICommand> commands)
        {
            Console.WriteLine();
            Console.WriteLine("Extension generate supports the following commands.");
            foreach (var cmd in FindAllCommands())
                Console.WriteLine($"  {cmd.Name,-25} {cmd.Description}");
            Console.WriteLine();
        }

        static IEnumerable<ICommand> FindAllCommands()
        {
            var type = typeof(ICommand);
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var it in types)
                if (!it.IsAbstract && !it.IsInterface && it.IsClass && type.IsAssignableFrom(it))
                    yield return (ICommand)Activator.CreateInstance(it);
        }
    }
}