namespace Rees.TangyFruitMapper
{
    internal class FetchSourceUsingPropertyAccess : FetchSourceStrategy
    {
        public FetchSourceUsingPropertyAccess(string sourceName)
        {
            SourceName = sourceName;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            return $"var {SourceVariableName} = {SourceObjectName(sourceKind)}.{SourceName};";
        }
    }
}