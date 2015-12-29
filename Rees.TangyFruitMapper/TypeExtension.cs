﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal static class TypeExtension
    {
        private static readonly Type[] ExemptionList = new[] { typeof(string) };

        public static bool IsComplexType(this Type instance)
        {
            if (instance == null) return false;
            if (instance.GetTypeInfo().IsPrimitive)
            {
                // https://msdn.microsoft.com/en-us/library/system.type.isprimitive(v=vs.110).aspx
                // Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
                return false;
            }

            if (instance == typeof(decimal) || instance == typeof(string))
            {
                return false;
            }

            return true;
        }

        public static bool IsCollection(this Type instance)
        {
            if (instance == null) return false;
            if (ExemptionList.Contains(instance))
            {
                return false;
            }

            return typeof(IEnumerable).IsAssignableFrom(instance);
        }
    }
}