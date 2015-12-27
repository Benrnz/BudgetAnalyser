using System;

namespace Rees.TangyFruitMapper
{
    public class PropertiesMustBePublicWriteableException : Exception
    {
        public PropertiesMustBePublicWriteableException()
        {
        }

        public PropertiesMustBePublicWriteableException(string message) : base(message)
        {
        }

        public PropertiesMustBePublicWriteableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}