using System;

namespace Rees.TangyFruitMapper
{
    public class PropertyNotMatchedException : Exception
    {
        public PropertyNotMatchedException()
        {
        }

        public PropertyNotMatchedException(string message) : base(message)
        {
        }

        public PropertyNotMatchedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}