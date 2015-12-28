using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class PrivateFieldAssignment : AssignDestinationStrategy
    {
        private readonly FieldInfo sourceField;

        public PrivateFieldAssignment(FieldInfo sourceField) : base(sourceField.FieldType)
        {
            this.sourceField = sourceField;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            var destinationObject = DestinationObjectName(destinationKind);
            return
                $@"{AssignmentStrategy.ModelTypeVariableName}.GetField(""{this.sourceField.Name}"", BindingFlags.Instance | BindingFlags.NonPublic).SetValue({destinationObject}, {sourceVariableName
                    });";
        }
    }
}