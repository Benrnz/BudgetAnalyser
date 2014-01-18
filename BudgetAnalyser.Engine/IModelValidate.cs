using System.Text;

namespace BudgetAnalyser.Engine
{
    public interface IModelValidate
    {
        bool Validate(StringBuilder validationMessages);
    }
}
