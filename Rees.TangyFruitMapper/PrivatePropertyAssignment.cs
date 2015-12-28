using System;

namespace Rees.TangyFruitMapper
{
    internal class PrivatePropertyAssignment : AssignDestinationStrategy
    {
        public PrivatePropertyAssignment(string assignmentDestinationName, Type destinationType) : base(destinationType)
        {
            AssignmentDestinationName = assignmentDestinationName;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            var destinationObject = DestinationObjectName(destinationKind);
            return $@"{AssignmentStrategy.ModelTypeVariableName}.GetProperty(""{AssignmentDestinationName}"").SetValue({destinationObject}, {sourceVariableName});";
        }
    }
}