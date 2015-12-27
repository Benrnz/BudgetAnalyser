namespace Rees.TangyFruitMapper
{
    internal abstract class FetchSourceStrategy
    {
        private static int VariableCounter = 1;
        private static readonly object SyncRoot = new object();

        private string variableName = null;

        public string SourceName { get; set; }

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
                            this.variableName = $"source{VariableCounter++}";
                        }
                    }
                }

                return this.variableName;
            }
        }

        public abstract string CreateCodeLine(DtoOrModel sourceKind);

        protected string SourceObjectName(DtoOrModel sourceKind)
        {
            return sourceKind == DtoOrModel.Dto ? AssignmentStrategy.DtoVariableName : AssignmentStrategy.ModelVariableName;
        }
    }
}