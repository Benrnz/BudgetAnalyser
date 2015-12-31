using System;
using System.Collections.Generic;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     An exception to indicate that the DTO type contains collections that are not of type <see cref="List{T}" />. This
    ///     is a convention requirement for DTO types.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CollectionsMustBeListTException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CollectionsMustBeListTException" /> class.
        /// </summary>
        public CollectionsMustBeListTException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CollectionsMustBeListTException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CollectionsMustBeListTException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CollectionsMustBeListTException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CollectionsMustBeListTException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}