using System;

namespace Rees.TangyFruitMapper
{
    internal abstract class AssignDestinationStrategy
    {
        protected AssignDestinationStrategy(Type destinationType)
        {
            DestinationType = destinationType;
        }

        public string AssignmentDestinationName { get; set; }

        public Type DestinationType { get; set; }

        public abstract string CreateCodeLine(DtoOrModel destinationKind, string sourceVariableName);

        protected static string DestinationObjectName(DtoOrModel destinationKind)
        {
            return destinationKind == DtoOrModel.Dto ? AssignmentStrategy.DtoVariableName : AssignmentStrategy.ModelVariableName;
        }
    }
}