using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class PrivateFieldAssignment : AssignDestinationStrategy
    {
        private readonly FieldInfo sourceField;

        public PrivateFieldAssignment(FieldInfo sourceField)
        {
            this.sourceField = sourceField;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            var destinationObject = DestinationObjectName(destinationKind);
            return $"{destinationObject}.GetType().GetField({this.sourceField.Name}).SetValue({destinationObject}, {sourceVariableName});";
        }
    }
}