using System;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType10_InternalSet
    {
        public ModelType10_InternalSet()
        {
        }

        public ModelType10_InternalSet(int age, Guid fiddleStick, string name)
        {
            Age = age;
            FiddleStick = fiddleStick;
            Name = name;
        }

        public string Name { get; internal set; }

        public int Age { get; internal set; }

        public Guid FiddleStick { get; internal set; }
    }
}
