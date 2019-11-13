using CommandLine;

namespace ExtensionGenerator
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        int Run(string[] args);
    }

    public abstract class Command : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public int Run(string[] args) => OnRun();

        protected abstract int OnRun();
    }

    public abstract class CommandWithOptions<T> : ICommand
        where T : class, new()
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public int Run(string[] args)
        {
            var parser = Parser.Default;
            return parser
                    .ParseArguments<T>(args)
                    .MapResult((T options) => OnRun(options), _ => -1);
        }

        protected abstract int OnRun(T options);
    }
}