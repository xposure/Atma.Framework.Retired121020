namespace Atma.Entities
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class UpdateBefore : Attribute
    {
        public Type Type { get; private set; }
        public UpdateBefore(Type type)
        {
            Type = type;
        }
    }

    public sealed class DependencyUpdateBefore : DependencyResolver
    {
        public override void Resolve(ComponentSystemList list)
        {
            foreach (var system in list.All)
            {
                var attrs = system.Type.GetCustomAttributes(typeof(UpdateBefore), true);
                if (attrs != null)
                {
                    foreach (var it in attrs)
                    {
                        var groupAttr = (UpdateBefore)it;
                        var componentSystem = list.GetByType(groupAttr.Type);
                        if (componentSystem == null)
                            throw new Exception("You can only depend on systems in your own group.");

                        list.AddDependency(componentSystem, system);
                    }
                }
            }
        }
    }
}
