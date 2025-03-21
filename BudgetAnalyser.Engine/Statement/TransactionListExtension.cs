﻿namespace BudgetAnalyser.Engine.Statement;

internal static class TransactionListExtension
{
    public static IEnumerable<Transaction> Merge(this IEnumerable<Transaction> instance, IEnumerable<Transaction> additionalTransactions)
    {
        var result = instance.ToList();
        result.AddRange(additionalTransactions);
        return result.OrderBy(t => t.Date);
    }
}
