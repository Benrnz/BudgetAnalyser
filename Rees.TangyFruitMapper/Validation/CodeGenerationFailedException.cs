using System;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     A general exception type for unexpected errors during code generation.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CodeGenerationFailedException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeGenerationFailedException" /> class.
        /// </summary>
        public CodeGenerationFailedException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeGenerationFailedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CodeGenerationFailedException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeGenerationFailedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CodeGenerationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}