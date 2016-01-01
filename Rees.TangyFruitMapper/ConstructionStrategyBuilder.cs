using System;
using System.Linq;
using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class ConstructionStrategyBuilder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ConstructionStrategy Build(Type type)
        {
            var constructor = type.GetConstructor(new Type[] {});
            if (constructor != null)
            {
                return new PublicDefaultConstructor(type);
            }

            // non-public default constructor?
            var constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            constructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (constructor != null)
            {
                return new NonpublicDefaultConstructor(type);
            }

            return new CommentedOutConstructor(type);
        }
    }
}