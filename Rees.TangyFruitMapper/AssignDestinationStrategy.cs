namespace Rees.TangyFruitMapper
{
    internal abstract class AssignDestinationStrategy
    {
        public string AssignmentDestinationName { get; set; }

        public abstract string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName);

        protected string DestinationObjectName(DtoOrModel destinationKind)
        {
            return destinationKind == DtoOrModel.Dto ? AssignmentStrategy.DtoVariableName : AssignmentStrategy.ModelVariableName;
        }
    }
}