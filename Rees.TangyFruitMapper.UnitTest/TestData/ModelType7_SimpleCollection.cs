using System.Collections.Generic;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType7_SimpleCollection
    {
        public ModelType7_SimpleCollection()
        {
        }

        public ModelType7_SimpleCollection(int age, decimal myNumber, IEnumerable<string> names)
        {
            Age = age;
            MyNumber = myNumber;
            Names = names;
        }

        public IEnumerable<string> Names { get; private set; }

        public int Age { get; private set; }

        public decimal MyNumber { get; private set; }
    }
}
