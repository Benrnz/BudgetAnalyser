using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class NewRuleController : ControllerBase, IInitializableController, IShellDialogInteractivity, IShellDialogToolTips
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IMatchingRuleFactory ruleFactory;
        private decimal doNotUseAmount;
        private string doNotUseDescription;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;
        private string doNotUseTransactionType;
        private bool doNotUseUseAmount;
        private bool doNotUseUseReference1;
        private bool doNotUseUseReference2;
        private bool doNotUseUseReference3;
        private bool doNotUseUseTransactionType;
        private Guid shellDialogCorrelationId;

        public event EventHandler RuleCreated;

        public NewRuleController([NotNull] UiContext uiContext, [NotNull] IBudgetBucketRepository budgetBucketRepository, [NotNull] IMatchingRuleFactory ruleFactory)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            if (ruleFactory == null)
            {
                throw new ArgumentNullException("ruleFactory");
            }

            this.budgetBucketRepository = budgetBucketRepository;
            this.ruleFactory = ruleFactory;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public string ActionButtonToolTip
        {
            get { return "Save the new rule."; }
        }

        public decimal Amount
        {
            get { return this.doNotUseAmount; }

            set
            {
                this.doNotUseAmount = value;
                RaisePropertyChanged(() => Amount);
                UpdateSimilarRules();
            }
        }

        public BudgetBucket Bucket { get; set; }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return false; }
        }

        public bool CanExecuteSaveButton
        {
            get { return UseAmount || UseDescription || UseReference1 || UseReference2 || UseReference3 || UseTransactionType; }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public string Description
        {
            get { return this.doNotUseDescription; }

            set
            {
                this.doNotUseDescription = value;
                RaisePropertyChanged(() => Description);
                UpdateSimilarRules();
            }
        }

        public MatchingRule NewRule { get; set; }

        public string Reference1
        {
            get { return this.doNotUseReference1; }

            set
            {
                this.doNotUseReference1 = value;
                RaisePropertyChanged(() => Reference1);
                UpdateSimilarRules();
            }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }

            set
            {
                this.doNotUseReference2 = value;
                RaisePropertyChanged(() => Reference2);
                UpdateSimilarRules();
            }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }

            set
            {
                this.doNotUseReference3 = value;
                RaisePropertyChanged(() => Reference3);
                UpdateSimilarRules();
            }
        }

        public IEnumerable<object> SimilarRules { get; private set; }

        public bool SimilarRulesExist { get; private set; }

        public string Title
        {
            get { return "New Matching Rule for: " + Bucket; }
        }

        public string TransactionType
        {
            get { return this.doNotUseTransactionType; }

            set
            {
                this.doNotUseTransactionType = value;
                RaisePropertyChanged(() => TransactionType);
                UpdateSimilarRules();
            }
        }

        public bool UseAmount
        {
            get { return this.doNotUseUseAmount; }

            set
            {
                this.doNotUseUseAmount = value;
                RaisePropertyChanged(() => UseAmount);
            }
        }

        public bool UseDescription { get; set; }

        public bool UseReference1
        {
            get { return this.doNotUseUseReference1; }

            set
            {
                this.doNotUseUseReference1 = value;
                RaisePropertyChanged(() => UseReference1);
            }
        }

        public bool UseReference2
        {
            get { return this.doNotUseUseReference2; }

            set
            {
                this.doNotUseUseReference2 = value;
                RaisePropertyChanged(() => UseReference2);
            }
        }

        public bool UseReference3
        {
            get { return this.doNotUseUseReference3; }

            set
            {
                this.doNotUseUseReference3 = value;
                RaisePropertyChanged(() => UseReference3);
            }
        }

        public bool UseTransactionType
        {
            get { return this.doNotUseUseTransactionType; }

            set
            {
                this.doNotUseUseTransactionType = value;
                RaisePropertyChanged(() => UseTransactionType);
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

            NewRule = null;
        }

        public void ShowDialog(IEnumerable<MatchingRule> allRules)
        {
            SimilarRules = new List<object>(
                allRules.Select(rule => new
                {
                    rule.Amount,
                    rule.Description,
                    rule.Reference1,
                    rule.Reference2,
                    rule.Reference3,
                    this.budgetBucketRepository.Buckets.Single(b => b.Code == rule.Bucket.Code).Code,
                    rule.TransactionType
                }));

            UpdateSimilarRules();

            this.shellDialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = this.shellDialogCorrelationId,
                Title = Title,
            };
            MessengerInstance.Send(dialogRequest);
        }

        private static bool IsEqualButNotBlank(string operand1, string operand2)
        {
            if (string.IsNullOrWhiteSpace(operand1) || string.IsNullOrWhiteSpace(operand2))
            {
                return false;
            }

            return operand1 == operand2;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.shellDialogCorrelationId))
            {
                return;
            }

            var newRule = this.ruleFactory.CreateRule(Bucket.Code); //new MatchingRule(this.budgetBucketRepository) { Bucket = Bucket };

            if (Bucket == null)
            {
                MessageBox.Show("Bucket cannot be null.");
                return;
            }

            if (UseDescription)
            {
                newRule.Description = Description;
            }

            if (UseReference1)
            {
                newRule.Reference1 = Reference1;
            }

            if (UseReference2)
            {
                newRule.Reference2 = Reference2;
            }

            if (UseReference3)
            {
                newRule.Reference3 = Reference3;
            }

            if (UseTransactionType)
            {
                newRule.TransactionType = TransactionType;
            }

            NewRule = newRule;
            var handler = RuleCreated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed, acceptable here.")]
        private void UpdateSimilarRules()
        {
            if (SimilarRules == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(SimilarRules);
            view.Filter = item =>
            {
                dynamic currentRule = item;
                return Amount == currentRule.Amount
                       || IsEqualButNotBlank(Description, currentRule.Description)
                       || IsEqualButNotBlank(Reference1, currentRule.Reference1)
                       || IsEqualButNotBlank(Reference2, currentRule.Reference2)
                       || IsEqualButNotBlank(Reference3, currentRule.Reference3)
                       || IsEqualButNotBlank(TransactionType, currentRule.TransactionType);
            };

            SimilarRulesExist = !view.IsEmpty;
            RaisePropertyChanged(() => SimilarRulesExist);
            RaisePropertyChanged(() => SimilarRules);
        }
    }
}