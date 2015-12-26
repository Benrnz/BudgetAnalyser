using System;
using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class PrivateFieldAssignment : AssignmentStrategy
    {
        private readonly Type destinationType;
        private readonly FieldInfo sourceField;

        public PrivateFieldAssignment(Type destinationType, FieldInfo sourceField)
        {
            this.destinationType = destinationType;
            this.sourceField = sourceField;
        }

        public override string CreateCodeLine()
        {
            return $"typeof({this.destinationType.FullName}).GetField({this.sourceField.Name}).SetValue({DestinationName}, {SourceName}.{AssignmentSource};";
        }
    }
}