using System;
using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Account
{
    [TestClass]
    public class AccountTypeRepositoryTest
    {
        private const string Key1 = AccountTypeRepositoryConstants.Cheque;
        private const string Key2 = AccountTypeRepositoryConstants.Visa;

        [TestMethod]
        public void AddingDuplicateEntryShouldNotThrow()
        {
            var subject = CreateSubject();
            var data = CreateTestData2();
            subject.Add(Key1, data);
            var data2 = CreateTestData2();
            subject.Add(Key1, data2);
            Assert.AreEqual(data.Name, subject.GetByKey(Key1).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingEmptyKeyEntryShouldThrow()
        {
            var subject = CreateSubject();
            subject.Add(string.Empty, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void AddingNewEntryShouldBeRetrievableByKey()
        {
            var subject = CreateSubject();
            var data = CreateTestData2();
            subject.Add(Key1, data);
            Assert.AreEqual(data.Name, subject.GetByKey(Key1).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullEntryShouldThrow()
        {
            var subject = CreateSubject();
            subject.Add(Key1, null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullKeyEntryShouldThrow()
        {
            var subject = CreateSubject();
            subject.Add(null, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void FindExistingValueShouldSucceed()
        {
            var subject = CreateSubject();
            subject.Add(Key1, CreateTestData());
            subject.Add(Key2, CreateTestData());

            var result = subject.Find(a => a.Name == Key1);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindNonExistentValueShouldFail()
        {
            var subject = CreateSubject();
            subject.Add(Key1, CreateTestData());
            subject.Add(Key2, CreateTestData());

            var result = subject.Find(a => a.Name == "Key99");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetOrCreateNewDuplicateEntryShouldNotThrow()
        {
            var subject = CreateSubject();
            var result1 = subject.GetByKey(Key1);
            var result2 = subject.GetByKey(Key1);

            Assert.AreSame(result1, result2);
        }

        private static AmexAccount CreateTestData()
        {
            return new AmexAccount(AccountTypeRepositoryConstants.Amex);
        }

        private static ChequeAccount CreateTestData2()
        {
            return new ChequeAccount(AccountTypeRepositoryConstants.Cheque);
        }

        private InMemoryAccountTypeRepository CreateSubject()
        {
            return new InMemoryAccountTypeRepository();
        }
    }
}
