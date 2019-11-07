namespace Atma.Entities
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UpdateGroup : Attribute
    {
        public Type Type { get; private set; }
        public UpdateGroup(Type type)
        {
            Type = type;
        }
    }
}
