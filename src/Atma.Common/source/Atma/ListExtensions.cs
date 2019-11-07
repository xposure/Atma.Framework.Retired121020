namespace Atma
{
    using System.Collections.Generic;
    public static class ListExtensions
    {
        public static void RemoveFast<T>(this List<T> list, int index)
        {
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }
    }
}
