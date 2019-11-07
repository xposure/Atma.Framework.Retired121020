namespace Atma.Sandbox
{
    using System;
    using Atma.Entities;

    class Program
    {
        public struct Test
        {
            public float x;
            public float y;
        }

        static void Main(string[] args)
        {
            var cl = new ComponentList();
            var em = new EntityManager(cl);

            var type = em.CreateArchetype(typeof(Test));
            var e = em.CreateEntity(type);

            Console.WriteLine("Hello World!");
        }
    }
}
