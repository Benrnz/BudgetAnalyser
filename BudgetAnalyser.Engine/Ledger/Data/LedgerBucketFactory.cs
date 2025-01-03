﻿using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A factory to create <see cref="LedgerBucket" />s from minimally persisted storage data.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Ledger.Data.ILedgerBucketFactory" />
[AutoRegisterWithIoC]
internal class LedgerBucketFactory : ILedgerBucketFactory
{
    private readonly IAccountTypeRepository accountRepo;
    private readonly IBudgetBucketRepository bucketRepo;

    public LedgerBucketFactory([NotNull] IBudgetBucketRepository bucketRepo,
                               [NotNull] IAccountTypeRepository accountRepo)
    {
        this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        this.accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
    }

    public LedgerBucket Build(string bucketCode, string accountName)
    {
        var account = this.accountRepo.GetByKey(accountName);
        return Build(bucketCode, account);
    }

    public LedgerBucket Build(string bucketCode, Account account)
    {
        var bucket = this.bucketRepo.GetByCode(bucketCode);
        if (bucket is SavedUpForExpenseBucket)
        {
            return new SavedUpForLedger { BudgetBucket = bucket, StoredInAccount = account };
        }

        return bucket is SpentPerPeriodExpenseBucket
            ? (LedgerBucket)new SpentPerPeriodLedger { BudgetBucket = bucket, StoredInAccount = account }
            : throw new NotSupportedException($"Unsupported budget bucket {bucketCode} with type {bucket.GetType().Name}, found in ledger book");
    }
}
