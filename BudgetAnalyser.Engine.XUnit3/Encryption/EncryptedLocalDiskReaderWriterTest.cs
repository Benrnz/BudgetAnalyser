using System.Security;
using System.Text;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Encryption;

public class EncryptedLocalDiskReaderWriterTest
{
    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeFalse_GivenNull()
    {
        var subject = CreateSubject();
        var result = subject.IsValidAlphaNumericWithPunctuation(null);
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeFalse_GivenTextStartsWithNull()
    {
        var subject = CreateSubject();
        var bytes = new byte[] { 0x00000000, 0x00000000 };
        var chars = Encoding.UTF8.GetChars(bytes);
        var result = subject.IsValidAlphaNumericWithPunctuation(new string(chars));
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_Given1Char()
    {
        var subject = CreateSubject();
        var result = subject.IsValidAlphaNumericWithPunctuation(" ");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenAnyNumber()
    {
        var subject = CreateSubject();
        var number = Enumerable.Range(0, 10);
        var text = string.Concat(number);
        var result = subject.IsValidAlphaNumericWithPunctuation(text);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenEmpty()
    {
        var subject = CreateSubject();
        var result = subject.IsValidAlphaNumericWithPunctuation(string.Empty);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenLargeTextBlob()
    {
        var subject = CreateSubject();
        var result =
            subject.IsValidAlphaNumericWithPunctuation(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer in massa ullamcorper, aliquam augue sed, vulputate risus. Morbi et sagittis massa. Nulla nec ante aliquam, suscipit eros a, efficitur ligula. Suspendisse ac arcu mattis, dignissim libero eget, euismod elit. Nunc bibendum rutrum efficitur. Ut quis velit non tortor convallis gravida. Ut sed velit feugiat augue aliquam eleifend. Nam nulla dui, sagittis nec congue lacinia, ornare eget odio. Donec sed augue ac est vestibulum porttitor. Maecenas porttitor ligula erat, et facilisis lacus rhoncus non. Nulla consequat massa lorem, sed pellentesque nunc facilisis ultricies. Quisque at malesuada magna. Proin consectetur justo vel ipsum ornare luctus. Ut sed sodales mauris.Fusce mattis cursus risus in venenatis.Donec mollis leo at mauris lacinia accumsan.Nunc suscipit, urna vitae laoreet posuere, nunc nulla scelerisque diam, sed aliquam turpis nulla nec mauris.Sed dictum eget arcu non suscipit.Pellentesque ut semper ipsum.Integer sem nisl, tempor varius sodales a, convallis in elit.Vestibulum gravida ac quam eu fringilla. Praesent tincidunt turpis augue, eu lobortis mi convallis quis.Vivamus finibus tincidunt dolor sit amet convallis.Integer molestie tortor nec euismod eleifend.Nam vitae purus fringilla, condimentum magna tristique, vulputate est.Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.Maecenas varius orci in nunc sagittis congue.Integer mattis venenatis velit, sed venenatis nibh volutpat et.Mauris aliquet urna nulla, at auctor augue hendrerit a.Vestibulum justo diam, efficitur non accumsan quis, imperdiet a sapien.Phasellus fermentum erat augue, id eleifend eros pharetra ac.Aliquam bibendum porttitor est, eu tincidunt felis commodo imperdiet.Phasellus lobortis quis lorem quis pulvinar. Cras vitae dignissim nibh.Aliquam auctor mauris non velit aliquet, vitae pharetra lectus vestibulum.Proin et tellus diam.Mauris in nisi eu urna feugiat convallis iaculis eu dui.Cras lobortis, justo at finibus tincidunt, ligula lacus ullamcorper libero, at vestibulum lorem enim at tortor.Sed varius sapien varius arcu suscipit, a aliquet lorem pharetra.Praesent mattis nibh id leo viverra, et placerat ipsum cursus.In massa lectus, fermentum quis commodo id, blandit nec quam.Quisque pharetra tempor urna quis hendrerit.Proin dolor urna, porttitor volutpat arcu accumsan, tempor tincidunt lectus.Duis tristique euismod turpis.Duis suscipit interdum nibh, sit amet rutrum tortor faucibus quis. Morbi tempor consequat risus quis consectetur.Nullam in augue magna.Cras id porttitor neque.Duis efficitur lectus et tincidunt egestas.Ut euismod mollis massa.Nam blandit quam sed porta semper.Etiam viverra cursus iaculis.Phasellus pretium odio nec arcu malesuada, eget ultrices nisl porta.");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols1()
    {
        var subject = CreateSubject();
        var text = "~`!@#$%^&*()_-+=";
        var result = subject.IsValidAlphaNumericWithPunctuation(text);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols2()
    {
        var subject = CreateSubject();
        var text = @"\|]}{[:;""',<.>";
        var result = subject.IsValidAlphaNumericWithPunctuation(text);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenPunctuationSymbols3()
    {
        var subject = CreateSubject();
        var text = "/?";
        var result = subject.IsValidAlphaNumericWithPunctuation(text);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValidAlphaNumericWithPunctuation_ShouldBeTrue_GivenTilde()
    {
        var subject = CreateSubject();
        var result = subject.IsValidAlphaNumericWithPunctuation("~~~~~~~~");
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task LoadFromDiskAsync_ShouldReturnAnObject_GivenValidFileName()
    {
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var mockCredentialStore = Substitute.For<ICredentialStore>();
        var subject = new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentialStore);
        var password = new SecureString();
        mockCredentialStore.RetrievePasskey().Returns(password);
        var data =
            "<List x:TypeArguments=\"x:String\" Capacity=\"4\" xmlns=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">\r\n  <x:String>The</x:String>\r\n  <x:String>Quick</x:String>\r\n  <x:String>Brown</x:String>\r\n  <x:String>Fox</x:String>\r\n</List>";
        mockFileEncryptor.LoadEncryptedFileAsync("Foo", password).Returns(data);
        var result = await subject.LoadFromDiskAsync("Foo");

        result.ShouldNotBeNull();
        result.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task LoadFromDiskAsync_ShouldThrow_GivenEmptyFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.LoadFromDiskAsync(string.Empty));
    }

    [Fact]
    public async Task LoadFromDiskAsync_ShouldThrow_GivenInvalidPassword()
    {
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var mockCredentialStore = Substitute.For<ICredentialStore>();
        var subject = new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentialStore);
        var password = new SecureString();
        mockCredentialStore.RetrievePasskey().Returns(password);
        byte[] bytes = { 0, 1, 3 };
        var data = Encoding.UTF8.GetString(bytes);
        mockFileEncryptor.LoadEncryptedFileAsync("Foo", password).Returns(data);

        await Should.ThrowAsync<EncryptionKeyIncorrectException>(async () => await subject.LoadFromDiskAsync("Foo"));
    }

    [Fact]
    public async Task LoadFromDiskAsync_ShouldThrow_GivenNoPassword()
    {
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var mockCredentialStore = Substitute.For<ICredentialStore>();
        var subject = new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentialStore);
        mockCredentialStore.RetrievePasskey().Returns((object?)null);

        await Should.ThrowAsync<EncryptionKeyNotProvidedException>(async () => await subject.LoadFromDiskAsync("Foo"));
    }

    [Fact]
    public async Task LoadFromDiskAsync_ShouldThrow_GivenNullFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.LoadFromDiskAsync(null!));
    }

    [Fact]
    public async Task WriteToDiskAsync_ShouldCallFileEncryptor_GivenValidFileNameAndData()
    {
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var mockCredentialStore = Substitute.For<ICredentialStore>();
        var subject = new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentialStore);
        var password = new SecureString();
        mockCredentialStore.RetrievePasskey().Returns(password);
        mockFileEncryptor.SaveStringDataToEncryptedFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SecureString>()).Returns(Task.CompletedTask);

        await subject.WriteToDiskAsync("foo", "Foo");

        await mockFileEncryptor.Received(1).SaveStringDataToEncryptedFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SecureString>());
        mockCredentialStore.Received(1).RetrievePasskey();
    }

    [Fact]
    public async Task WriteToDiskAsync_ShouldThrow_GivenEmptyData()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.WriteToDiskAsync("Foo", string.Empty));
    }

    [Fact]
    public async Task WriteToDiskAsync_ShouldThrow_GivenEmptyFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.WriteToDiskAsync(string.Empty, "Foo"));
    }

    [Fact]
    public async Task WriteToDiskAsync_ShouldThrow_GivenNullData()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.WriteToDiskAsync("Foo", null!));
    }

    [Fact]
    public async Task WriteToDiskAsync_ShouldThrow_GivenNullFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.WriteToDiskAsync(null!, "Foo"));
    }

    private static EncryptedLocalDiskReaderWriter CreateSubject()
    {
        var mockFileEncryptor = Substitute.For<IFileEncryptor>();
        var mockCredentialStore = Substitute.For<ICredentialStore>();
        return new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentialStore);
    }
}
