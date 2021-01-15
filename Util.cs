using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mima_c
{
    static class Util
    {
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
    }
}
