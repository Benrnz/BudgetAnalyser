using System;
using System.Collections;
using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    public class DictionariesAreNotSupportedRule : PreconditionRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
            {
                throw new NotSupportedException("Dictionaries are currently not supported.");
            }
        }
    }
}
