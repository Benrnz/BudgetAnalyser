namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType3_BackingField
    {
        private readonly string name;
        private readonly int age;
        private readonly decimal myNumber;

        public ModelType3_BackingField()
        {
        }

        public ModelType3_BackingField(int age, decimal myNumber, string name)
        {
            this.age = age;
            this.myNumber = myNumber;
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public int Age
        {
            get { return this.age; }
        }

        public decimal MyNumber
        {
            get { return this.myNumber; }
        }
    }
}
