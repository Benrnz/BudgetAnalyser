using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class NewRuleController : ControllerBase, IInitializableController, IShellDialogInteractivity, IShellDialogToolTips
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ILogger logger;
        private readonly IUserMessageBox messageBoxService;
        private readonly ITransactionRuleService rulesService;
        private bool doNotUseAndChecked;
        private bool doNotUseOrChecked;
        private Guid shellDialogCorrelationId;

        public NewRuleController(
            [NotNull] UiContext uiContext,
            [NotNull] ITransactionRuleService rulesService,
            [NotNull] IBudgetBucketRepository bucketRepo)
            : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (rulesService == null)
            {
                throw new ArgumentNullException(nameof(rulesService));
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            this.rulesService = rulesService;
            this.bucketRepo = bucketRepo;
            this.messageBoxService = uiContext.UserPrompts.MessageBox;
            this.logger = uiContext.Logger;

            Messenger.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler RuleCreated;
        public string ActionButtonToolTip => "Save the new rule.";

        public DecimalCriteria Amount { get; set; }

        public bool AndChecked
        {
            get { return this.doNotUseAndChecked; }
            set
            {
                this.doNotUseAndChecked = value;
                OnPropertyChanged();
                this.doNotUseOrChecked = !AndChecked;
                OnPropertyChanged(nameof(OrChecked));
            }
        }

        public BudgetBucket Bucket { get; set; }
        public bool CanExecuteCancelButton => true;
        public bool CanExecuteOkButton => false;
        public bool CanExecuteSaveButton => Amount.Applicable || Description.Applicable || Reference1.Applicable || Reference2.Applicable || Reference3.Applicable || TransactionType.Applicable;
        public string CloseButtonToolTip => "Cancel";

        public StringCriteria Description { get; set; }

        public MatchingRule NewRule { get; set; }

        public bool OrChecked
        {
            get { return this.doNotUseOrChecked; }
            [UsedImplicitly]
            set
            {
                this.doNotUseOrChecked = value;
                OnPropertyChanged();
                this.doNotUseAndChecked = !OrChecked;
                OnPropertyChanged(nameof(AndChecked));
            }
        }

        public StringCriteria Reference1 { get; set; }

        public StringCriteria Reference2 { get; set; }

        public StringCriteria Reference3 { get; set; }

        public IEnumerable<SimilarMatchedRule> SimilarRules { get; private set; }
        public bool SimilarRulesExist { get; private set; }
        public string Title => "New Matching Rule for: " + Bucket;

        public StringCriteria TransactionType { get; set; }

        public void Initialize()
        {
            SimilarRules = null;
            AndChecked = true;
            NewRule = null;
            if (Description != null)
            {
                Description.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }
            if (Reference1 != null)
            {
                Reference1.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }
            if (Reference2 != null)
            {
                Reference2.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }
            if (Reference3 != null)
            {
                Reference3.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }
            if (Amount != null)
            {
                Amount.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }
            if (TransactionType != null)
            {
                TransactionType.PropertyChanged -= OnCriteriaValuePropertyChanged;
            }

            Description = new StringCriteria { Applicable = true };
            Description.PropertyChanged += OnCriteriaValuePropertyChanged;
            Reference1 = new StringCriteria();
            Reference1.PropertyChanged += OnCriteriaValuePropertyChanged;
            Reference2 = new StringCriteria();
            Reference2.PropertyChanged += OnCriteriaValuePropertyChanged;
            Reference3 = new StringCriteria();
            Reference3.PropertyChanged += OnCriteriaValuePropertyChanged;
            Amount = new DecimalCriteria();
            Amount.PropertyChanged += OnCriteriaValuePropertyChanged;
            TransactionType = new StringCriteria();
            TransactionType.PropertyChanged += OnCriteriaValuePropertyChanged;
        }

        public void ShowDialog(IEnumerable<MatchingRule> allRules)
        {
            SimilarRules = allRules.Select(r => new SimilarMatchedRule(this.bucketRepo, r)).ToList();
            UpdateSimilarRules();

            this.shellDialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = this.shellDialogCorrelationId,
                Title = Title
            };
            Messenger.Send(dialogRequest);
        }

        private void OnCriteriaValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshSimilarRules();
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.shellDialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                return;
            }

            if (Bucket == null)
            {
                this.messageBoxService.Show("Bucket cannot be null.");
                return;
            }

            NewRule = this.rulesService.CreateNewRule(
                Bucket.Code,
                Description.Applicable ? Description.Value : null,
                new[]
                {
                    Reference1.Applicable ? Reference1.Value : null,
                    Reference2.Applicable ? Reference2.Value : null,
                    Reference3.Applicable ? Reference3.Value : null
                },
                TransactionType.Applicable ? TransactionType.Value : null,
                Amount.Applicable ? Amount.Value : null,
                AndChecked);

            EventHandler handler = RuleCreated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshSimilarRules()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(SimilarRules);
            view?.Refresh();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed, acceptable here.")]
        private void UpdateSimilarRules()
        {
            if (SimilarRules == null)
            {
                return;
            }

            this.logger.LogInfo(l => l.Format("UpdateSimilarRules1: Rules.Count() = {0}", SimilarRules.Count()));
            ICollectionView view = CollectionViewSource.GetDefaultView(SimilarRules);
            view.Filter = item => this.rulesService.IsRuleSimilar((SimilarMatchedRule)item, Amount, Description, new[] { Reference1, Reference2, Reference3 }, TransactionType);
            view.SortDescriptions.Add(new SortDescription(nameof(SimilarMatchedRule.SortOrder), ListSortDirection.Descending));

            SimilarRulesExist = !view.IsEmpty;
            OnPropertyChanged(nameof(SimilarRulesExist));
            OnPropertyChanged(nameof(SimilarRules));
            this.logger.LogInfo(l => l.Format("UpdateSimilarRules2: Rules.Count() = {0}", SimilarRules.Count()));
        }
    }
}