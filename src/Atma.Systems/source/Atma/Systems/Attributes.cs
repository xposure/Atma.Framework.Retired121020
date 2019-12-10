namespace Atma.Systems
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NameAttribute : Attribute
    {
        private string _name;

        public string Name => _name;

        public NameAttribute(string name)
        {
            _name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BeforeAttribute : Attribute
    {
        private string[] _names;

        public string[] Names => _names;

        public BeforeAttribute(params string[] names)
        {
            _names = names;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AfterAttribute : Attribute
    {
        private string[] _names;

        public string[] Names => _names;

        public AfterAttribute(params string[] names)
        {
            _names = names;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        private int _priority;

        public int Priority => _priority;

        public PriorityAttribute(int priority)
        {
            _priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AnyAttribute : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public AnyAttribute(params Type[] types)
        {
            _types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HasAttribute : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public HasAttribute(params Type[] types)
        {
            _types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
        private Type[] _types;

        public Type[] Types => _types;

        public IgnoreAttribute(params Type[] types)
        {
            _types = types;
        }
    }

}