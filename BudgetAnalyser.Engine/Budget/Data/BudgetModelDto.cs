using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a Budget Model.
/// </summary>
public class BudgetModelDto
{
    /// <summary>
    ///     Gets the pay cycle for this budget. Can only be set during budget creation.
    /// </summary>
    public BudgetCycle BudgetCycle { get; set; }

    /// <summary>
    ///     Gets or sets the effective from date.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    ///     No need for a data type for <see cref="Income" />, <see cref="Expenses" />, <see cref="BudgetItem" />,
    ///     as these have no properties that need to be private.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
    public List<ExpenseDto> Expenses { get; set; }

    /// <summary>
    ///     No need for a data type for <see cref="Income" />, <see cref="Expenses" />, <see cref="BudgetItem" />,
    ///     as these have no properties that need to be private.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
    public List<IncomeDto> Incomes { get; set; }

    /// <summary>
    ///     Gets the date and time the budget model was last modified by the user.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    ///     Gets an optional comment than can be given when a change is made to the budget model.
    /// </summary>
    public string LastModifiedComment { get; set; }

    /// <summary>
    ///     Gets or sets the name of the Budget Model.
    /// </summary>
    public string Name { get; set; }
}