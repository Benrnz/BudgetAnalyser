using System;
using System.Threading;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Matching
{
    [TestClass]
    public class MatchingRuleTest
    {
        private IBudgetBucketRepository BucketRepo { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            BucketRepo = new BucketBucketRepoAlwaysFind();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfNullBucketRepo()
        {
            new MatchingRule(null);
            Assert.Fail();
        }

        [TestMethod]
        public void CtorShouldInitialiseId()
        {
            var subject = Arrange();
            Assert.AreNotEqual(Guid.Empty, subject.RuleId);
        }

        [TestMethod]
        public void CtorShouldInitialiseCreated()
        {
            var subject = Arrange();
            Assert.AreNotEqual(DateTime.MinValue, subject.Created);
        }

        [TestMethod]
        public void TwoObjectsReferringToSameAreEqual()
        {
            var subject = Arrange();
            var subject2 = subject;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoObjectsReferringToSameAreEqual2()
        {
            var subject = Arrange();
            object subject2 = subject;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        [TestMethod]
        public void AnObjectIsNotEqualWithNull()
        {
            var subject = Arrange();
            Assert.IsFalse(subject.Equals(null));
            Assert.IsTrue(subject != null);
        }

        [TestMethod]
        public void RulesWithSameIdAreConsideredEqual()
        {
            var subject = Arrange();
            var subject2 = Arrange();
            subject2.RuleId = subject.RuleId;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        [TestMethod]
        public void RulesWithDifferentIdAreConsideredNotEqual()
        {
            var subject = Arrange();
            var subject2 = Arrange();
            
            Assert.IsFalse(subject.Equals(subject2));
            Assert.IsTrue(subject != subject2);
            Assert.IsTrue(subject.GetHashCode() != subject2.GetHashCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowWhenTransactionIsNull()
        {
            var subject = Arrange();
            subject.Match(null);
            Assert.Fail();
        }

        [TestMethod]
        public void MatchShouldMatchOnDescription()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";

            var success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void MatchShouldSetLastMatchWhenMatchIsMade()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";
            subject.LastMatch = DateTime.MinValue;
            subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsTrue(subject.LastMatch > DateTime.MinValue);
        }
        
        [TestMethod]
        public void MatchShouldSetMatchCountWhenMatchIsMade()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";
            subject.MatchCount = 1;
            subject.Match(new Transaction { Description = "Testing Description" });
            Assert.AreEqual(2, subject.MatchCount);
        }

        [TestMethod]
        public void MatchShouldMatchOnAmountAndDescription()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";
            subject.Amount = 11.01M;

            var success = subject.Match(new Transaction { Description = "Testing Description", Amount = 11.01M });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void MatchShouldMatchOnReferences()
        {
            var subject = Arrange();
            subject.Reference1 = "Testing 1";
            subject.Reference2 = "Testing 2";
            subject.Reference3 = "Testing 3";

            var success = subject.Match(new Transaction { Reference1 = "Testing 1", Reference2 = "Testing 2", Reference3 = "Testing 3" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void MatchShouldMatchOnTransacionType()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";
            subject.TransactionType = "Foo";

            var success = subject.Match(new Transaction { Description = "Testing Description", TransactionType = new NamedTransaction("Foo") });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void MatchShouldNotMatchOnDescription1()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";

            var success = subject.Match(new Transaction { Description = "xxxTesting Description" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void MatchShouldNotMatchOnDescription2()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";

            var success = subject.Match(new Transaction { Description = "Testing Descriptionxxx" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void MatchShouldNotMatchOnDescription3()
        {
            var subject = Arrange();
            subject.Description = "Testing Description";
            subject.Reference1= "Ref1";

            var success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsFalse(success);
        }

        private MatchingRule Arrange()
        {
            return new MatchingRule(BucketRepo);
        }
    }
}
