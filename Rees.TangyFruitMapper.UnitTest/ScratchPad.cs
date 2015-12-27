
using System;
using System.CodeDom.Compiler;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{
    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType2_ModelType2_PrivateSet : IDtoMapper<DtoType2, ModelType2_PrivateSet>
    {

        public ModelType2_PrivateSet ToModel(DtoType2 dto)
        {
            var model = new ModelType2_PrivateSet();
            var modelType = model.GetType();
            var source1 = dto.Name;
            modelType.GetProperty("Name").SetValue(model, source1);
            var source2 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source2);
            var source3 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, source3);
            return model;
        } // End ToModel Method

        public DtoType2 ToDto(ModelType2_PrivateSet model)
        {
            var dto = new DtoType2();
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
