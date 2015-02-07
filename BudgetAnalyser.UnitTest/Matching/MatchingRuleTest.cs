using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Matching
{
    [TestClass]
    public class MatchingRuleTest
    {
        private IBudgetBucketRepository BucketRepo { get; set; }

        [TestMethod]
        public void AndMatchShouldMatchOnAmountAndDescription()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.Amount = 11.01M;

            bool success = subject.Match(new Transaction { Description = "Testing Description", Amount = 11.01M });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void AndMatchShouldMatchOnDescription()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";

            bool success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void AndMatchShouldMatchOnReferences()
        {
            MatchingRule subject = Arrange();
            subject.Reference1 = "Testing 1";
            subject.Reference2 = "Testing 2";
            subject.Reference3 = "Testing 3";

            bool success = subject.Match(new Transaction { Reference1 = "Testing 1", Reference2 = "Testing 2", Reference3 = "Testing 3" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void AndMatchShouldMatchOnTransacionType()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.TransactionType = "Foo";

            bool success = subject.Match(new Transaction { Description = "Testing Description", TransactionType = new NamedTransaction("Foo") });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void AndMatchShouldNotMatchOnDescription1()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";

            bool success = subject.Match(new Transaction { Description = "xxxTesting Description" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AndMatchShouldFailWhenBucketIsInactive()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.Bucket.Active = false;

            bool success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AndMatchShouldNotMatchOnDescription2()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";

            bool success = subject.Match(new Transaction { Description = "Testing Descriptionxxx" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AndMatchShouldNotMatchOnDescription3()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.Reference1 = "Ref1";

            bool success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void AnObjectIsNotEqualWithNull()
        {
            MatchingRule subject = Arrange();
            Assert.IsFalse(subject.Equals(null));
            Assert.IsTrue(subject != null);
        }

        [TestMethod]
        public void CtorShouldInitialiseCreated()
        {
            MatchingRule subject = Arrange();
            Assert.AreNotEqual(DateTime.MinValue, subject.Created);
        }

        [TestMethod]
        public void CtorShouldInitialiseId()
        {
            MatchingRule subject = Arrange();
            Assert.AreNotEqual(Guid.Empty, subject.RuleId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfNullBucketRepo()
        {
            new MatchingRule(null);
            Assert.Fail();
        }

        [TestMethod]
        public void MatchShouldSetLastMatchWhenMatchIsMade()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.LastMatch = DateTime.MinValue;
            subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsTrue(subject.LastMatch > DateTime.MinValue);
        }

        [TestMethod]
        public void MatchShouldSetMatchCountWhenMatchIsMade()
        {
            MatchingRule subject = Arrange();
            subject.Description = "Testing Description";
            subject.MatchCount = 1;
            subject.Match(new Transaction { Description = "Testing Description" });
            Assert.AreEqual(2, subject.MatchCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowWhenTransactionIsNull()
        {
            MatchingRule subject = Arrange();
            subject.Match(null);
            Assert.Fail();
        }

        [TestMethod]
        public void OrMatchShouldMatchOnAmountAndDescription()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Description = "Testing Description";
            subject.Amount = 11.01M;

            bool success = subject.Match(new Transaction { Description = "Testing xxxx", Amount = 11.01M });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void OrMatchShouldMatchOnDescription3()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Description = "Testing Description";
            subject.Reference1 = "Ref1";

            bool success = subject.Match(new Transaction { Description = "Testing Description" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void OrMatchShouldMatchOnReferences()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Reference1 = "Testing 1";
            subject.Reference2 = "Testing 2";
            subject.Reference3 = "Testing 3";

            bool success = subject.Match(new Transaction { Reference1 = "Testing 1", Reference3 = "Testing xxx" });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void OrMatchShouldMatchOnTransacionType()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Description = "Testing Description";
            subject.TransactionType = "Foo";

            bool success = subject.Match(new Transaction { Description = "Testing Description", TransactionType = new NamedTransaction("Foo") });
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void OrMatchShouldNotMatchOnDescription1()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Description = "Testing Description";

            bool success = subject.Match(new Transaction { Description = "xxxTesting Description" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void OrMatchShouldNotMatchOnDescription2()
        {
            MatchingRule subject = Arrange();
            subject.And = false;
            subject.Description = "Testing Description";

            bool success = subject.Match(new Transaction { Description = "Testing Descriptionxxx" });
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void RulesWithDifferentIdAreConsideredNotEqual()
        {
            MatchingRule subject = Arrange();
            MatchingRule subject2 = Arrange();

            Assert.IsFalse(subject.Equals(subject2));
            Assert.IsTrue(subject != subject2);
            Assert.IsTrue(subject.GetHashCode() != subject2.GetHashCode());
        }

        [TestMethod]
        public void RulesWithSameIdAreConsideredEqual()
        {
            MatchingRule subject = Arrange();
            MatchingRule subject2 = Arrange();
            subject2.RuleId = subject.RuleId;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BucketRepo = new BucketBucketRepoAlwaysFind();
        }

        [TestMethod]
        public void TwoObjectsReferringToSameAreEqual()
        {
            MatchingRule subject = Arrange();
            MatchingRule subject2 = subject;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoObjectsReferringToSameAreEqual2()
        {
            MatchingRule subject = Arrange();
            object subject2 = subject;
            Assert.IsTrue(subject.Equals(subject2));
            Assert.IsTrue(subject == subject2);
            Assert.IsTrue(subject.GetHashCode() == subject2.GetHashCode());
        }

        private MatchingRule Arrange()
        {
            return new MatchingRule(BucketRepo)
            {
                BucketCode = StatementModelTestData.PowerBucket.Code,
            };
        }
    }
}