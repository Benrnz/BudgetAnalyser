namespace Rees.TangyFruitMapper
{
    internal class SimpleAssignment : AssignDestinationStrategy
    {
        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            return $"{DestinationObjectName(destinationKind)}.{AssignmentDestinationName} = {sourceVariableName};";
        }
    }
}