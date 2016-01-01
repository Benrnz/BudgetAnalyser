
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using Rees.TangyFruitMapper.UnitTest.TestData;
using System;

namespace GeneratedCode
{
    
    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType1_ModelType1_AllWriteable : IDtoMapper<DtoType1, ModelType1_AllWriteable>
    {
        
        public ModelType1_AllWriteable ToModel(DtoType1 dto)
        {
            var model = new ModelType1_AllWriteable();
            var modelType = model.GetType();
            var name7 = dto.Name;
            model.Name = name7;
            var age8 = dto.Age;
            model.Age = age8;
            var myNumber9 = dto.MyNumber;
            model.MyNumber = myNumber9;
            return model;
        } // End ToModel Method

        public DtoType1 ToDto(ModelType1_AllWriteable model)
        {
            var dto = new DtoType1();
            var name10 = model.Name;
            dto.Name = name10;
            var age11 = model.Age;
            dto.Age = age11;
            var myNumber12 = model.MyNumber;
            dto.MyNumber = myNumber12;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace

