using System.Text;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A generic interface that indicates the class supports validation and population of validation messages.
    /// </summary>
    public interface IModelValidate
    {
        /// <summary>
        /// Validate the instance and populate any warnings and errors into the <paramref name="validationMessages"/> string builder.  
        /// </summary>
        /// <param name="validationMessages">A non-null string builder that will be appended to for any messages.</param>
        /// <returns>If the instance is in an invalid state it will return false, otherwise it returns true.</returns>
        bool Validate(StringBuilder validationMessages);
    }
}