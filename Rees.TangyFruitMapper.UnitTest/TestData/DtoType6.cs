using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class DtoType6
    {
        public NameDto6 Name { get; set; }

        public int Age { get; set; }

        public decimal MyNumber { get; set; }
    }

    namespace SubNamespace
    {
        public class NameDto6
        {
            public string FirstName { get; set; }

            public string Surname { get; set; }

            public AddressDto6 StreetAddress { get; set; }
        }

        public class AddressDto6
        {
            public string StreetNumber { get; set; }

            public string Line1 { get; set; }

            public string City { get; set; }

            public int PostalCode { get; set; }
        }
    }
}
