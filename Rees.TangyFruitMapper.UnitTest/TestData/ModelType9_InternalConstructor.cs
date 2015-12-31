using System;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class ModelType9_InternalConstructor
    {
        internal ModelType9_InternalConstructor(string name, int age, DateTime dob)
        {
            // Required for unit testing to create the object normally with code.
            Name = name;
            Age = age;
            Dob = dob;
        }

        internal ModelType9_InternalConstructor()
        {
            // Required for code generation
        }

        public string Name { get; private set; }

        public int Age { get; private set; }

        public DateTime Dob { get; private set; }
    }
}
