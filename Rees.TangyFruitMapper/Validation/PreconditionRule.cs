using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    public abstract class PreconditionRule
    {
        public abstract void IsCompliant(PropertyInfo property);
    }
}