namespace Atma.Entities
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class UpdateAfter : Attribute
    {
        public Type Type { get; private set; }
        public UpdateAfter(Type type)
        {
            Type = type;
        }
    }

    public sealed class DependencyUpdateAfter : DependencyResolver
    {
        public override void Resolve(ComponentSystemList list)
        {
            foreach (var system in list.All)
            {
                var attrs = system.Type.GetCustomAttributes(typeof(UpdateAfter), true);
                if (attrs != null)
                {
                    foreach (var it in attrs)
                    {
                        var groupAttr = (UpdateAfter)it;
                        var componentSystem = list.GetByType(groupAttr.Type);
                        if (componentSystem == null)
                            throw new Exception("You can only depend on systems in your own group.");

                        list.AddDependency(system, componentSystem);
                    }
                }
            }
        }
    }
}
