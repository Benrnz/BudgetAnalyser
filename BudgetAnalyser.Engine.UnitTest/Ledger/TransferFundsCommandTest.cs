using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class TransferFundsCommandTest
    {
        private TransferFundsCommand subject;

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenBlankNarrative()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger()
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger()
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "",
                TransferAmount = 0M
            };
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenLedgersAreTheBothSurplusForSameAccount()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SurplusLedger
                {
                    BudgetBucket = new SurplusBucket(),
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SurplusLedger
                {
                    BudgetBucket = new SurplusBucket(),
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 1
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenLedgersAreTheSame()
        {
            var ledger = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.InsHomeBucket,
                StoredInAccount = StatementModelTestData.ChequeAccount
            };
            this.subject = new TransferFundsCommand
            {
                FromLedger = ledger,
                ToLedger = ledger,
                Narrative = "Foo",
                TransferAmount = 1
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenNegativeAmount()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = -20M
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenNullFromLedger()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = null,
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 1
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenNullNarrative()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = null,
                TransferAmount = 0M
            };
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenNullToLedger()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = null,
                Narrative = "Foo",
                TransferAmount = 1
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenSmallFractionalAmount()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 0.00001M
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_GivenZeroAmount()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 0M
            };

            Assert.IsFalse(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldBeTrue_GivenThisRealExample()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SurplusLedger
                {
                    BudgetBucket = new SurplusBucket(),
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.CarMtcBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 12
            };

            Assert.IsTrue(this.subject.IsValid);
        }

        [TestMethod]
        public void IsValid_ShouldIndicateBankTransferNotRequired_GivenFromAndToLedgersAreSame()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                Narrative = "Foo",
                TransferAmount = 12.34M
            };

            Assert.IsFalse(this.subject.BankTransferRequired);
        }

        [TestMethod]
        public void IsValid_ShouldIndicateBankTransferRequired_GivenFromAndToLedgersAreDifferent()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.SavingsAccount
                },
                Narrative = "Foo",
                TransferAmount = 12.34M
            };

            Assert.IsTrue(this.subject.BankTransferRequired);
        }

        [TestMethod]
        public void SettingFromLedger_ShouldPopulateAutoMatchingReference_GivenFromAndToLedgersAreDifferent()
        {
            this.subject = new TransferFundsCommand
            {
                FromLedger = new SpentPerPeriodLedger
                {
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount
                },
                ToLedger = new SavedUpForLedger
                {
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    StoredInAccount = StatementModelTestData.SavingsAccount
                },
                Narrative = "Foo",
                TransferAmount = 12.34M
            };

            Assert.IsTrue(this.subject.AutoMatchingReference.IsSomething());
        }

        [TestInitialize]
        public void TestInitialise()
        {
        }
    }
}