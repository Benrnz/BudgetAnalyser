using System.Collections.Generic;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType8_ComplexCollection
    {
        public ModelType8_ComplexCollection()
        {
        }

        public ModelType8_ComplexCollection(int age, decimal myNumber, IEnumerable<Name5> names)
        {
            Age = age;
            MyNumber = myNumber;
            Names = names;
        }

        public IEnumerable<Name5> Names { get; private set; }

        public int Age { get; private set; }

        public decimal MyNumber { get; private set; }
    }
}
