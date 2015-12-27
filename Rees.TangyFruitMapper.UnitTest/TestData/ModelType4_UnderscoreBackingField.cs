namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType4_UnderscoreBackingField
    {
        private readonly string _name;
        private readonly int _age;
        private readonly decimal _myNumber;

        public ModelType4_UnderscoreBackingField()
        {
        }

        public ModelType4_UnderscoreBackingField(int age, decimal myNumber, string name)
        {
            this._age = age;
            this._myNumber = myNumber;
            this._name = name;
        }

        public string Name
        {
            get { return this._name; }
        }

        public int Age
        {
            get { return this._age; }
        }

        public decimal MyNumber
        {
            get { return this._myNumber; }
        }
    }
}
