namespace Rees.TangyFruitMapper
{
    internal class CommentedFetchSource : FetchSourceStrategy
    {
        private readonly string reason;

        public CommentedFetchSource(string sourceName) : base(null, sourceName)
        {
        }

        public CommentedFetchSource(string sourceName, string reason) : base(null, sourceName)
        {
            this.reason = reason;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            return $"// var {SourceVariableName} = // TODO Cannot find a way to retrieve this property: {SourceObjectName(sourceKind)}.{SourceName}. {this.reason}";
        }
    }
}