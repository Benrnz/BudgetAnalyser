using System;

namespace Rees.TangyFruitMapper
{
    internal class NonpublicDefaultConstructor : ConstructionStrategy
    {
        public NonpublicDefaultConstructor(Type destinationType) : base(destinationType)
        {
        }

        public override string CreateCodeLine(DtoOrModel destinationKind)
        {
            return
                $@"var constructors = typeof({DestinationType.Name
                    }).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
var constructor = constructors.First(c => c.GetParameters().Length == 0);
{DestinationObjectName(destinationKind)} = ({DestinationType.Name})constructor.Invoke(new Type[] {{ }});";
        }
    }
}