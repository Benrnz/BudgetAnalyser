using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void TruncateEmptyStringTo9()
        {
            Assert.AreEqual(string.Empty, string.Empty.Truncate(9));
        }

        [TestMethod]
        public void TruncateValidStringTo9()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("The quick", data1.Truncate(9));
        }

        [TestMethod]
        public void TruncateShortStringTo20()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.Truncate(20));
        }

        [TestMethod]
        public void TruncateValidStringTo9WithEllipses()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("The quic…", data1.Truncate(9, true));
        }

        [TestMethod]
        public void TruncateEmptyStringTo9WithIsEmptyFlag()
        {
            bool isEmpty = false;
            Assert.AreEqual(string.Empty, string.Empty.Truncate(9, ref isEmpty));
            Assert.IsTrue(isEmpty);
        }

        [TestMethod]
        public void TruncateValidStringTo9WithIsEmptyFlagTrueAndPrefix()
        {
            string data1 = "The quick brown fox";

            bool isEmpty = true;
            Assert.AreEqual("The quic…", data1.Truncate(9, ref isEmpty, true, " / "));
            Assert.IsFalse(isEmpty);
        }

        [TestMethod]
        public void TruncateValidStringTo9WithIsEmptyFlagFalseAndPrefix()
        {
            string data1 = "The quick brown fox";

            bool isEmpty = false;
            Assert.AreEqual(" / The quic…", data1.Truncate(9, ref isEmpty, true, " / "));
            Assert.IsFalse(isEmpty);
        }

        [TestMethod]
        public void TruncateLeftValidStringTo9()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("brown fox", data1.TruncateLeft(9));
        }

        [TestMethod]
        public void TruncateLeftValidStringTo9WithEllipses()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("…rown fox", data1.TruncateLeft(9, true));
        }

        [TestMethod]
        public void TruncateLeftEmptyStringTo9()
        {
            Assert.AreEqual(string.Empty, string.Empty.TruncateLeft(9, true));
        }

        [TestMethod]
        public void TruncateLeftShortStringTo30()
        {
            string data1 = "The quick brown fox";

            Assert.AreEqual("The quick brown fox", data1.TruncateLeft(30));
        }
    }
}
