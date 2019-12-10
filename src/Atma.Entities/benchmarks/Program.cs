namespace Atma.Entities
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Threading;
    using Atma.Memory;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    class Program
    {
        static void Main(string[] args)
        {
            //for (var i = 0; i < 15; i++)
            //    RunOnce(1000);
            //var summary = BenchmarkRunner.Run<ReadWriteStructs>();
            var summary = BenchmarkRunner.Run<ForEntityBench>();
        }
    }
}
