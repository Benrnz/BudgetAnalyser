using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Statement;

public class BankImportUtilitiesTest
{
    private IBudgetBucketRepository bucketRepository;
    private string[] StringArrayTestData => ["123.34", "14/04/2014", "FUEL", "42/12/2088", "A94B4FE5-4F43-43A6-8CD2-8430F45FB58D"];

    [Fact]
    public void FetchBudgetBucketWithNegativeOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchBudgetBucket(testArray, -12, this.bucketRepository));
    }

    [Fact]
    public void FetchBudgetBucketWithNullArrayShouldThrow()
    {
        var subject = CreateSubject();

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchBudgetBucket(null, 2, this.bucketRepository));
    }

    [Fact]
    public void FetchBudgetBucketWithNullBucketRepositoryShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchBudgetBucket(testArray, 2, null));
    }

    [Fact]
    public void FetchBudgetBucketWithOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchBudgetBucket(testArray, 12, this.bucketRepository));
    }

    [Fact]
    public void FetchBudgetBucketWithValidParamsShouldReturnBucketObject()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;
        var expectedResult = new SpentPerPeriodExpenseBucket("FUEL", "Fuel");
        this.bucketRepository.GetByCode("FUEL").Returns(expectedResult);

        var result = subject.FetchBudgetBucket(testArray, 2, this.bucketRepository);

        Assert.Same(expectedResult, result);
    }

    [Fact]
    public void FetchDate_WithEmptySpan_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDate(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchDate_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "14/04/2014,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDate(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchDate_WithInvalidDate_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "invalid-date,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDate(span, 0);
            Assert.Fail("Expected InvalidDataException was not thrown.");
        }
        catch (InvalidDataException ex)
        {
            // Assert
            ex.Message.ShouldContain("Unable to parse DateOnly");
        }
    }

    [Fact]
    public void FetchDate_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "14/04/2014,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDate(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Theory]
    [InlineData(0, "14/04/2014,123.45,FUEL")]
    [InlineData(1, "Test Data,14/04/2014,123.45,FUEL")]
    [InlineData(2, "TestData,FUEL,14/04/2014")]
    [InlineData(2, "TestData,FUEL,14/04/2014,")]
    public void FetchDate_WithValidDate_ShouldReturnDateOnly(int index, string data)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchDate(span, index);

        // Assert
        result.ShouldBe(new DateOnly(2014, 4, 14));
    }

    [Fact]
    public void FetchDateTime_WithEmptySpan_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDateTime(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Expected exception was thrown
        }
    }

    [Fact]
    public void FetchDateTime_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "14/04/2014,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDateTime(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchDateTime_WithInvalidDate_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "invalid-date,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDateTime(span, 0);
            Assert.Fail("Expected InvalidDataException was not thrown.");
        }
        catch (InvalidDataException ex)
        {
            // Assert
            ex.Message.ShouldContain("Unable to parse DateTime");
        }
    }

    [Fact]
    public void FetchDateTime_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "14/04/2014,123.45,FUEL";
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchDateTime(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Theory]
    [InlineData(0, "2014-04-14T04:50:06Z,123.45,FUEL")]
    [InlineData(1, "123.45,2014-04-14T04:50:06Z,123.45,FUEL")]
    [InlineData(2, "123.45,FUEL,2014-04-14T04:50:06Z,123.45")]
    [InlineData(2, "123.45,FUEL,2014-04-14T04:50:06Z,123.45,")]
    public void FetchDateTime_WithValidDate_ShouldReturnDateTime(int index, string data)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchDateTime(span, index);

        // Assert
        result.ShouldBe(new DateTime(new DateOnly(2014, 4, 14), new TimeOnly(4, 50, 6), DateTimeKind.Utc));
    }

    [Fact]
    public void FetchDateWithInvalidDateShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<InvalidDataException>(() =>
            subject.FetchDate(testArray, 3));
    }

    [Fact]
    public void FetchDateWithNegativeOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchDate(testArray, -12));
    }

    [Fact]
    public void FetchDateWithNullArrayShouldThrow()
    {
        var subject = CreateSubject();

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchDate((string[])null, 2));
    }

    [Fact]
    public void FetchDateWithOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchDate(testArray, 12));
    }

    [Fact]
    public void FetchDateWithValidDateStringShouldReturnDate()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        var result = subject.FetchDate(testArray, 1);

        Assert.IsType<DateOnly>(result);
        Assert.NotEqual(DateOnly.MinValue, result);
    }

    [Fact]
    public void FetchDecimal_WithEmptySpan_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchDecimal(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Expected exception was thrown
        }
    }

    [Fact]
    public void FetchDecimal_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "123.45,FUEL";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchDecimal(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchDecimal_WithInvalidDecimal_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "invalid-decimal,FUEL";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchDecimal(span, 0);
            Assert.Fail("Expected InvalidDataException was not thrown.");
        }
        catch (InvalidDataException ex)
        {
            // Assert
            ex.Message.ShouldContain("Unable to parse decimal");
        }
    }

    [Fact]
    public void FetchDecimal_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "123.45,FUEL";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchDecimal(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Theory]
    [InlineData(0, "123.45,FUEL")]
    [InlineData(1, "2014-04-15,123.45,FUEL")]
    [InlineData(2, "2014-04-15,\"Foo Bar\",123.45")]
    [InlineData(2, "2014-04-15,\"Foo Bar\",123.45,")]
    [InlineData(2, "2014-04-15,\"Foo Bar\", 123.45,")]
    public void FetchDecimal_WithValidDecimal_ShouldReturnDecimal(int index, string data)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchDecimal(span, index);

        // Assert
        result.ShouldBe(123.45m);
    }

    [Fact]
    public void FetchDecimalWithInvalidDecimalShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<InvalidDataException>(() =>
            subject.FetchDecimal(testArray, 2));
    }

    [Fact]
    public void FetchDecimalWithNegativeOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchDecimal(testArray, -12));
    }

    [Fact]
    public void FetchDecimalWithNullArrayShouldThrow()
    {
        var subject = CreateSubject();

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchDecimal((string[])null, 2));
    }

    [Fact]
    public void FetchDecimalWithOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchDecimal(testArray, 12));
    }

    [Fact]
    public void FetchDecimalWithValidDecimalStringShouldReturnDecimal()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        var result = subject.FetchDecimal(testArray, 0);

        Assert.IsType<decimal>(result);
        Assert.NotEqual(decimal.MinValue, result);
    }

    [Fact]
    public void FetchGuid_WithEmptySpan_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchGuid(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Expected exception was thrown
        }
    }

    [Fact]
    public void FetchGuid_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "123e4567-e89b-12d3-a456-426614174000,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchGuid(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchGuid_WithInvalidGuid_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "invalid-guid,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchGuid(span, 0);
            Assert.Fail("Expected InvalidDataException was not thrown.");
        }
        catch (InvalidDataException ex)
        {
            // Assert
            ex.Message.ShouldContain("Unable to parse Guid");
        }
    }

    [Fact]
    public void FetchGuid_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "123e4567-e89b-12d3-a456-426614174000,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchGuid(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Theory]
    [InlineData("123e4567-e89b-12d3-a456-426614174000,SomeData", 0)]
    [InlineData("2014-04-15,123e4567-e89b-12d3-a456-426614174000,SomeData", 1)]
    [InlineData("2014-04-15,SomeData,123e4567-e89b-12d3-a456-426614174000", 2)]
    [InlineData("2015-04-15,SomeData,123e4567-e89b-12d3-a456-426614174000,", 2)]
    [InlineData("Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,123e4567-e89b-12d3-a456-426614174000,", 9)]
    public void FetchGuid_WithValidGuid_ShouldReturnGuid(string data, int commaIndex)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchGuid(span, commaIndex);

        // Assert
        result.ShouldBe(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));
    }

    [Fact]
    public void FetchGuidWithInvalidGuidShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<InvalidDataException>(() =>
            subject.FetchGuid(testArray, 2));
    }

    [Fact]
    public void FetchGuidWithNegativeOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchGuid(testArray, -12));
    }

    [Fact]
    public void FetchGuidWithNullArrayShouldThrow()
    {
        var subject = CreateSubject();

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchGuid((string[])null, 2));
    }

    [Fact]
    public void FetchGuidWithOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchGuid(testArray, 12));
    }

    [Fact]
    public void FetchGuidWithValidGuidStringShouldReturnGuid()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        var result = subject.FetchGuid(testArray, 4);

        Assert.IsType<Guid>(result);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void FetchLong_WithEmptySpan_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchLong(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Expected exception was thrown
        }
    }

    [Fact]
    public void FetchLong_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "1234567890,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchLong(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchLong_WithInvalidLong_ShouldThrowInvalidDataException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "invalid-long,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchLong(span, 0);
            Assert.Fail("Expected InvalidDataException was not thrown.");
        }
        catch (InvalidDataException ex)
        {
            // Assert
            ex.Message.ShouldContain("Unable to parse long");
        }
    }

    [Fact]
    public void FetchLong_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "1234567890,SomeData";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchLong(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Theory]
    [InlineData(0, "1234567890,SomeData")]
    [InlineData(2, "ABCV,SomeData,1234567890,ee")]
    [InlineData(2, "ABCV,SomeData,1234567890")]
    [InlineData(2, "ABCV,SomeData,1234567890,")]
    public void FetchLong_WithValidLongAtIndex_ShouldReturnLong(int index, string data)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchLong(span, index);

        // Assert
        result.ShouldBe(1234567890L);
    }

    [Fact]
    public void FetchString_ShouldRemoveQuotes()
    {
        var subject = CreateSubject();
        var myData = new[] { "\"Test String\"", "no quotes", "-21.45" };

        var result1 = subject.FetchString(myData, 0);

        Assert.DoesNotContain('"', result1);
    }

    [Fact]
    public void FetchString_WithEmptySpan_ShouldReturnEmptyString()
    {
        // Arrange
        var subject = CreateSubject();
        var data = string.Empty;
        var span = data.AsSpan();

        // Act
        try
        {
            subject.FetchString(span, 0);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Expected exception was thrown
        }
    }

    [Fact]
    public void FetchString_WithIndexOutOfRange_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "Test String,Another Value";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchString(span, 50);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchString_WithNegativeIndex_ShouldThrowUnexpectedIndexException()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "Test String,Another Value";
        var span = data.AsSpan();

        // Act & Assert
        try
        {
            subject.FetchString(span, -1);
            Assert.Fail("Expected UnexpectedIndexException was not thrown.");
        }
        catch (UnexpectedIndexException ex)
        {
            // Assert
            // Exception was thrown as expected
        }
    }

    [Fact]
    public void FetchString_WithQuotedString_ShouldRemoveQuotes()
    {
        // Arrange
        var subject = CreateSubject();
        var data = "\"Quoted String\",Another Value";
        var span = data.AsSpan();

        // Act
        var result = subject.FetchString(span, 0);

        // Assert
        result.ShouldBe("Quoted String");
    }

    [Theory]
    [InlineData(0, "\"Test String\",Another Value")]
    [InlineData(1, "Nonsense,Test String,0333.11")]
    [InlineData(2, "3.14,Another Value,Test String")]
    [InlineData(2, "3.14,Another Value,Test String,")]
    public void FetchString_WithValidStringAtIndex_ShouldReturnString(int index, string data)
    {
        // Arrange
        var subject = CreateSubject();
        var span = data.AsSpan();

        // Act
        var result = subject.FetchString(span, index);

        // Assert
        result.ShouldBe("Test String");
    }

    [Fact]
    public void FetchStringShouldRemoveQuotes()
    {
        var subject = CreateSubject();
        var myData = new[] { "\"Test String\"", "no quotes", "-21.45" };

        var result1 = subject.FetchString(myData, 0);

        Assert.DoesNotContain('"', result1);
    }

    [Fact]
    public void FetchStringWithNegativeOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchString(testArray, -12));
    }

    [Fact]
    public void FetchStringWithNullArrayShouldThrow()
    {
        var subject = CreateSubject();

        Assert.Throws<ArgumentNullException>(() =>
            subject.FetchString((string[])null, 2));
    }

    [Fact]
    public void FetchStringWithOutOfRangeIndexShouldThrow()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        Assert.Throws<UnexpectedIndexException>(() =>
            subject.FetchString(testArray, 12));
    }

    [Fact]
    public void FetchStringWithValidStringShouldReturnString()
    {
        var subject = CreateSubject();
        var testArray = StringArrayTestData;

        var result = subject.FetchString(testArray, 2);

        Assert.IsType<string>(result);
        Assert.NotEqual(string.Empty, result);
        Assert.NotNull(result);
    }

    private BankImportUtilities CreateSubject()
    {
        this.bucketRepository = Substitute.For<IBudgetBucketRepository>();

        var subject = new BankImportUtilities(new FakeLogger());
        subject.ConfigureLocale(CultureInfo.CreateSpecificCulture("en-NZ"));
        return subject;
    }
}
