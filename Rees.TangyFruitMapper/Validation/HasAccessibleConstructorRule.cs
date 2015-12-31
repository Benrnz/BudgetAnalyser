using System;
using System.Linq;
using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    internal class HasAccessibleConstructorRule : PreconditionTypeRule
    {
        public override void IsCompliant(Type typeToCheck)
        {
            // public default constructor?
            var constructor = typeToCheck.GetConstructor(new Type[] {});
            if (constructor != null)
            {
                return;
            }

            // non-public default constructor?
            var constructors = typeToCheck.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            constructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (constructor != null)
            {
                return;
            }

            throw new NoAccessibleDefaultConstructorException($"No constructor found on {typeToCheck.Name}");
        }
    }
}