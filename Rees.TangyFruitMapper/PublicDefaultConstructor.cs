using System;

namespace Rees.TangyFruitMapper
{
    internal class PublicDefaultConstructor : ConstructionStrategy
    {
        public PublicDefaultConstructor(Type destinationType) : base(destinationType)
        {
        }

        public override string CreateCodeLine(DtoOrModel destinationKind)
        {
            return $"var {DestinationObjectName(destinationKind)} = new {DestinationType.Name}();";
        }
    }
}