using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class FetchSourceByReflection : FetchSourceStrategy
    {
        private readonly FieldInfo sourceField;

        public FetchSourceByReflection(FieldInfo sourceField) : base(sourceField.FieldType, sourceField.Name)
        {
            this.sourceField = sourceField;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            var sourceObjectName = SourceObjectName(sourceKind);
            return $@"var {SourceVariableName} = {sourceObjectName}.GetType().GetField(""{this.sourceField}"", BindingFlags.Instance | BindingFlags.NonPublic).GetValue({sourceObjectName});";
        }
    }
}