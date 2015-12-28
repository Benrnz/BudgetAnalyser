using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal abstract class DtoPreconditionRule
    {
        public abstract void IsCompliant(PropertyInfo property);
    }
}
