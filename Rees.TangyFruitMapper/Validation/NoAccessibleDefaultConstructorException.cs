using System;

namespace Rees.TangyFruitMapper
{
    public class NoAccessibleDefaultConstructorException : Exception
    {
        public NoAccessibleDefaultConstructorException()
        {
        }

        public NoAccessibleDefaultConstructorException(string message) : base(message)
        {
        }

        public NoAccessibleDefaultConstructorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}