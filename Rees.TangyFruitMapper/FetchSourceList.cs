using System;

namespace Rees.TangyFruitMapper
{
    internal class FetchSourceList : FetchSourceStrategy
    {
        private readonly Type genericListType;

        public FetchSourceList(FetchSourceStrategy source, Type genericListType) : base(source.SourceType, source.SourceName)
        {
            this.genericListType = genericListType;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            // TODO
            return $"// TODO List support not yet complete.";
        }
    }
}