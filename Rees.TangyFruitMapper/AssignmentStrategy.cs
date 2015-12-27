namespace Rees.TangyFruitMapper
{
    internal class AssignmentStrategy
    {
        public const string ModelVariableName = "model";
        public const string ModelTypeVariableName = "modelType";
        public const string DtoVariableName = "dto";
        public AssignDestinationStrategy Destination { get; set; }
        public FetchSourceStrategy Source { get; set; }
    }
}