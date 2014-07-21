namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetModelDto, BudgetModel>))]
    public class DtoToBudgetModelMapper : MagicMapper<BudgetModelDto, BudgetModel>
    {
    }
}