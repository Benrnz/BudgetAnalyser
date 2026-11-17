using System.Text;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Encryption;

/// <summary>
///     DPAPI is only available on Windows, so these tests are skipped elsewhere. All values are obviously fake.
/// </summary>
public sealed class DpapiBankFeedCredentialStoreTest : IDisposable
{
    private const string FakeSecret = "fake-secret-value-for-tests";
    private const string Provider1 = "test.provider.one";
    private const string Provider2 = "test.provider.two";
    private readonly string filePath;
    private readonly string folder;

    public DpapiBankFeedCredentialStoreTest()
    {
        this.folder = Path.Combine(Path.GetTempPath(), "BudgetAnalyserTests", Guid.NewGuid().ToString("N"));
        this.filePath = Path.Combine(this.folder, "BankFeedCredentials.dat");
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, true);
        }
    }

    [Fact]
    public void Clear_ShouldRemoveOnlyThatProvider()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();
        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret });
        subject.Save(Provider2, new Dictionary<string, string> { ["Field1"] = "other" });

        subject.Clear(Provider1);

        subject.Get(Provider1, "Field1").ShouldBeNull();
        subject.Get(Provider2, "Field1").ShouldBe("other");
    }

    [Fact]
    public void File_ShouldNotContainPlainTextValues()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();

        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret });

        var bytes = File.ReadAllBytes(this.filePath);
        Encoding.UTF8.GetString(bytes).ShouldNotContain(FakeSecret);
        Encoding.Unicode.GetString(bytes).ShouldNotContain(FakeSecret);
    }

    [Fact]
    public void Get_ShouldReturnNull_GivenNothingStored()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();

        subject.Get(Provider1, "Field1").ShouldBeNull();
        subject.GetConnectionVerified(Provider1).ShouldBeNull();
    }

    [Fact]
    public void Get_ShouldReturnValues_GivenANewInstanceReadingTheSameFile()
    {
        SkipUnlessWindows();
        CreateSubject().Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret, ["Field2"] = "value2" });

        var subject = CreateSubject();

        subject.Get(Provider1, "Field1").ShouldBe(FakeSecret);
        subject.Get(Provider1, "Field2").ShouldBe("value2");
    }

    [Fact]
    public void Get_ShouldReturnNull_GivenCorruptFile()
    {
        SkipUnlessWindows();
        Directory.CreateDirectory(this.folder);
        File.WriteAllBytes(this.filePath, [1, 2, 3, 4]);

        var subject = CreateSubject();

        subject.Get(Provider1, "Field1").ShouldBeNull();
    }

    [Fact]
    public void Save_ShouldClearConnectionVerified()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();
        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret });
        subject.SetConnectionVerified(Provider1, new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero));

        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = "replacement" });

        subject.GetConnectionVerified(Provider1).ShouldBeNull();
    }

    [Fact]
    public void Save_ShouldReplaceAllValuesForProvider()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();
        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret, ["Field2"] = "value2" });

        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = "replacement" });

        subject.Get(Provider1, "Field1").ShouldBe("replacement");
        subject.Get(Provider1, "Field2").ShouldBeNull();
    }

    [Fact]
    public void SetConnectionVerified_ShouldPersist()
    {
        SkipUnlessWindows();
        var verified = new DateTimeOffset(2026, 11, 1, 2, 3, 4, TimeSpan.Zero);
        var subject = CreateSubject();
        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret });

        subject.SetConnectionVerified(Provider1, verified);

        CreateSubject().GetConnectionVerified(Provider1).ShouldBe(verified);
    }

    [Fact]
    public void SetConnectionVerified_ShouldClear_GivenNull()
    {
        SkipUnlessWindows();
        var subject = CreateSubject();
        subject.Save(Provider1, new Dictionary<string, string> { ["Field1"] = FakeSecret });
        subject.SetConnectionVerified(Provider1, new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero));

        subject.SetConnectionVerified(Provider1, null);

        CreateSubject().GetConnectionVerified(Provider1).ShouldBeNull();
        CreateSubject().Get(Provider1, "Field1").ShouldBe(FakeSecret);
    }

    private static void SkipUnlessWindows()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "DPAPI is only available on Windows.");
    }

    private DpapiBankFeedCredentialStore CreateSubject()
    {
        return new DpapiBankFeedCredentialStore(this.filePath, new FakeLogger());
    }
}
