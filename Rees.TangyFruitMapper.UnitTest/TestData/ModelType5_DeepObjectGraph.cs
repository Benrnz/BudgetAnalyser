namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType5_DeepObjectGraph
    {
        public ModelType5_DeepObjectGraph()
        {
        }

        public ModelType5_DeepObjectGraph(int age, decimal myNumber, Name5 name)
        {
            Age = age;
            MyNumber = myNumber;
            Name = name;
        }

        public Name5 Name { get; private set; }

        public int Age { get; private set; }

        public decimal MyNumber { get; private set; }
    }

    public class Name5
    {
        public Name5()
        {
        }

        public Name5(string firstName, string surname)
        {
            FirstName = firstName;
            Surname = surname;
        }

        public string FirstName { get; private set; }

        public string Surname { get; private set; }
    }
}
