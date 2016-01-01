using System;

namespace Rees.TangyFruitMapper
{
    internal abstract class ConstructionStrategy
    {
        protected ConstructionStrategy(Type destinationType)
        {
            DestinationType = destinationType;
        }

        public Type DestinationType { get; set; }

        public abstract string CreateCodeLine(DtoOrModel destinationKind);

        protected static string DestinationObjectName(DtoOrModel destinationKind)
        {
            return destinationKind == DtoOrModel.Dto ? AssignmentStrategy.DtoVariableName : AssignmentStrategy.ModelVariableName;
        }
    }
}