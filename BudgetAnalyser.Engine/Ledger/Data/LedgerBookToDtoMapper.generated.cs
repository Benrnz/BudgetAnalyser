using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Ledger;
using System;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Mobile;

namespace BudgetAnalyser.Engine.Ledger.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerBookDto_LedgerBook : IDtoMapper<LedgerBookDto, LedgerBook>
    {

        public virtual LedgerBook ToModel(LedgerBookDto dto)
        {
            ToModelPreprocessing(dto);
            LedgerBook model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(LedgerBook).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (LedgerBook)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var mapper1 = new Mapper_LedgerBucketDto_LedgerBucket(this.bucketRepo, this.accountTypeRepo, this.bucketFactory);
            var ledgers1 = dto.Ledgers.Select(mapper1.ToModel).ToList();
            modelType.GetProperty("Ledgers").SetValue(model, ledgers1);
            var modified2 = dto.Modified;
            modelType.GetProperty("Modified").SetValue(model, modified2);
            var name3 = dto.Name;
            modelType.GetProperty("Name").SetValue(model, name3);
            var mapper2 = new Mapper_LedgerEntryLineDto_LedgerEntryLine(this.accountTypeRepo, this.bucketFactory, this.transactionFactory);
            var reconciliations4 = dto.Reconciliations.Select(mapper2.ToModel).ToList();
            modelType.GetProperty("Reconciliations").SetValue(model, reconciliations4);
            var storageKey5 = dto.StorageKey;
            modelType.GetProperty("StorageKey").SetValue(model, storageKey5);
            var mapper3 = new Mapper_MobileSettingsDto_MobileSettings();
            model.MobileSettings = mapper3.ToModel(dto.MobileSettings);

            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerBookDto ToDto(LedgerBook model)
        {
            ToDtoPreprocessing(model);
            LedgerBookDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerBookDto();
            }
            var mapper3 = new Mapper_LedgerBucketDto_LedgerBucket(this.bucketRepo, this.accountTypeRepo, this.bucketFactory);
            var ledgers7 = model.Ledgers.Select(mapper3.ToDto).ToList();
            dto.Ledgers = ledgers7;
            var modified8 = model.Modified;
            dto.Modified = modified8;
            var name9 = model.Name;
            dto.Name = name9;
            var mapper4 = new Mapper_LedgerEntryLineDto_LedgerEntryLine(this.accountTypeRepo, this.bucketFactory, this.transactionFactory);
            var reconciliations10 = model.Reconciliations.Select(mapper4.ToDto).ToList();
            dto.Reconciliations = reconciliations10;
            var storageKey11 = model.StorageKey;
            dto.StorageKey = storageKey11;
            var mapper5 = new Mapper_MobileSettingsDto_MobileSettings();
            dto.MobileSettings = mapper5.ToDto(model.MobileSettings);

            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerBookDto dto);
        partial void ToDtoPreprocessing(LedgerBook model);
        partial void ModelFactory(LedgerBookDto dto, ref LedgerBook model);
        partial void DtoFactory(ref LedgerBookDto dto, LedgerBook model);
        partial void ToModelPostprocessing(LedgerBookDto dto, ref LedgerBook model);
        partial void ToDtoPostprocessing(ref LedgerBookDto dto, LedgerBook model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerBucketDto_LedgerBucket : IDtoMapper<LedgerBucketDto, LedgerBucket>
    {

        public virtual LedgerBucket ToModel(LedgerBucketDto dto)
        {
            ToModelPreprocessing(dto);
            LedgerBucket model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(LedgerBucket).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (LedgerBucket)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            model.Id = dto.Id;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerBucketDto ToDto(LedgerBucket model)
        {
            ToDtoPreprocessing(model);
            LedgerBucketDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerBucketDto();
            }
            dto.Id = model.Id;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerBucketDto dto);
        partial void ToDtoPreprocessing(LedgerBucket model);
        partial void ModelFactory(LedgerBucketDto dto, ref LedgerBucket model);
        partial void DtoFactory(ref LedgerBucketDto dto, LedgerBucket model);
        partial void ToModelPostprocessing(LedgerBucketDto dto, ref LedgerBucket model);
        partial void ToDtoPostprocessing(ref LedgerBucketDto dto, LedgerBucket model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerEntryLineDto_LedgerEntryLine : IDtoMapper<LedgerEntryLineDto, LedgerEntryLine>
    {

        public virtual LedgerEntryLine ToModel(LedgerEntryLineDto dto)
        {
            ToModelPreprocessing(dto);
            LedgerEntryLine model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(LedgerEntryLine).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (LedgerEntryLine)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var mapper5 = new Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction(this.accountTypeRepo);
            var bankBalanceAdjustments18 = dto.BankBalanceAdjustments.Select(mapper5.ToModel).ToList();
            modelType.GetProperty("BankBalanceAdjustments").SetValue(model, bankBalanceAdjustments18);
            var mapper6 = new Mapper_BankBalanceDto_BankBalance(this.accountTypeRepo);
            var bankBalances19 = dto.BankBalances.Select(mapper6.ToModel).ToList();
            modelType.GetProperty("BankBalances").SetValue(model, bankBalances19);
            var date20 = dto.Date;
            modelType.GetProperty("Date").SetValue(model, date20);
            var mapper7 = new Mapper_LedgerEntryDto_LedgerEntry(this.bucketFactory, this.transactionFactory, this.accountTypeRepo);
            var entries21 = dto.Entries.Select(mapper7.ToModel).ToList();
            modelType.GetProperty("Entries").SetValue(model, entries21);
            var remarks22 = dto.Remarks;
            modelType.GetProperty("Remarks").SetValue(model, remarks22);
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerEntryLineDto ToDto(LedgerEntryLine model)
        {
            ToDtoPreprocessing(model);
            LedgerEntryLineDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerEntryLineDto();
            }
            var mapper8 = new Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction(this.accountTypeRepo);
            var bankBalanceAdjustments24 = model.BankBalanceAdjustments.Select(mapper8.ToDto).ToList();
            dto.BankBalanceAdjustments = bankBalanceAdjustments24;
            var mapper9 = new Mapper_BankBalanceDto_BankBalance(this.accountTypeRepo);
            var bankBalances25 = model.BankBalances.Select(mapper9.ToDto).ToList();
            dto.BankBalances = bankBalances25;
            var date26 = model.Date;
            dto.Date = date26;
            var mapper10 = new Mapper_LedgerEntryDto_LedgerEntry(this.bucketFactory, this.transactionFactory, this.accountTypeRepo);
            var entries27 = model.Entries.Select(mapper10.ToDto).ToList();
            dto.Entries = entries27;
            var remarks28 = model.Remarks;
            dto.Remarks = remarks28;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerEntryLineDto dto);
        partial void ToDtoPreprocessing(LedgerEntryLine model);
        partial void ModelFactory(LedgerEntryLineDto dto, ref LedgerEntryLine model);
        partial void DtoFactory(ref LedgerEntryLineDto dto, LedgerEntryLine model);
        partial void ToModelPostprocessing(LedgerEntryLineDto dto, ref LedgerEntryLine model);
        partial void ToDtoPostprocessing(ref LedgerEntryLineDto dto, LedgerEntryLine model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction : IDtoMapper<LedgerTransactionDto, BankBalanceAdjustmentTransaction>
    {

        public virtual BankBalanceAdjustmentTransaction ToModel(LedgerTransactionDto dto)
        {
            ToModelPreprocessing(dto);
            BankBalanceAdjustmentTransaction model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(BankBalanceAdjustmentTransaction).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (BankBalanceAdjustmentTransaction)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var amount30 = dto.Amount;
            modelType.GetProperty("Amount").SetValue(model, amount30);
            var autoMatchingReference31 = dto.AutoMatchingReference;
            modelType.GetProperty("AutoMatchingReference").SetValue(model, autoMatchingReference31);
            var date32 = dto.Date;
            modelType.GetProperty("Date").SetValue(model, date32);
            var id33 = dto.Id;
            modelType.GetProperty("Id").SetValue(model, id33);
            var narrative34 = dto.Narrative;
            modelType.GetProperty("Narrative").SetValue(model, narrative34);
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerTransactionDto ToDto(BankBalanceAdjustmentTransaction model)
        {
            ToDtoPreprocessing(model);
            LedgerTransactionDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerTransactionDto();
            }
            var amount36 = model.Amount;
            dto.Amount = amount36;
            var autoMatchingReference37 = model.AutoMatchingReference;
            dto.AutoMatchingReference = autoMatchingReference37;
            var date38 = model.Date;
            dto.Date = date38;
            var id39 = model.Id;
            dto.Id = id39;
            var narrative40 = model.Narrative;
            dto.Narrative = narrative40;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerTransactionDto dto);
        partial void ToDtoPreprocessing(BankBalanceAdjustmentTransaction model);
        partial void ModelFactory(LedgerTransactionDto dto, ref BankBalanceAdjustmentTransaction model);
        partial void DtoFactory(ref LedgerTransactionDto dto, BankBalanceAdjustmentTransaction model);
        partial void ToModelPostprocessing(LedgerTransactionDto dto, ref BankBalanceAdjustmentTransaction model);
        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, BankBalanceAdjustmentTransaction model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_BankBalanceDto_BankBalance : IDtoMapper<BankBalanceDto, BankBalance>
    {

        public virtual BankBalance ToModel(BankBalanceDto dto)
        {
            ToModelPreprocessing(dto);
            BankBalance model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                // model = new BankBalance(); 
            }
            var modelType = model.GetType();
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual BankBalanceDto ToDto(BankBalance model)
        {
            ToDtoPreprocessing(model);
            BankBalanceDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new BankBalanceDto();
            }
            var balance46 = model.Balance;
            dto.Balance = balance46;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(BankBalanceDto dto);
        partial void ToDtoPreprocessing(BankBalance model);
        partial void ModelFactory(BankBalanceDto dto, ref BankBalance model);
        partial void DtoFactory(ref BankBalanceDto dto, BankBalance model);
        partial void ToModelPostprocessing(BankBalanceDto dto, ref BankBalance model);
        partial void ToDtoPostprocessing(ref BankBalanceDto dto, BankBalance model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerEntryDto_LedgerEntry : IDtoMapper<LedgerEntryDto, LedgerEntry>
    {

        public virtual LedgerEntry ToModel(LedgerEntryDto dto)
        {
            LedgerEntry model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(LedgerEntry).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (LedgerEntry)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            ToModelPreprocessing(dto, model);
            var balance47 = dto.Balance;
            modelType.GetProperty("Balance").SetValue(model, balance47);
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerEntryDto ToDto(LedgerEntry model)
        {
            ToDtoPreprocessing(model);
            LedgerEntryDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerEntryDto();
            }
            var balance49 = model.Balance;
            dto.Balance = balance49;
            var mapper11 = new Mapper_LedgerTransactionDto_LedgerTransaction(this.transactionFactory, this.accountTypeRepo);
            var transactions52 = model.Transactions.Select(mapper11.ToDto).ToList();
            dto.Transactions = transactions52;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerEntryDto dto, LedgerEntry model);
        partial void ToDtoPreprocessing(LedgerEntry model);
        partial void ModelFactory(LedgerEntryDto dto, ref LedgerEntry model);
        partial void DtoFactory(ref LedgerEntryDto dto, LedgerEntry model);
        partial void ToModelPostprocessing(LedgerEntryDto dto, ref LedgerEntry model);
        partial void ToDtoPostprocessing(ref LedgerEntryDto dto, LedgerEntry model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_LedgerTransactionDto_LedgerTransaction : IDtoMapper<LedgerTransactionDto, LedgerTransaction>
    {

        public virtual LedgerTransaction ToModel(LedgerTransactionDto dto)
        {
            ToModelPreprocessing(dto);
            LedgerTransaction model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                var constructors = typeof(LedgerTransaction).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (LedgerTransaction)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var amount53 = dto.Amount;
            modelType.GetProperty("Amount").SetValue(model, amount53);
            var autoMatchingReference54 = dto.AutoMatchingReference;
            modelType.GetProperty("AutoMatchingReference").SetValue(model, autoMatchingReference54);
            var date55 = dto.Date;
            modelType.GetProperty("Date").SetValue(model, date55);
            var id56 = dto.Id;
            modelType.GetProperty("Id").SetValue(model, id56);
            var narrative57 = dto.Narrative;
            modelType.GetProperty("Narrative").SetValue(model, narrative57);
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual LedgerTransactionDto ToDto(LedgerTransaction model)
        {
            ToDtoPreprocessing(model);
            LedgerTransactionDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new LedgerTransactionDto();
            }
            var amount59 = model.Amount;
            dto.Amount = amount59;
            var autoMatchingReference60 = model.AutoMatchingReference;
            dto.AutoMatchingReference = autoMatchingReference60;
            var date61 = model.Date;
            dto.Date = date61;
            var id62 = model.Id;
            dto.Id = id62;
            var narrative63 = model.Narrative;
            dto.Narrative = narrative63;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(LedgerTransactionDto dto);
        partial void ToDtoPreprocessing(LedgerTransaction model);
        partial void ModelFactory(LedgerTransactionDto dto, ref LedgerTransaction model);
        partial void DtoFactory(ref LedgerTransactionDto dto, LedgerTransaction model);
        partial void ToModelPostprocessing(LedgerTransactionDto dto, ref LedgerTransaction model);
        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, LedgerTransaction model);
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper 3/01/2016 2:19:01 AM UTC")]
    internal partial class Mapper_MobileSettingsDto_MobileSettings : IDtoMapper<MobileStorageSettingsDto, Mobile.MobileStorageSettings>
    {
        public virtual Mobile.MobileStorageSettings ToModel(MobileStorageSettingsDto dto)
        {
            ToModelPreprocessing(dto);
            Mobile.MobileStorageSettings model = null;
            ModelFactory(dto, ref model);
            if (model == null)
            {
                model = new MobileStorageSettings();
            }
            if (dto == null) return model;
            var modelType = model.GetType();
            model.AccessKeyId = dto.AccessKeyId;
            model.AccessKeySecret = dto.AccessKeySecret;
            model.AmazonS3Region = dto.AmazonS3Region;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual MobileStorageSettingsDto ToDto(Mobile.MobileStorageSettings model)
        {
            ToDtoPreprocessing(model);
            MobileStorageSettingsDto dto = null;
            DtoFactory(ref dto, model);
            if (dto == null)
            {
                dto = new MobileStorageSettingsDto();
            }

            dto.AccessKeyId = model.AccessKeyId;
            dto.AccessKeySecret = model.AccessKeySecret;
            dto.AmazonS3Region = model.AmazonS3Region;

            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method

        partial void ToModelPreprocessing(MobileStorageSettingsDto dto);
        partial void ToDtoPreprocessing(Mobile.MobileStorageSettings model);
        partial void ModelFactory(MobileStorageSettingsDto dto, ref Mobile.MobileStorageSettings model);
        partial void DtoFactory(ref MobileStorageSettingsDto dto, Mobile.MobileStorageSettings model);
        partial void ToModelPostprocessing(MobileStorageSettingsDto dto, ref Mobile.MobileStorageSettings model);
        partial void ToDtoPostprocessing(ref MobileStorageSettingsDto dto, Mobile.MobileStorageSettings model);
    } // End Class

} // End Namespace
