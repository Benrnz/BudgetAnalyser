using System.Security;
using BudgetAnalyser.Encryption;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Encryption;

public class CredentialStoreTest
{
    [Fact]
    public void Dispose_ShouldDisposeStoredObject()
    {
        var subject = new SecureStringCredentialStore();
        var credential = CreateSecureString("Foo");
        subject.SetPasskey(credential);

        subject.Dispose();

        Should.Throw<ObjectDisposedException>(() => credential.AppendChar('c'));
    }

    [Fact]
    public void RetrievePasskey_ShouldReturnNull_GivenEmptyWasStored()
    {
        var subject = new SecureStringCredentialStore();
        subject.SetPasskey(CreateSecureString(""));

        var result = (SecureString?)subject.RetrievePasskey();

        result.ShouldBeNull();
    }

    [Fact]
    public void RetrievePasskey_ShouldReturnNull_GivenNullWasStored()
    {
        var subject = new SecureStringCredentialStore();

        var result = (SecureString?)subject.RetrievePasskey();

        result.ShouldBeNull();
    }

    [Fact]
    public void RetrievePasskey_ShouldReturnSecuredString_GivenTextValue()
    {
        var subject = new SecureStringCredentialStore();
        subject.SetPasskey(CreateSecureString("Foo"));

        var result = subject.RetrievePasskey();

        result.ShouldBeOfType<SecureString>();
    }

    [Fact]
    public void RetrievePasskey_ShouldStoreForRetrieval_GivenTextValue()
    {
        var subject = new SecureStringCredentialStore();
        subject.SetPasskey(CreateSecureString("Foo"));

        var result = (SecureString?)subject.RetrievePasskey();

        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
    }

    [Fact]
    public void SetPasskey_ShouldDisposeStoredObject()
    {
        var subject = new SecureStringCredentialStore();
        var credential = CreateSecureString("Foo");
        subject.SetPasskey(credential);

        subject.SetPasskey(CreateSecureString("Bar"));

        Should.Throw<ObjectDisposedException>(() => credential.AppendChar('c'));
    }

    private static SecureString CreateSecureString(string text)
    {
        var securedText = new SecureString();
        foreach (var c in text)
        {
            securedText.AppendChar(c);
        }

        return securedText;
    }
}
