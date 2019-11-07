namespace Atma.DI
{
    using System;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultService : Attribute
    {
        public static bool IsDefaultService(Type type)
        {
            if (type.GetCustomAttributes(typeof(DefaultService), true).Any())
                return true;

            if (type.BaseType != null)
                return IsDefaultService(type.BaseType);

            return false;
        }
    }
}
