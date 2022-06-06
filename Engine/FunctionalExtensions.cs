using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public static class FunctionalExtensions
    {
        public static IEnumerable<TOut> ZipWith<TOut, T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) @this, Func<T1, T2, TOut> fn)
        {
            var (l1, l2) = @this;
            return l1.Zip(l2).Select(t => fn(t.First, t.Second));
        }
    }
}
