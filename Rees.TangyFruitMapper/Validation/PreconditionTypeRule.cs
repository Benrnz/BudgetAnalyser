using System;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     An abstract type to describe the shape of a precondition rule.  These rules are applied and evaluated against Dto
    ///     and Model types before they are mapped.
    /// </summary>
    internal abstract class PreconditionTypeRule
    {
        /// <summary>
        ///     Determines whether the specified type is compliant with the rule.
        /// </summary>
        /// <param name="typeToCheck">The type to examine.</param>
        public abstract void IsCompliant(Type typeToCheck);
    }
}