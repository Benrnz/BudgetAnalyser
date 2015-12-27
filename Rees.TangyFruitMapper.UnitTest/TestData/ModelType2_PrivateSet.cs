namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType2_PrivateSet
    {
        public ModelType2_PrivateSet()
        {
        }

        public ModelType2_PrivateSet(int age, decimal myNumber, string name)
        {
            Age = age;
            MyNumber = myNumber;
            Name = name;
        }

        public string Name { get; private set; }

        public int Age { get; private set; }

        public decimal MyNumber { get; private set; }
    }
}
