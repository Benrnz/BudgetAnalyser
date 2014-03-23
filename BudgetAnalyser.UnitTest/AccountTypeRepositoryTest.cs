using System;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class AccountTypeRepositoryTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingEmptyKeyEntryShouldThrow()
        {
            AccountTypeRepository subject = CreateSubject();
            subject.Add(string.Empty, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void AddingNewEntryShouldBeRetrievableByKey()
        {
            AccountTypeRepository subject = CreateSubject();
            AmexAccount data = CreateTestData();
            subject.Add("Key12", data);
            Assert.AreSame(data, subject.GetByKey("Key12"));
        }

        [TestMethod]
        public void AddingDuplicateEntryShouldNotThrow()
        {
            AccountTypeRepository subject = CreateSubject();
            AmexAccount data = CreateTestData();
            subject.Add("Key12", data);
            var data2 = CreateTestData();
            subject.Add("Key12", data2);
            Assert.AreSame(data, subject.GetByKey("Key12"));
        }

        [TestMethod]
        public void GetOrCreateNewDuplicateEntryShouldNotThrow()
        {
            AccountTypeRepository subject = CreateSubject();
            var result1 = subject.GetOrCreateNew("Key12");
            var result2 = subject.GetOrCreateNew("Key12");

            Assert.AreSame(result1, result2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullEntryShouldThrow()
        {
            AccountTypeRepository subject = CreateSubject();
            subject.Add("Key12", null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullKeyEntryShouldThrow()
        {
            AccountTypeRepository subject = CreateSubject();
            subject.Add(null, CreateTestData());
            Assert.Fail();
        }

        [TestMethod]
        public void FindExistingValueShouldSucceed()
        {
            var subject = CreateSubject();
            subject.Add("Key1", CreateTestData());
            subject.Add("Key2", CreateTestData());
            subject.Add("Key3", CreateTestData());
            subject.Add("Key4", CreateTestData());

            var result = subject.Find(a => a.Name == "Key12");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindNonExistentValueShouldFail()
        {
            var subject = CreateSubject();
            subject.Add("Key1", CreateTestData());
            subject.Add("Key2", CreateTestData());
            subject.Add("Key3", CreateTestData());
            subject.Add("Key4", CreateTestData());

            var result = subject.Find(a => a.Name == "Key99");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ListCurrentlyUsedAccountTypesShouldReturnCorrectCount()
        {
            var subject = CreateSubject();
            subject.Add("Key1", CreateTestData());
            subject.Add("Key2", CreateTestData());
            subject.Add("Key3", CreateTestData());
            subject.Add("Key4", CreateTestData());

            var result = subject.ListCurrentlyUsedAccountTypes();

            Assert.AreEqual(4, result.Count());
        }

        private static AmexAccount CreateTestData()
        {
            return new AmexAccount("Key12");
        }

        private AccountTypeRepository CreateSubject()
        {
            return new AccountTypeRepository();
        }
    }
}