namespace Rees.TangyFruitMapper
{
    internal class FetchSourceAndMap : FetchSourceStrategy
    {
        private readonly FetchSourceStrategy unmappedSource;
        private readonly MapResult dependentMapper;

        public FetchSourceAndMap(FetchSourceStrategy unmappedSource, MapResult dependentMapper) : base(unmappedSource.SourceType, unmappedSource.SourceName)
        {
            this.unmappedSource = unmappedSource;
            this.dependentMapper = dependentMapper;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            var rawSource = this.unmappedSource.CreateCodeLine(sourceKind);
            var mapperMethod = sourceKind == DtoOrModel.Dto ? "ToModel" : "ToDto";
            var dtoType = this.dependentMapper.DtoType.Name;
            var modelType = this.dependentMapper.ModelType.Name;
            return $"{rawSource}\nvar {SourceVariableName} = new Mapper_{dtoType}_{modelType}().{mapperMethod}({this.unmappedSource.SourceVariableName});";
        }
    }
}