using System;

namespace Rees.TangyFruitMapper
{
    internal class CommentedOutConstructor : ConstructionStrategy
    {
        public CommentedOutConstructor(Type destinationType) : base(destinationType)
        {
        }

        public override string CreateCodeLine(DtoOrModel destinationKind)
        {
            var varName = DestinationObjectName(destinationKind);
            return $@"// {varName} = new {DestinationType.Name}(); // TODO unable to find an accessible constructor on {DestinationType.Name}";
        }
    }
}