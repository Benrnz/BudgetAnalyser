using System;

namespace Rees.TangyFruitMapper
{
    internal class SimpleAssignment : AssignDestinationStrategy
    {
        public SimpleAssignment(Type destinationType) : base(destinationType)
        {
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            return $"{DestinationObjectName(destinationKind)}.{AssignmentDestinationName} = {sourceVariableName};";
        }
    }
}