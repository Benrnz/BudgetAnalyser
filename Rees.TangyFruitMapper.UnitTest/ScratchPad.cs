
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData.SubNamespace;
using System.Collections.Generic;
using System;
using Rees.TangyFruitMapper.UnitTest.TestData;

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
            var names5 = dto.Names.Select(mapper1.ToModel).ToList();
            modelType.GetProperty("Names").SetValue(model, names5);
            var age7 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, age7);
            var myNumber8 = dto.MyNumber;
            modelType.GetProperty("MyNumber").SetValue(model, myNumber8);
            return model;
        } // End ToModel Method

        public DtoType8 ToDto(ModelType8_ComplexCollection model)
        {
            var dto = new DtoType8();
            var mapper2 = new Mapper_NameDto5_Name5();
            var names19 = model.Names.Select(mapper2.ToDto).ToList();
            dto.Names = names19;
            var age20 = model.Age;
            dto.Age = age20;
            var myNumber21 = model.MyNumber;
            dto.MyNumber = myNumber21;
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
            var firstName36 = dto.FirstName;
            modelType.GetProperty("FirstName").SetValue(model, firstName36);
            var surname37 = dto.Surname;
            modelType.GetProperty("Surname").SetValue(model, surname37);
            return model;
        } // End ToModel Method

        public NameDto5 ToDto(Name5 model)
        {
            var dto = new NameDto5();
            var firstName38 = model.FirstName;
            dto.FirstName = firstName38;
            var surname39 = model.Surname;
            dto.Surname = surname39;
            return dto;
        } // End ToDto Method
    } // End Class


} // End Namespace