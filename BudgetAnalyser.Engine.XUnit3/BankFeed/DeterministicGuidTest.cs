using BudgetAnalyser.Engine.BankFeed;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

public class DeterministicGuidTest
{
    private static readonly Guid DnsNamespace = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    [Fact]
    public void Create_ShouldMatchRfc9562TestVector()
    {
        // RFC 9562 Appendix A.4: UUIDv5 of "www.example.com" in the DNS namespace.
        DeterministicGuid.Create(DnsNamespace, "www.example.com").ShouldBe(new Guid("2ed6657d-e927-568b-95e1-2665a8aea6a2"));
    }

    [Fact]
    public void Create_ShouldReturnSameGuid_GivenSameInput()
    {
        DeterministicGuid.Create(DeterministicGuid.BankFeedNamespace, "provider:trans_1")
            .ShouldBe(DeterministicGuid.Create(DeterministicGuid.BankFeedNamespace, "provider:trans_1"));
    }

    [Fact]
    public void Create_ShouldReturnVersion5RfcVariant()
    {
        var result = DeterministicGuid.Create(DeterministicGuid.BankFeedNamespace, "provider:trans_1");

        result.Version.ShouldBe(5);
        result.Variant.ShouldBeInRange(8, 11); // 10xx in binary is the RFC 9562 variant.
    }

    [Fact]
    public void CreateForBankFeedTransaction_ShouldDiffer_GivenDifferentProviderKeys()
    {
        DeterministicGuid.CreateForBankFeedTransaction("provider.one", "trans_1")
            .ShouldNotBe(DeterministicGuid.CreateForBankFeedTransaction("provider.two", "trans_1"));
    }

    [Fact]
    public void CreateForBankFeedTransaction_ShouldDiffer_GivenDifferentTransactionIds()
    {
        DeterministicGuid.CreateForBankFeedTransaction("provider.one", "trans_1")
            .ShouldNotBe(DeterministicGuid.CreateForBankFeedTransaction("provider.one", "trans_2"));
    }

    [Fact]
    public void CreateForBankFeedTransaction_ShouldNeverReturnEmpty()
    {
        DeterministicGuid.CreateForBankFeedTransaction("provider.one", "trans_1").ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void CreateForBankFeedTransaction_ShouldThrow_GivenBlankId()
    {
        Should.Throw<ArgumentException>(() => DeterministicGuid.CreateForBankFeedTransaction("provider.one", " "));
    }
}
