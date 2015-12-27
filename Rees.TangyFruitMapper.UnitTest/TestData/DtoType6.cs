using System.Collections.Generic;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class DtoType6
    {
        public DtoType6()
        {
            Names = new List<string>();
        }

        public List<string> Names { get; set; }

        public int Age { get; set; }

        public decimal MyNumber { get; set; }
    }
}
