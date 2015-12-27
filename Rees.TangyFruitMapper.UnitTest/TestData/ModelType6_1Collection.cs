using System.Collections.Generic;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType6_1Collection
    {
        public ModelType6_1Collection()
        {
        }

        public ModelType6_1Collection(int age, decimal myNumber, IEnumerable<string> names)
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
