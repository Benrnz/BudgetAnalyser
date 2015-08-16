using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToTo", Justification = "This is definitely not a toto")]
    public class DtoToToDoCollectionMapper : MagicMapper<List<ToDoTaskDto>, ToDoCollection>
    {
    }
}