namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetModel, BudgetModelDto>))]
    public class BudgetModelToDtoMapper : MagicMapper<BudgetModel, BudgetModelDto>
    {
    }
}