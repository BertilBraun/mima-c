using System;
using System.Collections.Generic;
using System.Linq;

namespace mima_c
{
    static class Util
    {
        public static interpreter.RuntimeType.Type GetRuntimeType(this string s)
        {
            return interpreter.RuntimeType.GetTypeFromString(s);
        }

        public static string Format(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
		
        public static string FormatList<T>(this List<T> list)
        {
            return string.Join(", ", list);
        }
		
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
		
        public static string Escape(this string s)
        {
            // TODO escape
            return s;
        }
    }
}
