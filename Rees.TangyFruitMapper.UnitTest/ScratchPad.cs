
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using System.Collections.Generic;
using System;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType8_ModelType8_ComplexCollection : IDtoMapper<DtoType8, ModelType8_ComplexCollection>
    {

        public ModelType8_ComplexCollection ToModel(DtoType8 dto)
        {
            var model = new ModelType8_ComplexCollection();
            var modelType = model.GetType();
            var mapper1 = new Mapper_NameDto5_Name5();
            var source1 = dto.Names.Select(mapper1.ToModel).ToList();
            modelType.GetProperty("Names").SetValue(model, source1);
            var source2 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source2);
            var source3 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, source3);
            return model;
        } // End ToModel Method

        public DtoType8 ToDto(ModelType8_ComplexCollection model)
        {
            var dto = new DtoType8();
            var mapper2 = new Mapper_NameDto5_Name5();
            var source4 = model.Names.Select(mapper2.ToDto).ToList();
            dto.Names = source4;
            var source5 = model.Age;
            dto.Age = source5;
            var source6 = model.MyNumber;
            dto.MyNumber = source6;
            return dto;
        } // End ToDto Method
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_NameDto5_Name5 : IDtoMapper<NameDto5, Name5>
    {

        public Name5 ToModel(NameDto5 dto)
        {
            var model = new Name5();
            var modelType = model.GetType();
            var source7 = dto.FirstName;
            modelType.GetProperty("FirstName").SetValue(model, source7);
            var source8 = dto.Surname;
            modelType.GetProperty("Surname").SetValue(model, source8);
            return model;
        } // End ToModel Method

        public NameDto5 ToDto(Name5 model)
        {
            var dto = new NameDto5();
            var source9 = model.FirstName;
            dto.FirstName = source9;
            var source10 = model.Surname;
            dto.Surname = source10;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace
