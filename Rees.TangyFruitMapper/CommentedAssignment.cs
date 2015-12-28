namespace Rees.TangyFruitMapper
{
    internal class CommentedAssignment : AssignDestinationStrategy
    {
        public CommentedAssignment(string assignmentDestinationName)
        {
            AssignmentDestinationName = assignmentDestinationName;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            // return $"// var {SourceVariableName} = // TODO Cannot find a way to retrieve this property: {SourceObjectName(sourceKind)}.{SourceName}.";
            var destinationObject = DestinationObjectName(destinationKind);
            return $"// {destinationObject}.{AssignmentDestinationName} = {sourceVariableName}; // TODO Cannot find a way to set this property: {destinationObject}.{AssignmentDestinationName}.";
        }
    }
}