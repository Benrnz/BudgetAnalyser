
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public partial class Mapper_DtoType1_ModelType1_AllWriteable : IDtoMapper<DtoType1, ModelType1_AllWriteable>
    {

        public virtual ModelType1_AllWriteable ToModel(DtoType1 dto)
        {
            ToModelPreprocessing(dto);
            ModelType1_AllWriteable model = null;
            ModelFactory(dto, ref model);
            if (model == null) model = new ModelType1_AllWriteable();
            var modelType = model.GetType();
            var name1 = dto.Name;
            model.Name = name1;
            var age2 = dto.Age;
            model.Age = age2;
            var myNumber3 = dto.MyNumber;
            model.MyNumber = myNumber3;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual DtoType1 ToDto(ModelType1_AllWriteable model)
        {
            ToDtoPreprocessing(model);
            DtoType1 dto;
            dto = new DtoType1();
            var name4 = model.Name;
            dto.Name = name4;
            var age5 = model.Age;
            dto.Age = age5;
            var myNumber6 = model.MyNumber;
            dto.MyNumber = myNumber6;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(DtoType1 dto);
        partial void ToDtoPreprocessing(ModelType1_AllWriteable model);
        partial void ModelFactory(DtoType1 dto, ref ModelType1_AllWriteable model);
        partial void ToModelPostprocessing(DtoType1 dto, ref ModelType1_AllWriteable model);
        partial void ToDtoPostprocessing(ref DtoType1 dto, ModelType1_AllWriteable model);
    } // End Class

} // End Namespace