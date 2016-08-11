using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void AnOrA_ShouldReturnAn_WhenFirstSentenceBeginsWithLowerVowel()
        {
            var result = "apple or banana".AnOrA();
            Assert.AreEqual("an", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnAn_WhenFirstSentenceBeginsWithUpperVowel()
        {
            var result = "Apple or banana".AnOrA();
            Assert.AreEqual("an", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnA_WhenFirstSentenceBeginsWithLowerVowel()
        {
            var result = "banana or apple".AnOrA();
            Assert.AreEqual("a", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnA_WhenFirstSentenceBeginsWithUpperVowel()
        {
            var result = "banana or apple".AnOrA();
            Assert.AreEqual("a", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnProperCaseAn_WhenFirstSentenceBeginsWithLowerVowel()
        {
            var result = "apple or banana".AnOrA(true);
            Assert.AreEqual("An", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnProperCaseAn_WhenFirstSentenceBeginsWithUpperVowel()
        {
            var result = "Apple or banana".AnOrA(true);
            Assert.AreEqual("An", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnProperCaseA_WhenFirstSentenceBeginsWithLowerVowel()
        {
            var result = "banana or apple".AnOrA(true);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void AnOrA_ShouldReturnProperCaseA_WhenFirstSentenceBeginsWithUpperVowel()
        {
            var result = "banana or apple".AnOrA(true);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void IsNothing_ShouldReturnFalse_GivenEmpty()
        {
            var subject = string.Empty;
            Assert.IsTrue(subject.IsNothing());
        }

        [TestMethod]
        public void IsNothing_ShouldReturnFalse_GivenNull()
        {
            string subject = null;
            Assert.IsTrue(subject.IsNothing());
        }

        [TestMethod]
        public void IsNothing_ShouldReturnTrue_GivenAnyText()
        {
            var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
            Assert.IsFalse(subject.IsNothing());
        }

        [TestMethod]
        public void IsSomething_ShouldReturnFalse_GivenEmpty()
        {
            var subject = string.Empty;
            Assert.IsFalse(subject.IsSomething());
        }

        [TestMethod]
        public void IsSomething_ShouldReturnFalse_GivenNull()
        {
            string subject = null;
            Assert.IsFalse(subject.IsSomething());
        }

        [TestMethod]
        public void IsSomething_ShouldReturnTrue_GivenAnyText()
        {
            var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
            Assert.IsTrue(subject.IsSomething());
        }

        [TestMethod]
        public void Truncate_ShouldReturnEmptyString_GivenEmptyString()
        {
            Assert.AreEqual(string.Empty, string.Empty.Truncate(9));
        }

        [TestMethod]
        public void TruncateLeft_ShouldReturnEmptyString_GivenEmptyString()
        {
            Assert.AreEqual(string.Empty, string.Empty.TruncateLeft(9, true));
        }

        [TestMethod]
        public void TruncateLeft_ShouldReturnInput_GivenInputIsShorterThanRequiredLength()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.TruncateLeft(30));
        }

        [TestMethod]
        public void TruncateLeft_ShouldReturnChoppedLeftString_GivenStringLongerThanLengthRequired()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("brown fox", data1.TruncateLeft(9));
        }

        [TestMethod]
        public void TruncateLeft_ShouldReturnChoppedLeftStringWithEllipses_GivenStringLongerThanLengthRequired()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("…rown fox", data1.TruncateLeft(9, true));
        }

        [TestMethod]
        public void Truncate_ShouldReturnInput_GivenInputIsShorterThanRequiredLength()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.Truncate(20));
        }

        [TestMethod]
        public void Truncate_ShouldReturnChoppedRight_GivenStringLongerThanLengthRequired()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick", data1.Truncate(9));
        }

        [TestMethod]
        public void Truncate_ShouldReturnChoppedLeftStringWithEllipses_GivenStringLongerThanLengthRequired()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quic…", data1.Truncate(9, true));
        }

        [TestMethod]
        public void SplitLines_ShouldReturnAllLines_GivenLineCount0()
        {
            var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
            var lines = data.SplitLines(0);

            Assert.AreEqual(3, lines.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SplitLines_ShouldThrow_GivenLineCountMinus1()
        {
            "Do you like Green eggs and ham?".SplitLines(-1);
        }

        [TestMethod]
        public void SplitLines_ShouldReturn3Lines_GivenStringWith3NewLineChars()
        {
            var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
            var lines = data.SplitLines(3);

            Assert.AreEqual(3, lines.Length);
        }

        [TestMethod]
        public void SplitLines_ShouldReturnLinesWithNoDataLoss_GivenStringWith3NewLineChars()
        {
            var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
            var lines = data.SplitLines(3);

            Assert.AreEqual(92, lines.Sum(l => l.Length));
        }

        [TestMethod]
        public void SplitLines_ShouldReturnLinesWithTrailingWhitespace_GivenStringWith3NewLineChars()
        {
            var data = @"Do you like Green eggs and ham?
I do not like them Sam I am.
I do not like green eggs and ham.";
            var lines = data.SplitLines(3);
            var lastChar = lines[0].ToCharArray().Last();
            Assert.AreNotEqual('\r', lastChar);
        }

        [TestMethod]
        public void SplitLines_ShouldReturnAnEmptyArray_GivenEmptyString()
        {
            var data = @"";
            var lines = data.SplitLines(3);

            Assert.AreEqual(0, lines.Length);
        }

        [TestMethod]
        public void SplitLines_ShouldReturnNull_GivenNullString()
        {
            string data = null;
            var lines = data.SplitLines(3);

            Assert.IsNull(lines);
        }
    }
}