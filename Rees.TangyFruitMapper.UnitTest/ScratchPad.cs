
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
            model.Name = dto.Name;
            model.Age = dto.Age;
            model.MyNumber = dto.MyNumber;
            return model;
        } // End ToModel Method

        public DtoType2 ToDto(ModelType2_PrivateSet model)
        {
            var dto = new DtoType2();
            dto.Name = model.Name;
            dto.Age = model.Age;
            dto.MyNumber = model.MyNumber;
            return dto;
        } // End ToDto Method
    } // End Class
} // End Namespace