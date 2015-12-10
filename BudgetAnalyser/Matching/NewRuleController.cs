using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
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
        private decimal doNotUseAmount;
        private bool doNotUseAndChecked;
        private string doNotUseDescription;
        private bool doNotUseOrChecked;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;
        private string doNotUseTransactionType;
        private bool doNotUseUseAmount;
        private bool doNotUseUseDescription;
        private bool doNotUseUseReference1;
        private bool doNotUseUseReference2;
        private bool doNotUseUseReference3;
        private bool doNotUseUseTransactionType;
        private Guid shellDialogCorrelationId;

        public NewRuleController(
            [NotNull] UiContext uiContext,
            [NotNull] ITransactionRuleService rulesService,
            [NotNull] IBudgetBucketRepository bucketRepo)
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

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler RuleCreated;
        public string ActionButtonToolTip => "Save the new rule.";

        public decimal Amount
        {
            get { return this.doNotUseAmount; }

            set
            {
                this.doNotUseAmount = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public bool AndChecked
        {
            get { return this.doNotUseAndChecked; }
            set
            {
                this.doNotUseAndChecked = value;
                RaisePropertyChanged();
                this.doNotUseOrChecked = !AndChecked;
                RaisePropertyChanged(() => OrChecked);
            }
        }

        public BudgetBucket Bucket { get; set; }
        public bool CanExecuteCancelButton => true;
        public bool CanExecuteOkButton => false;
        public bool CanExecuteSaveButton => UseAmount || UseDescription || UseReference1 || UseReference2 || UseReference3 || UseTransactionType;
        public string CloseButtonToolTip => "Cancel";

        public string Description
        {
            get { return this.doNotUseDescription; }

            set
            {
                this.doNotUseDescription = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public MatchingRule NewRule { get; set; }

        public bool OrChecked
        {
            get { return this.doNotUseOrChecked; }
            [UsedImplicitly]
            set
            {
                this.doNotUseOrChecked = value;
                RaisePropertyChanged();
                this.doNotUseAndChecked = !OrChecked;
                RaisePropertyChanged(() => AndChecked);
            }
        }

        public string Reference1
        {
            get { return this.doNotUseReference1; }

            set
            {
                this.doNotUseReference1 = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }

            set
            {
                this.doNotUseReference2 = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }

            set
            {
                this.doNotUseReference3 = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public IEnumerable<SimilarMatchedRule> SimilarRules { get; private set; }
        public bool SimilarRulesExist { get; private set; }
        public string Title => "New Matching Rule for: " + Bucket;

        public string TransactionType
        {
            get { return this.doNotUseTransactionType; }

            set
            {
                this.doNotUseTransactionType = value;
                RaisePropertyChanged();
                UpdateSimilarRules();
            }
        }

        public bool UseAmount
        {
            get { return this.doNotUseUseAmount; }

            [UsedImplicitly]
            set
            {
                this.doNotUseUseAmount = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public bool UseDescription
        {
            get { return this.doNotUseUseDescription; }
            set
            {
                this.doNotUseUseDescription = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public bool UseReference1
        {
            get { return this.doNotUseUseReference1; }

            set
            {
                this.doNotUseUseReference1 = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public bool UseReference2
        {
            get { return this.doNotUseUseReference2; }

            set
            {
                this.doNotUseUseReference2 = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public bool UseReference3
        {
            get { return this.doNotUseUseReference3; }

            set
            {
                this.doNotUseUseReference3 = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public bool UseTransactionType
        {
            get { return this.doNotUseUseTransactionType; }

            [UsedImplicitly]
            set
            {
                this.doNotUseUseTransactionType = value;
                RaisePropertyChanged();
                RefreshSimilarRules();
            }
        }

        public void Initialize()
        {
            UseDescription = true;
            UseReference1 = false;
            UseReference2 = false;
            UseReference3 = false;
            Description = null;
            Reference1 = null;
            Reference2 = null;
            Reference3 = null;
            SimilarRules = null;
            AndChecked = true;

            NewRule = null;
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
            MessengerInstance.Send(dialogRequest);
        }

        private bool FilterSimilarRules(object item)
        {
            var similarRule = (SimilarMatchedRule)item;

            bool[] matchedBy;
            if (this.rulesService.IsRuleSimilar(similarRule, Amount, Description, new[] { Reference1, Reference2, Reference3 }, TransactionType, out matchedBy))
            {
                similarRule.AmountMatched = matchedBy[0];
                similarRule.DescriptionMatched = matchedBy[1];
                similarRule.Reference1Matched = matchedBy[2];
                similarRule.Reference2Matched = matchedBy[3];
                similarRule.Reference3Matched = matchedBy[4];
                similarRule.TransactionTypeMatched = matchedBy[5];

                return (UseAmount && similarRule.AmountMatched)
                       || (UseDescription && similarRule.DescriptionMatched)
                       || (UseReference1 && similarRule.Reference1Matched)
                       || (UseReference2 && similarRule.Reference2Matched)
                       || (UseReference3 && similarRule.Reference3Matched)
                       || (UseTransactionType && similarRule.TransactionTypeMatched);
            }

            return false;
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
                UseDescription ? Description : null,
                new[]
                {
                    UseReference1 ? Reference1 : null,
                    UseReference2 ? Reference2 : null,
                    UseReference3 ? Reference3 : null
                },
                UseTransactionType ? TransactionType : null,
                UseAmount ? (decimal?)Amount : null,
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
            view.Filter = FilterSimilarRules;
            view.SortDescriptions.Add(new SortDescription(nameof(SimilarMatchedRule.SortOrder), ListSortDirection.Descending));

            SimilarRulesExist = !view.IsEmpty;
            RaisePropertyChanged(() => SimilarRulesExist);
            RaisePropertyChanged(() => SimilarRules);
            this.logger.LogInfo(l => l.Format("UpdateSimilarRules2: Rules.Count() = {0}", SimilarRules.Count()));
        }
    }
}