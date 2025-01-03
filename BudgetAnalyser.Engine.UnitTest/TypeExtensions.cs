﻿using System;
using System.Linq;
using System.Reflection;

namespace BudgetAnalyser.Engine.UnitTest
{
    public static class TypeExtensions
    {
        public static int CountProperties(this Type instance)
        {
            var properties = instance.GetProperties();
            properties.ToList().ForEach(p => Console.WriteLine(p.Name));
            return properties.Length;
        }
    }
}
