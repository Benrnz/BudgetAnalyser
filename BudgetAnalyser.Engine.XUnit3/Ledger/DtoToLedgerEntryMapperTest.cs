#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerEntryMapperTest
{
    public DtoToLedgerEntryMapperTest()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryToDto2(new LedgerBucketFactory(new BudgetBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);

        var book = LedgerBookTestData.TestData2();
        Control = book.Reconciliations.First(l => l.Date == new DateOnly(2013, 08, 15)).Entries.First(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
    }

    private LedgerEntry Control { get; }
    private LedgerEntry Result { get; }

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
        new
        (
            52.32M,
            TestDataConstants.PowerBucketCode,
            TestDataConstants.ChequeAccountName,
            [
                new LedgerTransactionDto
                (
                    TransactionsListModelTestData.ChequeAccount.Name,
                    140M,
                    Id: Guid.NewGuid(),
                    Narrative: "Foo...",
                    TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                    AutoMatchingReference: null,
                    Date: null
                ),
                new LedgerTransactionDto
                (
                    TransactionsListModelTestData.ChequeAccount.Name,
                    -98.56M,
                    Id: Guid.NewGuid(),
                    Narrative: "Bar...",
                    TransactionType: typeof(CreditLedgerTransaction).FullName,
                    AutoMatchingReference: null,
                    Date: null
                )
            ]
        );

    [Fact]
    public void ShouldMapBalance()
    {
        Result.Balance.ShouldBe(Control.Balance);
    }

    [Fact]
    public void ShouldMapBucketCode()
    {
        Result.LedgerBucket.BudgetBucket.Code.ShouldBe(TestDataConstants.PowerBucketCode);
    }

    [Fact]
    public void ShouldMapCorrectNumberOfTransactions()
    {
        Result.Transactions.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldMapNetAmount()
    {
        Result.NetAmount.ShouldBe(Control.NetAmount);
    }

    [Fact]
    public void ShouldSetIsNewToFalse()
    {
        ((bool)PrivateAccessor.GetField(Result, "isNew")).ShouldBeFalse();
    }
}
