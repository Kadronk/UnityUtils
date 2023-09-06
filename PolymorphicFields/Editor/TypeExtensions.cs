using System;
using System.Collections.Generic;
using System.Linq;

namespace Kadronk.PolymorphicFields.Editor
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetSubtypes(this Type type) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(type));
        }

        public static string GetDisplayName(this Type type) {
            object[] displayNames = type.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (displayNames.Length > 0)
                return ((DisplayNameAttribute)displayNames[0]).Name;
            return type.Name;
        }
    }
}