namespace Atma.Sandbox
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Atma.Entities;
    using Atma.Memory;
    using Shouldly;
    using System.Diagnostics;
    using static Atma.Debug;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Atma.Common;
    using System.Reflection.Emit;
    using System.Reflection;

    class Program
    {

        protected static ILoggerFactory _logFactory;
        protected static ILogger _logger;

        static unsafe AllocationHandle Take(IAllocator memory, int size)
        {
            var handle = memory.Take(size);
            var addr = (byte*)handle.Address;
            for (var k = 0; k < size; k++)
                addr[k] = 0xfe;
            return handle;
        }

        static unsafe void Verify(in AllocationHandle handle, int size)
        {
            var addr = (byte*)handle.Address;
            for (var k = 0; k < size; k++)
                if (addr[k] != 0xfe)
                    throw new Exception($"{handle}[{k}] was {addr[k].ToString("X2")}");
        }

        static unsafe void Main(string[] args)
        {
            _logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _logFactory.CreateLogger("Sandbox");

            using var memory = new HeapAllocator(_logFactory);
            using var entities = new EntityManager(_logFactory, memory);


            var data = stackalloc Test[2];
            var ptr = (ITest)data[0];
            ptr.Layout();



        }



        public interface ITest
        {
            Layout Size { get; }
            void Layout();

        }

        public struct Size
        {
            public int X;
            public int Y;
        }

        public struct Layout
        {
            public Size Min;
            public Size Max;
        }


        public struct Test : ITest
        {
            private Layout _size;
            public Layout Size => _size;
            public void Layout()
            {
                _size.Min.X = 10;
                _size.Min.Y = 10;
                _size.Max.X = 20;
                _size.Max.Y = 20;
            }
        }
        public struct Test2 : ITest
        {
            private Layout _size;
            public Layout Size => _size;
            public void Layout()
            {
                _size.Min.X = 100;
                _size.Min.Y = 100;
                _size.Max.X = 200;
                _size.Max.Y = 200;
            }
        }
    }
}
