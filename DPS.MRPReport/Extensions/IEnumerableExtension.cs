using Soneta.Towary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Extensions
{
    public static class IEnumerableExtension
    {
        public static Quantity Sum(this IEnumerable<Quantity> source)
        {
            if (!source.Any())
            {
                return Quantity.Zero;
            }
            return source.Aggregate((x, y) => x + y);
        }

        public static Quantity Sum<T>(this IEnumerable<T> source, Func<T, Quantity> selector)
        {
            if (!source.Any())
            {
                return Quantity.Zero;
            }
            return source.Select(selector).Aggregate((x, y) => x + y);
        }
    }
}
