namespace Rees.TangyFruitMapper
{
    internal class CommentedAssignment : AssignDestinationStrategy
    {
        private readonly string reason;

        public CommentedAssignment(string reason, string assignmentDestinationName)
        {
            this.reason = reason;
            AssignmentDestinationName = assignmentDestinationName;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            return $"// {DestinationObjectName(destinationKind)}.{AssignmentDestinationName} = {sourceVariableName}; // {this.reason}";
        }
    }
}