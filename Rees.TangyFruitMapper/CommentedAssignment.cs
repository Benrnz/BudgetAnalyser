namespace Rees.TangyFruitMapper
{
    internal class CommentedAssignment : AssignmentStrategy
    {
        private readonly string reason;

        public CommentedAssignment(string reason)
        {
            this.reason = reason;
        }

        public override string CreateCodeLine()
        {
            return $"// {AssignmentDestination} = {AssignmentSource}; // {this.reason}";
        }
    }
}