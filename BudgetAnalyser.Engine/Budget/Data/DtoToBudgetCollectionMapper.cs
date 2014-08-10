namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetCollectionDto, BudgetCollection>))]
    public class DtoToBudgetCollectionMapper : MagicMapper<BudgetCollectionDto, BudgetCollection>
    {
    }
}