﻿using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class FetchSourceByReflection : FetchSourceStrategy
    {
        private readonly FieldInfo sourceField;

        public FetchSourceByReflection(FieldInfo sourceField)
        {
            this.sourceField = sourceField;
            SourceName = this.sourceField.Name;
        }

        public override string CreateCodeLine(DtoOrModel sourceKind)
        {
            var sourceObjectName = SourceObjectName(sourceKind);
            return $@"var {SourceVariableName} = {sourceObjectName}.GetType().GetField(""{this.sourceField}"").GetValue({sourceObjectName});";
        }
    }
}