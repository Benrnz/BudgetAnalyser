namespace Rees.TangyFruitMapper
{
    internal class CommentedAssignment : AssignDestinationStrategy
    {
        private readonly string reason;

        public CommentedAssignment(string assignmentDestinationName) : base(null)
        {
            AssignmentDestinationName = assignmentDestinationName;
        }

        public CommentedAssignment(string assignmentDestinationName, string reason) : base(null)
        {
            this.reason = reason;
            AssignmentDestinationName = assignmentDestinationName;
        }

        public override string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName)
        {
            var destinationObject = DestinationObjectName(destinationKind);
            return
                $"// {destinationObject}.{AssignmentDestinationName} = {sourceVariableName}; // TODO Cannot find a way to set this property: {destinationObject}.{AssignmentDestinationName}. {this.reason}";
        }
    }
}