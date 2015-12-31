using System;
using System.Collections;
using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     A Precondition rule that ensures dictionaries are not used on DTO types.
    /// </summary>
    /// <seealso cref="PreconditionPropertyRule" />
    internal class DictionariesAreNotSupportedRule : PreconditionPropertyRule
    {
        /// <summary>
        ///     Determines whether the specified property is compliant with the rule.
        /// </summary>
        /// <param name="property">The property to examine.</param>
        /// <exception cref="System.NotSupportedException">Dictionaries are currently not supported.</exception>
        public override void IsCompliant(PropertyInfo property)
        {
            if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
            {
                throw new NotSupportedException("Dictionaries are currently not supported.");
            }
        }
    }
}