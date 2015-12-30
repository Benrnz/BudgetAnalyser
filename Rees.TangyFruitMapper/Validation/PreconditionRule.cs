using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     An abstract type to describe the shape of a precondition rule.  These rules are applied and evaluated against Dto
    ///     and Model types before they are mapped.
    /// </summary>
    public abstract class PreconditionRule
    {
        /// <summary>
        ///     Determines whether the specified property is compliant with the rule.
        /// </summary>
        /// <param name="property">The property to examine.</param>
        public abstract void IsCompliant(PropertyInfo property);
    }
}