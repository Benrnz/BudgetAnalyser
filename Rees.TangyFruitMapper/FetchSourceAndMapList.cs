namespace Rees.TangyFruitMapper
{
    internal class FetchSourceAndMapList : FetchSourceStrategy
    {
        private static int MapperCounter = 1;
        private readonly MapResult mapResult;

        public FetchSourceAndMapList(FetchSourceStrategy source, MapResult mapResult) : base(source.SourceType, source.SourceName)
        {
            this.mapResult = mapResult;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            // var listMapper = new Mapper_NameDto5_Name5();
            // var source1 = dto.Names.Select(listMapper.ToModel); 
            var mapperVar = GetMapperVariableName();
            return
                $@"var {mapperVar} = new {this.mapResult.MapperName}();
var {SourceVariableName} = {SourceObjectName(sourceKind)}.{SourceName}.Select({mapperVar}.{MapperMethodToCall(sourceKind)
                    }).ToList();";
        }

        private string GetMapperVariableName()
        {
            return $"mapper{MapperCounter++}";
        }

        private string MapperMethodToCall(DtoOrModel sourceKind)
        {
            return sourceKind == DtoOrModel.Dto ? "ToModel" : "ToDto";
        }
    }
}