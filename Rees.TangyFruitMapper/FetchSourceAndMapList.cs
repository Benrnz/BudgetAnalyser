using System;

namespace Rees.TangyFruitMapper
{
    internal class FetchSourceAndMapList : FetchSourceStrategy
    {
        private readonly Type genericSourceType;

        public FetchSourceAndMapList(FetchSourceStrategy source, MapResult mapResult, Type genericSourceType) : base(source.SourceType, source.SourceName)
        {
            this.genericSourceType = genericSourceType;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            // TODO
            return $"// TODO List support not yet complete.";
        }
    }
}