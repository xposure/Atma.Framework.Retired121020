using System;
using CommandLine;

namespace Clone
{
    public class CloneOptions
    {
        public int Run()
        {
            Console.WriteLine("Over engineering, we don't need this yet!");
            return 0;
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CloneOptions>(args);
            return result.MapResult(
                    (CloneOptions options) => options.Run(),
                    errors => -1);

        }
    }
}
