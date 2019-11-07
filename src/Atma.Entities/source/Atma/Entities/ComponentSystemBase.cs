namespace Atma.Entities
{
    using Atma.Profiling;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class ComponentSystemBase
    {
        private HashSet<ComponentSystemBase> _dependsOn = new HashSet<ComponentSystemBase>();
        public IReadOnlyCollection<ComponentSystemBase> DependsOn => _dependsOn;

        protected readonly IProfileService Profiler;

        internal abstract void InternalUpdate();

        public Type Type { get; internal set; }

        internal ComponentSystemBase(IProfileService profiler = null)
        {
            Type = this.GetType();
            Profiler = profiler ?? NullProfileService.Null;
        }

        protected internal virtual void Created() { }
        protected internal virtual void Destroyed() { }

        public void AddDependency(ComponentSystemBase system)
        {
            _dependsOn.Add(system);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb, 0);
            return sb.ToString();
        }

        internal virtual void ToString(StringBuilder sb, int depth)
        {
            sb.Append(new string(' ', depth * 2));
            sb.AppendLine(Type.Name);

            depth++;
            foreach (var it in _dependsOn)
            {
                sb.Append(new string(' ', depth * 2));
                sb.Append('*');
                sb.AppendLine(it.Type.Name);
            }
            //depth--;
        }
    }
}
