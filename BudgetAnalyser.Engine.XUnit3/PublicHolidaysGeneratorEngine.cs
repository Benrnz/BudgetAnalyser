using System.Text;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.XUnit;

public static class PublicHolidaysGeneratorEngine
{
    [UsedImplicitly]
    public static string Generate()
    {
        var builder = new StringBuilder();
        WriteFileHeader(builder);
        WriteMainBody(builder);
        WriteFileFooter(builder);
        return builder.ToString();
    }

    private static void WriteFileFooter(StringBuilder builder)
    {
        builder.AppendLine(@"
}");
    }

    private static void WriteFileHeader(StringBuilder builder)
    {
        builder.AppendFormat(@"
// Generated code do not directly modify
// {0}
using System.CodeDom.Compiler;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit
{{
", DateTime.Now);
    }

    private static void WriteMainBody(StringBuilder builder)
    {
        foreach (var annualHoliday in PublicHolidaysTestData.ExpectedHolidays)
        {
            WriteTestClassHeader(builder, annualHoliday);
            WriteTestClassBody(builder, annualHoliday.Key);
            WriteTestClassFooter(builder);
        }
    }

    private static void WriteTestClassBody(StringBuilder builder, int year)
    {
        builder.AppendFormat(@"
    [Fact]
    public void CorrectNumberOfHolidays()
    {{
        this.expectedHolidays.Count.ShouldBe(this.subject.Results.Count());
    }}

    [Fact]
    public void VerifyHolidays()
    {{
        this.subject.VerifyHolidays(this.expectedHolidays);
    }}
", year);
    }

    private static void WriteTestClassFooter(StringBuilder builder)
    {
        builder.AppendLine(@"
}");
    }

    private static void WriteTestClassHeader(StringBuilder builder, KeyValuePair<int, IEnumerable<DateOnly>> annualHoliday)
    {
        builder.AppendFormat(@"
[GeneratedCode(""PublicHolidaysGenerator"", ""{0}"")]
public class PublicHolidays{1}Test
{{
    private readonly List<DateOnly> expectedHolidays =
    [
", DateTime.Now, annualHoliday.Key);

        foreach (var holiday in annualHoliday.Value)
        {
            builder.AppendFormat(@"
        new({0}, {1}, {2}),
", holiday.Year, holiday.Month, holiday.Day);
        }

        builder.AppendLine(@"    ];");
        builder.AppendFormat(@"
    private readonly NewZealandPublicHolidaysTestHarness subject = new({0});
", annualHoliday.Key);
    }
}
