namespace Atma.DI
{
    using System;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceProvider : Attribute
    {
        private static bool IsServiceProvider(Type type)
        {
            if (!type.IsInterface)
                return false;

            if (type.GetCustomAttributes(typeof(ServiceProvider), true).Any())
                return true;

            return false;
        }

        public static Type GetServiceProvided(Type type)
        {
            foreach (var it in type.GetInterfaces())
                if (IsServiceProvider(it))
                    return it;

            if (type.BaseType != null)
                return GetServiceProvided(type.BaseType);

            return null;
        }
    }
}
