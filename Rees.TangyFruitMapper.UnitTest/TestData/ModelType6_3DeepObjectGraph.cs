namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType6_3DeepObjectGraph
    {
        public ModelType6_3DeepObjectGraph()
        {
        }

        public ModelType6_3DeepObjectGraph(int age, decimal myNumber, Name6 name)
        {
            Age = age;
            MyNumber = myNumber;
            Name = name;
        }

        public Name6 Name { get; private set; }

        public int Age { get; private set; }

        public decimal MyNumber { get; private set; }
    }

    public class Name6
    {
        public Name6()
        {
        }

        public Name6(string firstName, string surname, Address6 address)
        {
            FirstName = firstName;
            Surname = surname;
            StreetAddress = address;
        }

        public string FirstName { get; private set; }

        public string Surname { get; private set; }

        public Address6 StreetAddress { get; set; }
    }

    public class Address6
    {
        public string StreetNumber { get; set; }

        public string Line1 { get; set; }

        public string City { get; set; }

        public int PostalCode { get; set; }
    }
}
