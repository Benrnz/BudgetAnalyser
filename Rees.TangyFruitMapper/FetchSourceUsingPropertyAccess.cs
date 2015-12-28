using System;

namespace Rees.TangyFruitMapper
{
    internal class FetchSourceUsingPropertyAccess : FetchSourceStrategy
    {
        public FetchSourceUsingPropertyAccess(string sourceName, Type sourceType) : base(sourceType, sourceName)
        {
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            return $"var {SourceVariableName} = {SourceObjectName(sourceKind)}.{SourceName};";
        }
    }
}