
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using System;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType6_ModelType6_3DeepObjectGraph : IDtoMapper<DtoType6, ModelType6_3DeepObjectGraph>
    {

        public ModelType6_3DeepObjectGraph ToModel(DtoType6 dto)
        {
            var model = new ModelType6_3DeepObjectGraph();
            var modelType = model.GetType();
            var source1 = dto.Name;
            var source2 = new Mapper_NameDto6_Name6().ToModel(source1);
            modelType.GetProperty("Name").SetValue(model, source2);
            var source3 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source3);
            var source4 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, source4);
            return model;
        } // End ToModel Method

        public DtoType6 ToDto(ModelType6_3DeepObjectGraph model)
        {
            var dto = new DtoType6();
            var source5 = model.Name;
            var source6 = new Mapper_NameDto6_Name6().ToDto(source5);
            dto.Name = source6;
            var source7 = model.Age;
            dto.Age = source7;
            var source8 = model.MyNumber;
            dto.MyNumber = source8;
            return dto;
        } // End ToDto Method
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_NameDto6_Name6 : IDtoMapper<NameDto6, Name6>
    {

        public Name6 ToModel(NameDto6 dto)
        {
            var model = new Name6();
            var modelType = model.GetType();
            var source9 = dto.FirstName;
            modelType.GetProperty("FirstName").SetValue(model, source9);
            var source10 = dto.Surname;
            modelType.GetProperty("Surname").SetValue(model, source10);
            // var source11 = // TODO Cannot find a way to retrieve this property: dto.Address.
            // model.Address = source11; // TODO Cannot find a way to set this property: model.Address.
            return model;
        } // End ToModel Method

        public NameDto6 ToDto(Name6 model)
        {
            var dto = new NameDto6();
            var source12 = model.FirstName;
            dto.FirstName = source12;
            var source13 = model.Surname;
            dto.Surname = source13;
            // var source14 = // TODO Cannot find a way to retrieve this property: model.StreetAddress.
            // dto.StreetAddress = source14; // TODO Cannot find a way to set this property: dto.StreetAddress.
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace