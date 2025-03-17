using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToLedgerEntryMapperTest
{
    private LedgerEntry Control { get; set; }
    private LedgerEntry Result { get; set; }

    private LedgerEntryDto TestData =>
        /*
<LedgerEntryDto Balance="52.32" BucketCode="POWER">
<LedgerEntryDto.Transactions>
<scg:List x:TypeArguments="LedgerTransactionDto" Capacity="4">
<LedgerTransactionDto Account="{x:Null}" Credit="140" Debit="0" Id="601d77e5-63d5-479c-a0e5-d56a18c975f1" Narrative="Budgeted amount" TransactionType="BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction" />
<LedgerTransactionDto Account="{x:Null}" Credit="0" Debit="98.56" Id="450f9b46-010a-4508-afc5-d46042c80d02" Narrative="Power bill" TransactionType="BudgetAnalyser.Engine.Ledger.CreditLedgerTransaction" />
</scg:List>
</LedgerEntryDto.Transactions>
</LedgerEntryDto>                 */
        new()
        {
            Balance = 52.32M,
            BucketCode = TestDataConstants.PowerBucketCode,
            StoredInAccount = TestDataConstants.ChequeAccountName,
            Transactions = new List<LedgerTransactionDto>
            {
                new()
                {
                    Account = StatementModelTestData.ChequeAccount.Name,
                    Amount = 140M,
                    Id = Guid.NewGuid(),
                    Narrative = "Foo...",
                    TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                },
                new()
                {
                    Account = StatementModelTestData.ChequeAccount.Name,
                    Amount = -98.56M,
                    Id = Guid.NewGuid(),
                    Narrative = "Bar...",
                    TransactionType = typeof(CreditLedgerTransaction).FullName
                }
            }
        };

    [TestMethod]
    public void ShouldMapBalance()
    {
        Assert.AreEqual(Control.Balance, Result.Balance);
    }

    [TestMethod]
    public void ShouldMapBucketCode()
    {
        Assert.AreEqual(TestDataConstants.PowerBucketCode, Result.LedgerBucket.BudgetBucket.Code);
    }

    [TestMethod]
    public void ShouldMapCorrectNumberOfTransactions()
    {
        Assert.AreEqual(2, Result.Transactions.Count());
    }

    [TestMethod]
    public void ShouldMapNetAmount()
    {
        Assert.AreEqual(Control.NetAmount, Result.NetAmount);
    }

    [TestMethod]
    public void ShouldSetIsNewToFalse()
    {
        Assert.IsFalse((bool)PrivateAccessor.GetField(Result, "isNew"));
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryToDto2(new LedgerBucketFactory(new BucketBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);

        var book = LedgerBookTestData.TestData2();
        Control = book.Reconciliations.First(l => l.Date == new DateOnly(2013, 08, 15)).Entries.First(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
    }
}
