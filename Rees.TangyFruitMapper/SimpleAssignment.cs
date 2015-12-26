namespace Rees.TangyFruitMapper
{
    internal class SimpleAssignment : AssignmentStrategy
    {
        public override string CreateCodeLine()
        {
            return $"{DestinationName}.{AssignmentDestination} = {SourceName}.{AssignmentSource};";
        }
    }
}