namespace ServiceStack.Seq.RequestLogsFeature
{
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this List<T> enumerable)
            => enumerable == null || enumerable.Count == 0;
    }
}