
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType5_ModelType5_DeepObjectGraph : IDtoMapper<DtoType5, ModelType5_DeepObjectGraph>
    {

        public ModelType5_DeepObjectGraph ToModel(DtoType5 dto)
        {
            var model = new ModelType5_DeepObjectGraph();
            var modelType = model.GetType();
            var source1 = dto.Name;
            var source2 = new Mapper_NameDto5_Name().ToModel(source1);
            modelType.GetProperty("Name").SetValue(model, source2);
            var source3 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source3);
            var source4 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, source4);
            return model;
        } // End ToModel Method

        public DtoType5 ToDto(ModelType5_DeepObjectGraph model)
        {
            var dto = new DtoType5();
            var source5 = model.Name;
            var source6 = new Mapper_NameDto5_Name().ToDto(source5);
            dto.Name = source6;
            var source7 = model.Age;
            dto.Age = source7;
            var source8 = model.MyNumber;
            dto.MyNumber = source8;
            return dto;
        } // End ToDto Method
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_NameDto5_Name : IDtoMapper<NameDto5, Name>
    {

        public Name ToModel(NameDto5 dto)
        {
            var model = new Name();
            var modelType = model.GetType();
            var source9 = dto.FirstName;
            modelType.GetProperty("FirstName").SetValue(model, source9);
            var source10 = dto.Surname;
            modelType.GetProperty("Surname").SetValue(model, source10);
            return model;
        } // End ToModel Method

        public NameDto5 ToDto(Name model)
        {
            var dto = new NameDto5();
            var source11 = model.FirstName;
            dto.FirstName = source11;
            var source12 = model.Surname;
            dto.Surname = source12;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace