using System;
using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class AccountTypeRepositoryTest
    {
        private const string Key1 = AccountTypeRepositoryConstants.Cheque;
        private const string Key2 = AccountTypeRepositoryConstants.Visa;

        [TestMethod]
        public void AddingDuplicateEntryShouldNotThrow()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            ChequeAccount data = CreateTestData2();
            subject.Add(Key1, data);
            ChequeAccount data2 = CreateTestData2();
            subject.Add(Key1, data2);
            Assert.AreEqual(data.Name, subject.GetByKey(Key1).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingEmptyKeyEntryShouldThrow()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            subject.Add(string.Empty, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void AddingNewEntryShouldBeRetrievableByKey()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            ChequeAccount data = CreateTestData2();
            subject.Add(Key1, data);
            Assert.AreEqual(data.Name, subject.GetByKey(Key1).Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullEntryShouldThrow()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            subject.Add(Key1, null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullKeyEntryShouldThrow()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            subject.Add(null, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void FindExistingValueShouldSucceed()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            subject.Add(Key1, CreateTestData());
            subject.Add(Key2, CreateTestData());

            Engine.BankAccount.Account result = subject.Find(a => a.Name == Key1);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindNonExistentValueShouldFail()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            subject.Add(Key1, CreateTestData());
            subject.Add(Key2, CreateTestData());

            Engine.BankAccount.Account result = subject.Find(a => a.Name == "Key99");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetOrCreateNewDuplicateEntryShouldNotThrow()
        {
            InMemoryAccountTypeRepository subject = CreateSubject();
            Engine.BankAccount.Account result1 = subject.GetByKey(Key1);
            Engine.BankAccount.Account result2 = subject.GetByKey(Key1);

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