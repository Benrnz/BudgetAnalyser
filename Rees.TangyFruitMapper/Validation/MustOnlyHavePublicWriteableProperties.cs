using System.Reflection;
using Rees.TangyFruitMapper.Validation;

namespace Rees.TangyFruitMapper
{
    internal class MustOnlyHavePublicWriteableProperties : PreconditionRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            if (!property.CanWrite || !property.SetMethod.IsPublic)
            {
                throw new PropertiesMustBePublicWriteableException();
            }
        }
    }
}