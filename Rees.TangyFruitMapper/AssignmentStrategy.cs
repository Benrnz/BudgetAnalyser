namespace Rees.TangyFruitMapper
{
    internal abstract class AssignmentStrategy
    {
        public string AssignmentDestination { get; set; }

        public string AssignmentSource { get; set; }

        public bool AssignmentDestinationIsDto { get; set; }

        public abstract string CreateCodeLine();
    }
}
