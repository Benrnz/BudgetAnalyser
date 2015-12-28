using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class DtoType5
    {
        public NameDto5 Name { get; set; }

        public int Age { get; set; }

        public decimal MyNumber { get; set; }
    }

    namespace SubNamespace
    {
        public class NameDto5
        {
            public string FirstName { get; set; }

            public string Surname { get; set; }
        }
    }
}
