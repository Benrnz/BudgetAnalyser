using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.XUnit.TestData;

/// <summary>
///     Embedded resource names of the Akahu API response fixtures. Every value in these files is invented.
/// </summary>
internal static class AkahuTestDataConstants
{
    public const string AccountsListResponse = "BudgetAnalyser.Engine.XUnit.TestData.AkahuAccountsListResponse.json";
    public const string TransactionsFirstPageResponse = "BudgetAnalyser.Engine.XUnit.TestData.AkahuTransactionsFirstPageResponse.json";
    public const string TransactionsLastPageResponse = "BudgetAnalyser.Engine.XUnit.TestData.AkahuTransactionsLastPageResponse.json";
    public const string UnauthorisedErrorResponse = "BudgetAnalyser.Engine.XUnit.TestData.AkahuUnauthorisedErrorResponse.json";
    public const string UserDetailsResponse = "BudgetAnalyser.Engine.XUnit.TestData.AkahuUserDetailsResponse.json";

    /// <summary>
    ///     The account in the fixtures that has transactions.
    /// </summary>
    public const string TransactionAccountId = "acc_test00000000000000000001";

    public static string Read(string resourceName)
    {
        return typeof(AkahuTestDataConstants).Assembly.ExtractEmbeddedResourceAsText(resourceName);
    }
}
