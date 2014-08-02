using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class AutoMapperConfigurationTest
    {
        private static AutoMapperConfiguration Subject { get; set; }

        private static object SyncLock = new object();

        public static AutoMapperConfiguration AutoMapperConfiguration()
        {
            if (Subject == null)
            {
                lock (SyncLock)
                {
                    if (Subject == null)
                    {
                        Subject = new AutoMapperConfiguration(new ILocalAutoMapperConfiguration[]
                        {
                            new BudgetAutoMapperConfiguration(new BudgetBucketFactory(), new BucketBucketRepoAlwaysFind(), new FakeLogger()),
                            new LedgerAutoMapperConfiguration(new LedgerTransactionFactory(), new InMemoryAccountTypeRepository(), new BucketBucketRepoAlwaysFind(), new FakeLogger())
                        }).Configure();
                    }
                }
            }

            return Subject;
        }

        [TestMethod]
        public void IsConfigurationValid()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = AutoMapperConfiguration();
        }

        [TestMethod]
        public void TestAutoMapperInternalPropertyMapping()
        {
            Mapper.CreateMap<TestDto, TestClassInternalSetters>();

            var dto = new TestDto()
            {
                Description = "Foo bar...",
                Number = 339.38M,
            };

            var result = Mapper.Map<TestClassInternalSetters>(dto);

            Assert.AreEqual(dto.Number, result.Number);
            Assert.AreEqual(dto.Description, result.Description);
        }

        [TestMethod]
        public void TestAutoMapperPrivatePropertyMapping()
        {
            Mapper.CreateMap<TestDto, TestClassPrivateSetters>();

            var dto = new TestDto()
            {
                Description = "Foo bar...",
                Number = 339.38M,
            };

            var result = Mapper.Map<TestClassPrivateSetters>(dto);

            Assert.AreEqual(dto.Number, result.Number);
            Assert.AreEqual(dto.Description, result.Description);
        }

        [TestMethod]
        public void TestAutoMapperUseCtorMapping()
        {
            Mapper.CreateMap<TestDto, TestClassNoSetters>();

            var dto = new TestDto()
            {
                Description = "Foo bar...",
                Number = 339.38M,
            };

            var result = Mapper.Map<TestClassNoSetters>(dto);

            Assert.AreEqual(dto.Number, result.Number);
            Assert.AreEqual(dto.Description, result.Description);
        }

        public class TestDto
        {
            public string Description { get; set; }

            public decimal Number { get; set; }
        }

        public class TestClassInternalSetters
        {
            public string Description { get; internal set; }

            public decimal Number { get; internal set; }
        }

        public class TestClassPrivateSetters
        {
            public string Description { get; private set; }

            public decimal Number { get; private set; }
        }

        public class TestClassNoSetters
        {
            private string description;
            private decimal number;

            public TestClassNoSetters(string description, decimal number)
            {
                this.description = description;
                this.number = number;
            }

            public string Description
            {
                get { return this.description; }
            }

            public decimal Number
            {
                get { return this.number; }
            }
        }
    }
}