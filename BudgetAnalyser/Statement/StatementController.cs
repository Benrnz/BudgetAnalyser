using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Filtering;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.RecentFiles;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class StatementController : ControllerBase, IShowableController, IInitializableController
    {
        // Bug God would be jealous of the length of this class.
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly DemoFileHelper demoFileHelper;
        private readonly IRecentFileManager recentFileManager;
        private readonly IStatementFileManager statementFileManager;
        private readonly UiContext uiContext;
        private BudgetModel budgetModel;
        private string doNotUseBucketFilter;
        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private bool doNotUseShown;
        private StatementModel doNotUseStatement;
        private bool initialised;
        private List<ICommand> recentFileCommands;

        private string waitingForBudgetToLoad;

        public StatementController(
            [NotNull] UiContext uiContext,
            [NotNull] IStatementFileManager statementFileManager,
            [NotNull] IBudgetBucketRepository budgetBucketRepository,
            [NotNull] IRecentFileManager recentFileManager,
            [NotNull] DemoFileHelper demoFileHelper)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (statementFileManager == null)
            {
                throw new ArgumentNullException("statementFileManager");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            if (recentFileManager == null)
            {
                throw new ArgumentNullException("recentFileManager");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            this.uiContext = uiContext;
            this.statementFileManager = statementFileManager;
            this.budgetBucketRepository = budgetBucketRepository;
            this.recentFileCommands = new List<ICommand> { null, null, null, null, null };
            this.recentFileManager = recentFileManager;
            this.demoFileHelper = demoFileHelper;

            MessagingGate.Register<FilterAppliedMessage>(this, OnFilterApplied);
            MessagingGate.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessagingGate.Register<BudgetReadyMessage>(this, OnBudgetReadyMessage);
        }

        public ICommand ApplyRulesCommand
        {
            get { return new RelayCommand(OnApplyRulesCommandExecute, CanExecuteApplyRulesCommand); }
        }

        // TODO Need a find feature to find and highlight transactions based on text search

        public decimal AverageDebit
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    IEnumerable<Transaction> query = Statement.Transactions.Where(t => t.Amount < 0).ToList();
                    if (query.Any())
                    {
                        return query.Average(t => t.Amount);
                    }
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    List<Transaction> query2 =
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code)).ToList();
                    if (query2.Any())
                    {
                        return query2.Average(t => t.Amount);
                    }

                    return 0;
                }

                IEnumerable<Transaction> query3 = Statement.Transactions
                    .Where(
                        t =>
                            t.Amount < 0 && t.BudgetBucket != null &&
                            t.BudgetBucket.Code == BucketFilter)
                    .ToList();
                if (query3.Any())
                {
                    return query3.Average(t => t.Amount);
                }

                return 0;
            }
        }

        public IBackgroundProcessingJobMetadata BackgroundJob
        {
            get { return this.uiContext.BackgroundJob; }
        }

        public string BucketFilter
        {
            get { return this.doNotUseBucketFilter; }

            set
            {
                // TODO Change to a multi-select drop down and allow one or many buckets to be selected.
                this.doNotUseBucketFilter = value;
                RaisePropertyChanged(() => BucketFilter);
                UpdateTotalsRow();
            }
        }

        public IEnumerable<string> BudgetBuckets
        {
            get
            {
                return this.budgetBucketRepository.Buckets
                    .Select(b => b.Code)
                    .Union(new[] { string.Empty }).OrderBy(b => b);
            }
        }

        public ICommand CloseStatementCommand
        {
            get { return new RelayCommand(OnCloseStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public ICommand CreateRuleCommand
        {
            get { return new RelayCommand(OnCreateRuleCommandExecute, CanExecuteCreateRuleCommand); }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return new RelayCommand(OnDeleteTransactionCommandExecute, CanExecuteDeleteTransactionCommand); }
        }

        public ICommand DemoStatementCommand
        {
            get { return new RelayCommand(OnDemoStatementCommandExecuted, CanExecuteOpenStatementCommand); }
        }

        public string DuplicateSummary
        {
            get { return this.doNotUseDuplicateSummary; }

            private set
            {
                this.doNotUseDuplicateSummary = value;
                RaisePropertyChanged(() => DuplicateSummary);
            }
        }

        public IEnumerable<string> FilterBudgetBuckets
        {
            get { return BudgetBuckets.Union(new[] { UncategorisedFilter }).OrderBy(b => b); }
        }

        public bool HasTransactions
        {
            get { return Statement != null && Statement.Transactions.Any(); }
        }

        public DateTime MaxTransactionDate
        {
            get { return Statement.Transactions.Max(t => t.Date); }
        }

        public ICommand MergeStatementCommand
        {
            get { return new RelayCommand(() => OnMergeStatementCommandExecute(), CanExecuteCloseStatementCommand); }
        }

        public DateTime MinTransactionDate
        {
            get { return Statement.Transactions.Min(t => t.Date); }
        }

        public ICommand OpenStatementCommand
        {
            get { return new RelayCommand(() => OnOpenStatementExecute(null), CanExecuteOpenStatementCommand); }
        }

        public ICommand RecentFile1Command
        {
            get
            {
                if (this.recentFileCommands.Count > 0)
                {
                    return this.recentFileCommands[0];
                }

                return null;
            }
        }

        public ICommand RecentFile2Command
        {
            get
            {
                if (this.recentFileCommands.Count > 1)
                {
                    return this.recentFileCommands[1];
                }

                return null;
            }
        }

        public ICommand RecentFile3Command
        {
            get
            {
                if (this.recentFileCommands.Count > 2)
                {
                    return this.recentFileCommands[2];
                }

                return null;
            }
        }

        public ICommand RecentFile4Command
        {
            get
            {
                if (this.recentFileCommands.Count > 3)
                {
                    return this.recentFileCommands[3];
                }

                return null;
            }
        }

        public ICommand RecentFile5Command
        {
            get
            {
                if (this.recentFileCommands.Count > 4)
                {
                    return this.recentFileCommands[4];
                }

                return null;
            }
        }

        public RulesController RulesController
        {
            get { return this.uiContext.RulesController; }
        }

        public ICommand SaveStatementCommand
        {
            get { return new RelayCommand(OnSaveStatementExecute, CanExecuteCloseStatementCommand); }
        }

        public Transaction SelectedRow { get; set; }

        public ICommand ShowRulesCommand
        {
            get { return new RelayCommand(OnShowRulesCommandExecute); }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public StatementModel Statement
        {
            get { return this.doNotUseStatement; }

            private set
            {
                this.doNotUseStatement = value;
                RaisePropertyChanged(() => Statement);
            }
        }

        public string StatementName
        {
            get
            {
                if (Statement != null)
                {
                    return string.Format(
                        "{0} {1}",
                        Path.GetFileNameWithoutExtension(Statement.FileName),
                        Dirty ? "*" : string.Empty);
                }

                return "[No Transactions Loaded]";
            }
        }

        public decimal TotalCount
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Count();
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Count(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code));
                }

                return Statement.Transactions.Count(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter);
            }
        }

        public decimal TotalCredits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount > 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDebits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount < 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDifference
        {
            get { return TotalCredits + TotalDebits; }
        }

        private bool Dirty
        {
            get { return this.doNotUseDirty; }

            set
            {
                this.doNotUseDirty = value;
                RaisePropertyChanged(() => StatementName);
            }
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;
            UpdateRecentFiles(this.recentFileManager.Files());
        }

        public void NotifyOfClosing()
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }
        }

        public void NotifyOfEdit()
        {
            Dirty = true;
            Messenger.Send(new StatementHasBeenModifiedMessage { Dirty = Dirty });
        }

        public void NotifyOfReset()
        {
            Dirty = false;
            Messenger.Send(new StatementHasBeenModifiedMessage { Dirty = false });
        }

        private bool CanExecuteApplyRulesCommand()
        {
            return RulesController.Rules.Any();
        }

        private bool CanExecuteCloseStatementCommand()
        {
            return BackgroundJob.MenuAvailable && Statement != null;
        }

        private bool CanExecuteCreateRuleCommand()
        {
            return SelectedRow != null;
        }

        private bool CanExecuteOpenStatementCommand()
        {
            return BackgroundJob.MenuAvailable;
        }

        private bool CanExecuteDeleteTransactionCommand()
        {
            return SelectedRow != null;
        }

        private bool Load(string fullFileName)
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }

            try
            {
                BackgroundJob.StartNew("Loading statement...", false);
                return LoadInternal(fullFileName);
            }
            finally
            {
                BackgroundJob.Finish();
            }
        }

        private bool LoadInternal(string fullFileName)
        {
            StatementModel statementModel = this.statementFileManager.LoadAnyStatementFile(fullFileName);

            using (this.uiContext.WaitCursorFactory())
            {
                if (statementModel == null)
                {
                    // User cancelled.
                    return false;
                }

                Statement = statementModel;
                var requestCurrentFilterMessage = new RequestFilterMessage(this);
                Messenger.Send(requestCurrentFilterMessage);
                if (requestCurrentFilterMessage.Criteria != null)
                {
                    Statement.Filter(requestCurrentFilterMessage.Criteria);
                }

                NotifyOfReset();
                UpdateTotalsRow();
            }

            MessagingGate.Send(new StatementReadyMessage(Statement));
            return true;
        }

        private void LoadStatementFromApplicationState(string statementFileName)
        {
            try
            {
                BackgroundJob.StartNew("Loading previous accounts...", false);
                if (string.IsNullOrWhiteSpace(statementFileName))
                {
                    return;
                }

                if (this.budgetModel == null)
                {
                    // Budget isn't yet loaded. Wait for the next BudgetClosedMessage to signal budget is ready.
                    this.waitingForBudgetToLoad = statementFileName;
                    return;
                }

                LoadInternal(statementFileName);
            }
            catch (FileNotFoundException)
            {
                // Ignore it.
            }
            finally
            {
                BackgroundJob.Finish();
            }
        }

        private void Merge()
        {
            Save();
            BucketFilter = null;

            try
            {
                BackgroundJob.StartNew("Merging statement...", false);
                StatementModel additionalModel = this.statementFileManager.ImportAndMergeBankStatement(Statement);
                using (this.uiContext.WaitCursorFactory())
                {
                    if (additionalModel == null)
                    {
                        // User cancelled.
                        return;
                    }

                    Statement.Merge(additionalModel);
                }

                RaisePropertyChanged(() => Statement);
                Messenger.Send(new TransactionsChangedMessage());
                NotifyOfEdit();
                UpdateTotalsRow();
            }
            finally
            {
                MessagingGate.Send(new StatementReadyMessage(Statement));
                BackgroundJob.Finish();
            }
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(LastStatementLoadedV1)))
            {
                return;
            }

            var statementFileName = message.RehydratedModels[typeof(LastStatementLoadedV1)].AdaptModel<string>();
            LoadStatementFromApplicationState(statementFileName);
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastStatement = new LastStatementLoadedV1
            {
                Model = Statement == null ? null : Statement.FileName,
            };
            message.PersistThisModel(lastStatement);
        }

        private void OnApplyRulesCommandExecute()
        {
            bool matchesOccured = false;
            foreach (Transaction transaction in Statement.Transactions)
            {
                if (transaction.BudgetBucket == null || transaction.BudgetBucket.Code == null)
                {
                    foreach (MatchingRule rule in RulesController.Rules)
                    {
                        if (rule.Match(transaction))
                        {
                            transaction.BudgetBucket = rule.Bucket;
                            matchesOccured = true;
                        }
                    }
                }
            }

            if (matchesOccured)
            {
                RulesController.SaveRules();
            }
        }

        private void OnBudgetReadyMessage(BudgetReadyMessage message)
        {
            if (!message.ActiveBudget.BudgetActive)
            {
                // Not the current budget for today so ignore.
                return;
            }

            BudgetModel oldBudget = this.budgetModel;
            this.budgetModel = message.ActiveBudget.Model;

            if (this.waitingForBudgetToLoad != null)
            {
                // We've been waiting for the budget to load so we can load previous statement.
                LoadStatementFromApplicationState(this.waitingForBudgetToLoad);
                this.waitingForBudgetToLoad = null;
                return;
            }

            if (oldBudget != null
                && (oldBudget.Expenses.Any() || oldBudget.Incomes.Any())
                && oldBudget.Id != this.budgetModel.Id
                && Statement != null
                && Statement.AllTransactions.Any())
            {
                this.uiContext.UserPrompts.MessageBox.Show(
                    "WARNING! By loading a different budget with a Statement loaded data loss may occur. There may be budget categories used in the Statement that do not exist in the loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                    "Data Loss Wanring!");
            }
        }

        private void OnCloseStatementExecute()
        {
            if (PromptToSaveIfDirty())
            {
                Save();
            }

            Statement = null;
            NotifyOfReset();
            UpdateTotalsRow();
        }

        private void OnCreateRuleCommandExecute()
        {
            if (SelectedRow == null)
            {
                this.uiContext.UserPrompts.MessageBox.Show("No row selected.");
                return;
            }

            RulesController.CreateNewRuleFromTransaction(SelectedRow);
        }

        private void OnDeleteTransactionCommandExecute()
        {
            if (SelectedRow == null)
            {
                return;
            }

            bool? confirm = this.uiContext.UserPrompts.YesNoBox.Show(
                "Are you sure you want to delete this transaction?", "Delete Transaction");
            if (confirm != null && confirm.Value)
            {
                Statement.RemoveTransaction(SelectedRow);
                UpdateTotalsRow();
                NotifyOfEdit();
            }
        }

        private void OnDemoStatementCommandExecuted()
        {
            OnOpenStatementExecute(this.demoFileHelper.FindDemoFile("DemoTransactions.csv"));
        }

        private void OnFilterApplied(FilterAppliedMessage message)
        {
            if (message.Sender == this || message.Criteria == null)
            {
                return;
            }

            if (Statement == null)
            {
                return;
            }

            Statement.Filter(message.Criteria);
            UpdateTotalsRow();
        }

        private void OnMergeStatementCommandExecute()
        {
            Merge();
        }

        private void OnOpenStatementExecute(string fullFileName)
        {
            try
            {
                if (!Load(fullFileName))
                {
                    return;
                }

                UpdateRecentFiles(this.recentFileManager.AddFile(Statement.FileName));
            }
            catch (FileNotFoundException ex)
            {
                // When merging this exception will never be thrown.
                if (!string.IsNullOrWhiteSpace(ex.FileName))
                {
                    UpdateRecentFiles(this.recentFileManager.Remove(ex.FileName));
                }
            }
        }

        private void OnSaveStatementExecute()
        {
            // Not async at this stage, because saving of data while user edits are taking place will result in inconsistent results.
            using (this.uiContext.WaitCursorFactory())
            {
                Save();
                UpdateRecentFiles(this.recentFileManager.UpdateFile(Statement.FileName));
            }
        }

        private void OnShowRulesCommandExecute()
        {
            RulesController.Show();
        }

        private bool PromptToSaveIfDirty()
        {
            if (Statement != null && Dirty)
            {
                bool? result = this.uiContext.UserPrompts.YesNoBox.Show("Statement has been modified, save changes?",
                    "Budget Analyser");
                if (result != null && result.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void Save()
        {
            this.statementFileManager.Save(Statement);
            UpdateTotalsRow();
            NotifyOfReset();
        }

        private void UpdateRecentFiles(IEnumerable<KeyValuePair<string, string>> files)
        {
            this.recentFileCommands =
                files.Select(f => (ICommand)new RecentFileRelayCommand(f.Value, f.Key, file => OnOpenStatementExecute(file), x => BackgroundJob.MenuAvailable))
                    .ToList();
            RaisePropertyChanged(() => RecentFile1Command);
            RaisePropertyChanged(() => RecentFile2Command);
            RaisePropertyChanged(() => RecentFile3Command);
            RaisePropertyChanged(() => RecentFile4Command);
            RaisePropertyChanged(() => RecentFile5Command);
        }

        private void UpdateTotalsRow()
        {
            RaisePropertyChanged(() => TotalCredits);
            RaisePropertyChanged(() => TotalDebits);
            RaisePropertyChanged(() => TotalDifference);
            RaisePropertyChanged(() => AverageDebit);
            RaisePropertyChanged(() => TotalCount);
            RaisePropertyChanged(() => HasTransactions);
            RaisePropertyChanged(() => StatementName);

            if (Statement == null)
            {
                DuplicateSummary = null;
            }
            else
            {
                List<IGrouping<int, Transaction>> duplicates = Statement.ValidateAgainstDuplicates().ToList();
                DuplicateSummary = duplicates.Any()
                    ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!",
                        duplicates.Sum(group => group.Count()))
                    : null;
            }
        }
    }
}