using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    internal class MustOnlyHavePublicWriteablePropertiesRule : PreconditionPropertyRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            if (!property.CanWrite || !property.SetMethod.IsPublic)
            {
                throw new PropertiesMustBePublicWriteableException($"{property.DeclaringType}.{property.Name}");
            }
        }
    }
}