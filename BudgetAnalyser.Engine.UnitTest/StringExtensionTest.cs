using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void AnOrAShouldReturnAnWhenFirstSentenceBeginsWithLowerVowel()
        {
            string result = "apple or banana".AnOrA();
            Assert.AreEqual("an", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnAnWhenFirstSentenceBeginsWithUpperVowel()
        {
            string result = "Apple or banana".AnOrA();
            Assert.AreEqual("an", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnAWhenFirstSentenceBeginsWithLowerVowel()
        {
            string result = "banana or apple".AnOrA();
            Assert.AreEqual("a", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnAWhenFirstSentenceBeginsWithUpperVowel()
        {
            string result = "banana or apple".AnOrA();
            Assert.AreEqual("a", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnProperCaseAnWhenFirstSentenceBeginsWithLowerVowel()
        {
            string result = "apple or banana".AnOrA(true);
            Assert.AreEqual("An", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnProperCaseAnWhenFirstSentenceBeginsWithUpperVowel()
        {
            string result = "Apple or banana".AnOrA(true);
            Assert.AreEqual("An", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnProperCaseAWhenFirstSentenceBeginsWithLowerVowel()
        {
            string result = "banana or apple".AnOrA(true);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void AnOrAShouldReturnProperCaseAWhenFirstSentenceBeginsWithUpperVowel()
        {
            string result = "banana or apple".AnOrA(true);
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void IsNothingShouldReturnFalseGivenEmpty()
        {
            string subject = string.Empty;
            Assert.IsTrue(subject.IsNothing());
        }

        [TestMethod]
        public void IsNothingShouldReturnFalseGivenNull()
        {
            string subject = null;
            Assert.IsTrue(subject.IsNothing());
        }

        [TestMethod]
        public void IsNothingShouldReturnTrueGivenAnyText()
        {
            var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
            Assert.IsFalse(subject.IsNothing());
        }

        [TestMethod]
        public void IsSomethingShouldReturnFalseGivenEmpty()
        {
            string subject = string.Empty;
            Assert.IsFalse(subject.IsSomething());
        }

        [TestMethod]
        public void IsSomethingShouldReturnFalseGivenNull()
        {
            string subject = null;
            Assert.IsFalse(subject.IsSomething());
        }

        [TestMethod]
        public void IsSomethingShouldReturnTrueGivenAnyText()
        {
            var subject = "BEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrr!";
            Assert.IsTrue(subject.IsSomething());
        }

        [TestMethod]
        public void TruncateEmptyStringTo9()
        {
            Assert.AreEqual(string.Empty, string.Empty.Truncate(9));
        }

        [TestMethod]
        public void TruncateLeftEmptyStringTo9()
        {
            Assert.AreEqual(string.Empty, string.Empty.TruncateLeft(9, true));
        }

        [TestMethod]
        public void TruncateLeftShortStringTo30()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.TruncateLeft(30));
        }

        [TestMethod]
        public void TruncateLeftValidStringTo9()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("brown fox", data1.TruncateLeft(9));
        }

        [TestMethod]
        public void TruncateLeftValidStringTo9WithEllipses()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("…rown fox", data1.TruncateLeft(9, true));
        }

        [TestMethod]
        public void TruncateShortStringTo20()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.Truncate(20));
        }

        [TestMethod]
        public void TruncateValidStringTo9()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quick", data1.Truncate(9));
        }

        [TestMethod]
        public void TruncateValidStringTo9WithEllipses()
        {
            var data1 = "The quick brown fox";

            Assert.AreEqual("The quic…", data1.Truncate(9, true));
        }
    }
}