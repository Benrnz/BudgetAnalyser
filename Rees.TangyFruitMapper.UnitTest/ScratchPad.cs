using System;
using System.CodeDom.Compiler;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{
    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType1_ModelType1_AllWriteable : IDtoMapper<DtoType1, ModelType1_AllWriteable>
    {

        public ModelType1_AllWriteable ToModel(DtoType1 dto)
        {
            var model = new ModelType1_AllWriteable();
            var modelType = model.GetType();
            var source1 = dto.Name;
            model.Name = source1;
            var source2 = dto.Age;
            model.Age = source2;
            var source3 = dto.MyNumber;
            model.MyNumber = source3;
            return model;
        } // End ToModel Method

        public DtoType1 ToDto(ModelType1_AllWriteable model)
        {
            var dto = new DtoType1();
            var source4 = model.Name;
            dto.Name = source4;
            var source5 = model.Age;
            dto.Age = source5;
            var source6 = model.MyNumber;
            dto.MyNumber = source6;
            return dto;
        } // End ToDto Method
    } // End Class
} // End Namespace