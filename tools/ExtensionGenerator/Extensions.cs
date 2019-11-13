namespace ExtensionGenerator
{
    using System.Collections.Generic;
    using System.Text;

    public static class Extensions
    {
        public static IEnumerable<int> Range(this int it)
        {
            for (var i = 0; i < it; i++)
                yield return i;
        }

        public static string Join(this IEnumerable<string> it, string join = ",")
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var t in it)
            {
                if (!first)
                    sb.Append(join);

                first = false;

                sb.Append(t);
            }
            return sb.ToString();
        }
        //public static void Expand<T>(this T it,  )
    }
}