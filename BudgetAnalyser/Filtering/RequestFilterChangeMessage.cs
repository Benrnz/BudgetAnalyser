﻿using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Filtering;

public class RequestFilterChangeMessage : MessageBase
{
    public RequestFilterChangeMessage(object? sender)
    {
        Sender = sender;
    }

    public required GlobalFilterCriteria Criteria { get; init; }
}
