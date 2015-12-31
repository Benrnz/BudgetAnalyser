
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System;
using Rees.TangyFruitMapper.UnitTest.TestData;

namespace GeneratedCode
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    public class Mapper_DtoType9_ModelType9_InternalConstructor : IDtoMapper<DtoType9, ModelType9_InternalConstructor>
    {

        public ModelType9_InternalConstructor ToModel(DtoType9 dto)
        {
            var constructors = typeof(ModelType9_InternalConstructor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var constructor = constructors.First(c => c.GetParameters().Length == 0);
            var model = (ModelType9_InternalConstructor)constructor.Invoke(new Type[] { });
            var modelType = model.GetType();
            var source1 = dto.Name;
            modelType.GetProperty("Name").SetValue(model, source1);
            var source2 = dto.Age;
            modelType.GetProperty("Age").SetValue(model, source2);
            var source3 = dto.Dob;
            modelType.GetProperty("Dob").SetValue(model, source3);
            return model;
        } // End ToModel Method

        public DtoType9 ToDto(ModelType9_InternalConstructor model)
        {
            var dto = new DtoType9();
            var source4 = model.Name;
            dto.Name = source4;
            var source5 = model.Age;
            dto.Age = source5;
            var source6 = model.Dob;
            dto.Dob = source6;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace