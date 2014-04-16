using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    public class NewRuleController : ControllerBase, IInitializableController
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IViewLoader viewLoader;
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

        public NewRuleController([NotNull] CreateNewRuleViewLoader viewLoader, [NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.viewLoader = viewLoader;
            this.budgetBucketRepository = budgetBucketRepository;
        }

        public decimal Amount
        {
            get { return this.doNotUseAmount; }

            set
            {
                this.doNotUseAmount = value;
                RaisePropertyChanged(() => this.Amount);
                UpdateSimilarRules();
            }
        }

        public BudgetBucket Bucket { get; set; }

        public ICommand CancelCommand
        {
            get { return new RelayCommand(OnCancelCommandExecute); }
        }

        public string Description
        {
            get { return this.doNotUseDescription; }

            set
            {
                this.doNotUseDescription = value;
                RaisePropertyChanged(() => this.Description);
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
                RaisePropertyChanged(() => this.Reference1);
                UpdateSimilarRules();
            }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }

            set
            {
                this.doNotUseReference2 = value;
                RaisePropertyChanged(() => this.Reference2);
                UpdateSimilarRules();
            }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }

            set
            {
                this.doNotUseReference3 = value;
                RaisePropertyChanged(() => this.Reference3);
                UpdateSimilarRules();
            }
        }

        public ICommand SaveCommand
        {
            get { return new RelayCommand(OnSaveCommandExecute); }
        }

        public List<object> SimilarRules { get; private set; }

        public bool SimilarRulesExist { get; private set; }

        public string Title
        {
            get { return "New Matching Rule for: " + this.Bucket.Description; }
        }

        public string TransactionType
        {
            get { return this.doNotUseTransactionType; }

            set
            {
                this.doNotUseTransactionType = value;
                RaisePropertyChanged(() => this.TransactionType);
                UpdateSimilarRules();
            }
        }

        public bool UseAmount
        {
            get { return this.doNotUseUseAmount; }

            set
            {
                this.doNotUseUseAmount = value;
                RaisePropertyChanged(() => this.UseAmount);
            }
        }

        public bool UseDescription { get; set; }

        public bool UseReference1
        {
            get { return this.doNotUseUseReference1; }

            set
            {
                this.doNotUseUseReference1 = value;
                RaisePropertyChanged(() => this.UseReference1);
            }
        }

        public bool UseReference2
        {
            get { return this.doNotUseUseReference2; }

            set
            {
                this.doNotUseUseReference2 = value;
                RaisePropertyChanged(() => this.UseReference2);
            }
        }

        public bool UseReference3
        {
            get { return this.doNotUseUseReference3; }

            set
            {
                this.doNotUseUseReference3 = value;
                RaisePropertyChanged(() => this.UseReference3);
            }
        }

        public bool UseTransactionType
        {
            get { return this.doNotUseUseTransactionType; }

            set
            {
                this.doNotUseUseTransactionType = value;
                RaisePropertyChanged(() => this.UseTransactionType);
            }
        }

        public void Initialize()
        {
            this.UseDescription = true;
            this.UseReference1 = false;
            this.UseReference2 = false;
            this.UseReference3 = false;
            this.Description = null;
            this.Reference1 = null;
            this.Reference2 = null;
            this.Reference3 = null;
            this.SimilarRules = null;

            this.NewRule = null;
        }

        public void ShowDialog(IEnumerable<MatchingRule> allRules)
        {
            this.SimilarRules = new List<object>(
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
            this.viewLoader.ShowDialog(this);
        }

        private bool IsEqualButNotBlank(string operand1, string operand2)
        {
            if (string.IsNullOrWhiteSpace(operand1) || string.IsNullOrWhiteSpace(operand2))
            {
                return false;
            }

            return operand1 == operand2;
        }

        private void OnCancelCommandExecute()
        {
            this.NewRule = null;
            this.viewLoader.Close();
        }

        private void OnSaveCommandExecute()
        {
            this.NewRule = new MatchingRule(this.budgetBucketRepository) { Bucket = this.Bucket };

            if (this.Bucket == null)
            {
                MessageBox.Show("Bucket cannot be null.");
                return;
            }

            if (this.UseDescription)
            {
                this.NewRule.Description = this.Description;
            }

            if (this.UseReference1)
            {
                this.NewRule.Reference1 = this.Reference1;
            }

            if (this.UseReference2)
            {
                this.NewRule.Reference2 = this.Reference2;
            }

            if (this.UseReference3)
            {
                this.NewRule.Reference3 = this.Reference3;
            }

            if (this.UseTransactionType)
            {
                this.NewRule.TransactionType = this.TransactionType;
            }

            this.viewLoader.Close();
        }

        private void UpdateSimilarRules()
        {
            if (this.SimilarRules == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(this.SimilarRules);
            view.Filter = item =>
            {
                dynamic currentRule = item;
                return this.Amount == currentRule.Amount
                       || IsEqualButNotBlank(this.Description, currentRule.Description)
                       || IsEqualButNotBlank(this.Reference1, currentRule.Reference1)
                       || IsEqualButNotBlank(this.Reference2, currentRule.Reference2)
                       || IsEqualButNotBlank(this.Reference3, currentRule.Reference3)
                       || IsEqualButNotBlank(this.TransactionType, currentRule.TransactionType);
            };

            this.SimilarRulesExist = !view.IsEmpty;
            RaisePropertyChanged(() => this.SimilarRulesExist);
            RaisePropertyChanged(() => this.SimilarRules);
        }
    }
}