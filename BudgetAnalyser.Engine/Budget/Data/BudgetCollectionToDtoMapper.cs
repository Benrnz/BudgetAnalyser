namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetCollection, BudgetCollectionDto>))]
    public class BudgetCollectionToDtoMapper : MagicMapper<BudgetCollection, BudgetCollectionDto>
    {
    }
}