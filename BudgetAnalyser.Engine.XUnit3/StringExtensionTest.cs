using Shouldly;

namespace BudgetAnalyser.Engine.XUnit;

public class StringExtensionTest
{
    [Fact]
    public void AnOrA_ShouldReturnA_WhenFirstSentenceBeginsWithLowerVowel()
    {
        var result = "banana or apple".AnOrA();
        result.ShouldBe("a");
    }

    [Fact]
    public void AnOrA_ShouldReturnA_WhenFirstSentenceBeginsWithUpperVowel()
    {
        var result = "banana or apple".AnOrA();
        result.ShouldBe("a");
    }

    [Fact]
    public void AnOrA_ShouldReturnAn_WhenFirstSentenceBeginsWithLowerVowel()
    {
        var result = "apple or banana".AnOrA();
        result.ShouldBe("an");
    }

    [Fact]
    public void AnOrA_ShouldReturnAn_WhenFirstSentenceBeginsWithUpperVowel()
    {
        var result = "Apple or banana".AnOrA();
        result.ShouldBe("an");
    }

    [Fact]
    public void AnOrA_ShouldReturnProperCaseA_WhenFirstSentenceBeginsWithLowerVowel()
    {
        var result = "banana or apple".AnOrA(true);
        result.ShouldBe("A");
    }

    [Fact]
    public void AnOrA_ShouldReturnProperCaseA_WhenFirstSentenceBeginsWithUpperVowel()
    {
        var result = "banana or apple".AnOrA(true);
        result.ShouldBe("A");
    }

    [Fact]
    public void AnOrA_ShouldReturnProperCaseAn_WhenFirstSentenceBeginsWithLowerVowel()
    {
        var result = "apple or banana".AnOrA(true);
        result.ShouldBe("An");
    }

    [Fact]
    public void AnOrA_ShouldReturnProperCaseAn_WhenFirstSentenceBeginsWithUpperVowel()
    {
        var result = "Apple or banana".AnOrA(true);
        result.ShouldBe("An");
    }

    [Fact]
    public void IsNothing_ShouldReturnFalse_GivenEmpty()
    {
        var subject = string.Empty;
        subject.IsNothing().ShouldBeTrue();
    }

    [Fact]
    public void IsNothing_ShouldReturnFalse_GivenNull()
    {
        string? subject = null;
        subject.IsNothing().ShouldBeTrue();
    }

    [Fact]
    public void IsNothing_ShouldReturnTrue_GivenAnyText()
    {
        var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
        subject.IsNothing().ShouldBeFalse();
    }

    [Fact]
    public void IsSomething_ShouldReturnFalse_GivenEmpty()
    {
        var subject = string.Empty;
        subject.IsSomething().ShouldBeFalse();
    }

    [Fact]
    public void IsSomething_ShouldReturnFalse_GivenNull()
    {
        string? subject = null;
        subject.IsSomething().ShouldBeFalse();
    }

    [Fact]
    public void IsSomething_ShouldReturnTrue_GivenAnyText()
    {
        var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
        subject.IsSomething().ShouldBeTrue();
    }

    [Fact]
    public void SplitLines_ShouldReturn3Lines_GivenStringWith3NewLineChars()
    {
        var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
        var lines = data.SplitLines(3);

        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void SplitLines_ShouldReturnAllLines_GivenLineCount0()
    {
        var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
        var lines = data.SplitLines();

        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void SplitLines_ShouldReturnAnEmptyArray_GivenEmptyString()
    {
        var data = string.Empty;
        var lines = data.SplitLines(3);

        lines.Length.ShouldBe(0);
    }

    [Fact]
    public void SplitLines_ShouldReturnLinesWithNoDataLoss_GivenStringWith3NewLineChars()
    {
        var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
        var lines = data.SplitLines(3);

        lines.Sum(l => l.Length).ShouldBe(92);
    }

    [Fact]
    public void SplitLines_ShouldReturnLinesWithTrailingWhitespace_GivenStringWith3NewLineChars()
    {
        var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
        var lines = data.SplitLines(3);
        var lastChar = lines[0].ToCharArray().Last();
        lastChar.ShouldNotBe('\r');
    }

    [Fact]
    public void SplitLines_ShouldThrow_GivenLineCountMinus1()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => "Do you like Green eggs and ham?".SplitLines(-1));
    }

    [Fact]
    public void SplitLines_ShouldThrow_GivenNullString()
    {
        string? data = null;
        Should.Throw<ArgumentNullException>(() => data!.SplitLines(3));
    }

    [Fact]
    public void Truncate_ShouldReturnChoppedLeftStringWithEllipses_GivenStringLongerThanLengthRequired()
    {
        var data1 = "The quick brown fox";

        data1.Truncate(9, true).ShouldBe("The quic…");
    }

    [Fact]
    public void Truncate_ShouldReturnChoppedRight_GivenStringLongerThanLengthRequired()
    {
        var data1 = "The quick brown fox";

        data1.Truncate(9).ShouldBe("The quick");
    }

    [Fact]
    public void Truncate_ShouldReturnEmptyString_GivenEmptyString()
    {
        string.Empty.Truncate(9).ShouldBe(string.Empty);
    }

    [Fact]
    public void Truncate_ShouldReturnInput_GivenInputIsShorterThanRequiredLength()
    {
        var data1 = "The quick brown fox";

        data1.Truncate(20).ShouldBe("The quick brown fox");
    }

    [Fact]
    public void TruncateLeft_ShouldReturnChoppedLeftString_GivenStringLongerThanLengthRequired()
    {
        var data1 = "The quick brown fox";

        data1.TruncateLeft(9).ShouldBe("brown fox");
    }

    [Fact]
    public void TruncateLeft_ShouldReturnChoppedLeftStringWithEllipses_GivenStringLongerThanLengthRequired()
    {
        var data1 = "The quick brown fox";

        data1.TruncateLeft(9, true).ShouldBe("…rown fox");
    }

    [Fact]
    public void TruncateLeft_ShouldReturnEmptyString_GivenEmptyString()
    {
        string.Empty.TruncateLeft(9, true).ShouldBe(string.Empty);
    }

    [Fact]
    public void TruncateLeft_ShouldReturnInput_GivenInputIsShorterThanRequiredLength()
    {
        var data1 = "The quick brown fox";

        data1.TruncateLeft(30).ShouldBe("The quick brown fox");
    }
}
