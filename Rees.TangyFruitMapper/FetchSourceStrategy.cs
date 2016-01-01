using System;

namespace Rees.TangyFruitMapper
{
    internal abstract class FetchSourceStrategy
    {
        private static int VariableCounter = 1;
        private static readonly object SyncRoot = new object();

        private string variableName;

        protected FetchSourceStrategy(Type sourceType, string sourceName)
        {
            SourceType = sourceType;
            SourceName = sourceName;
        }

        public string SourceName { get; set; }

        public Type SourceType { get; set; }

        public string SourceVariableName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.variableName))
                {
                    lock (SyncRoot)
                    {
                        if (string.IsNullOrWhiteSpace(this.variableName))
                        {
                            this.variableName = $"{SourceName.ConvertPascalCaseToCamelCase()}{VariableCounter++}";
                        }
                    }
                }

                return this.variableName;
            }
        }

        public abstract string CreateCodeLine(DtoOrModel sourceKind);

        protected static string SourceObjectName(DtoOrModel sourceKind)
        {
            return sourceKind == DtoOrModel.Dto ? AssignmentStrategy.DtoVariableName : AssignmentStrategy.ModelVariableName;
        }
    }
}