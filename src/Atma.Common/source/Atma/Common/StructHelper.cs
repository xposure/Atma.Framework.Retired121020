using System.Linq;
using System.Reflection;
using System.Text;

namespace Atma.Common
{
    public static class StructHelper
    {
        private static class StructHelperCache<T>
            where T : unmanaged
        {
            private readonly static FieldInfo[] fields;

            //we should think about making this thread safe
            //but its only for debugging purposes
            private readonly static StringBuilder sb;
            private readonly static string name;

            static StructHelperCache()
            {
                sb = new StringBuilder();
                var type = typeof(T);
                var typeInfo = type.GetTypeInfo();
                fields = typeInfo.DeclaredFields.ToArray();

                name = type.Name;
            }

            public static string ToString(in T t)
            {
                var o = (object)t;
                sb.Clear();
                sb.Append(name);
                sb.Append(" { ");
                for (var i = 0; i < fields.Length; i++)
                {
                    var it = fields[i];
                    sb.Append(it.Name);
                    sb.Append(": ");
                    sb.Append(it.GetValue(o).ToString());
                    if (i < fields.Length - 1)
                        sb.Append(", ");
                }
                sb.Append(" } ");
                return sb.ToString();
            }
        }

        public static string ToString<T>(in T t)
            where T : unmanaged
        {
            return StructHelperCache<T>.ToString(t);
        }
    }
}
