namespace Rees.TangyFruitMapper
{
    internal class FetchSourceList : FetchSourceStrategy
    {
        public FetchSourceList(FetchSourceStrategy source) : base(source.SourceType, source.SourceName)
        {
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            // var source1 = model.Names.ToList();
            return $"var {SourceVariableName} = {SourceObjectName(sourceKind)}.{SourceName}.ToList();";
        }
    }
}