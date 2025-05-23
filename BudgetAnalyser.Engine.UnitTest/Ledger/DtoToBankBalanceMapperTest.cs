﻿using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToBankBalanceMapperTest
{
    private BankBalance Result { get; set; }

    private BankBalanceDto TestData => new(StatementModelTestData.ChequeAccount.Name, 44552.44M);

    [TestMethod]
    public void ShouldMapAmount()
    {
        Assert.AreEqual(44552.44M, Result.Balance);
    }

    [TestMethod]
    public void ShouldMapBankAccount()
    {
        Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.Account.Name);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperBankBalanceToDto2(new InMemoryAccountTypeRepository());
        Result = subject.ToModel(TestData);
    }
}
