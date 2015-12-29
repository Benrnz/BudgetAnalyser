using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System.Collections.Generic;
using System;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType7_ModelType7_1Collection : IDtoMapper<DtoType7, ModelType7_1Collection>
    {

        public ModelType7_1Collection ToModel(DtoType7 dto)
        {
            var model = new ModelType7_1Collection();
            var modelType = model.GetType();
            var source1 = dto.Names.ToList();
            modelType.GetProperty("Names").SetValue(model, source1);
            var source2 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source2);
            var source3 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, source3);
            return model;
        } // End ToModel Method

        public DtoType7 ToDto(ModelType7_1Collection model)
        {
            var dto = new DtoType7();
            var source4 = model.Names.ToList();
            dto.Names = source4;
            var source5 = model.Age;
            dto.Age = source5;
            var source6 = model.MyNumber;
            dto.MyNumber = source6;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace