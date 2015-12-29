using System;

namespace Rees.TangyFruitMapper.Validation
{
    public class CollectionsMustBeListTException : Exception
    {
        public CollectionsMustBeListTException()
        {
        }

        public CollectionsMustBeListTException(string message) : base(message)
        {
        }

        public CollectionsMustBeListTException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}