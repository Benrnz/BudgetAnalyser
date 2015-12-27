namespace Rees.TangyFruitMapper
{
    internal class CommentedFetchSource : FetchSourceStrategy
    {
        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            return $"// var {SourceVariableName} = // TODO Cannot find a way to retrieve this property: {SourceObjectName(sourceKind)}.{SourceName}.";
        }
    }
}