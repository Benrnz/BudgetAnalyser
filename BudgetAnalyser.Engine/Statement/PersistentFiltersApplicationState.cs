﻿using System;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A state persistence Dto for global filters
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Persistence.IPersistent" />
    public class PersistentFiltersApplicationState : IPersistent
    {
        /// <summary>
        ///     Gets or sets the begin date.
        /// </summary>
        public DateTime? BeginDate { get; set; }

        /// <summary>
        ///     Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     Gets the order in which this object should be loaded.
        /// </summary>
        public int LoadSequence => 50;
    }
}