using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Encryption;

public class LocalDiskReaderWriterSelectorTest
{
    [Fact]
    public void Constructor_ShouldThrow_GivenNullEncrypted()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            var subject = new LocalDiskReaderWriterSelector(null!, new LocalDiskReaderWriter());
            subject.SelectReaderWriter(false);
        });
    }

    [Fact]
    public void Constructor_ShouldThrow_GivenNullUnprotected()
    {
        var mockCredentials = Substitute.For<ICredentialStore>();
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        Should.Throw<ArgumentNullException>(() =>
        {
            var subject = new LocalDiskReaderWriterSelector(new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials), null!);
            subject.SelectReaderWriter(true);
        });
    }

    [Fact]
    public void SelectReaderWriter_ShouldReturnEncrypted_GivenTrue()
    {
        var mockCredentials = Substitute.For<ICredentialStore>();
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var subject = new LocalDiskReaderWriterSelector(
            new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials),
            new LocalDiskReaderWriter());

        var result = subject.SelectReaderWriter(true);

        result.ShouldBeOfType<EncryptedLocalDiskReaderWriter>();
    }

    [Fact]
    public void SelectReaderWriter_ShouldReturnUnprotected_GivenFalse()
    {
        var mockCredentials = Substitute.For<ICredentialStore>();
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var subject = new LocalDiskReaderWriterSelector(
            new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials),
            new LocalDiskReaderWriter());

        var result = subject.SelectReaderWriter(false);

        result.ShouldBeOfType<LocalDiskReaderWriter>();
    }
}
