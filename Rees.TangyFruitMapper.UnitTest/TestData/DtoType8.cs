using System.Collections.Generic;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;

namespace Rees.TangyFruitMapper.UnitTest.TestData
{
    public class DtoType8
    {
        public DtoType8()
        {
            Names = new List<NameDto5>();
        }

        public List<NameDto5> Names { get; set; }

        public int Age { get; set; }

        public decimal MyNumber { get; set; }
    }
}
