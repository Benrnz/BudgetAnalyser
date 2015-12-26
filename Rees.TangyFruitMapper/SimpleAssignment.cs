namespace Rees.TangyFruitMapper
{
    internal class SimpleAssignment : AssignmentStrategy
    {
        public override string CreateCodeLine()
        {
            var destination = AssignmentDestinationIsDto ? "dto" : "model";
            var source = AssignmentDestinationIsDto ? "model" : "dto";
            return $"{destination}.{AssignmentDestination} = {source}.{AssignmentSource};";
        }
    }
}