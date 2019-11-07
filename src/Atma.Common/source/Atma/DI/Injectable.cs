namespace Atma.DI
{
    using System;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class Injectable : Attribute
    {
        public static bool IsInjectable(Type type)
        {
            if (type.GetCustomAttributes(typeof(Injectable), true).Any())
                return true;

            foreach (var it in type.GetInterfaces())
                if (IsInjectable(it))
                    return true;

            if (type.BaseType != null)
                return IsInjectable(type.BaseType);

            return false;
        }
    }
}
