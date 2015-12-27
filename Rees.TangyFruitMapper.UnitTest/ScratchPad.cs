
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{
    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType3_ModelType3_BackingField : IDtoMapper<DtoType3, ModelType3_BackingField>
    {

        public ModelType3_BackingField ToModel(DtoType3 dto)
        {
            var model = new ModelType3_BackingField();
            var modelType = model.GetType();
            var source1 = dto.Name;
            modelType.GetField("name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, source1);
            var source2 = dto.Age;
            modelType.GetField("age", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(model, source2);
            var source3 = dto.MyNumber;
            // model.MyNumber = source3; // TODO Cannot find a way to set this property: model.MyNumber.
            return model;
        } // End ToModel Method

        public DtoType3 ToDto(ModelType3_BackingField model)
        {
            var dto = new DtoType3();
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