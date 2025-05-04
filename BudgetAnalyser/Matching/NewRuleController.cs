using System.ComponentModel;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
public class NewRuleController : ControllerBase, IInitializableController, IShellDialogInteractivity, IShellDialogToolTips
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly ILogger logger;
    private readonly IUserMessageBox messageBoxService;
    private readonly ITransactionRuleService rulesService;
    private DecimalCriteria doNotUseAmount = new();
    private bool doNotUseAndChecked;
    private StringCriteria doNotUseDescription = new();
    private bool doNotUseOrChecked;
    private StringCriteria doNotUseReference1 = new();
    private StringCriteria doNotUseReference2 = new();
    private StringCriteria doNotUseReference3 = new();
    private StringCriteria doNotUseTransactionType = new();
    private Guid shellDialogCorrelationId;

    public NewRuleController(IUiContext uiContext, ITransactionRuleService rulesService, IBudgetBucketRepository bucketRepo) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
        this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
        this.messageBoxService = uiContext.UserPrompts.MessageBox;
        this.logger = uiContext.Logger;

        Messenger.Register<NewRuleController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public event EventHandler? RuleCreated;

    public DecimalCriteria Amount
    {
        get => this.doNotUseAmount;
        set
        {
            if (Equals(value, this.doNotUseAmount))
            {
                return;
            }

            this.doNotUseAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public bool AndChecked
    {
        get => this.doNotUseAndChecked;
        set
        {
            if (value == this.doNotUseAndChecked)
            {
                return;
            }

            this.doNotUseAndChecked = value;
            OnPropertyChanged();
            this.doNotUseOrChecked = !AndChecked;
            OnPropertyChanged(nameof(OrChecked));
        }
    }

    public BudgetBucket? Bucket { get; set; }

    public StringCriteria Description
    {
        get => this.doNotUseDescription;
        set
        {
            if (Equals(value, this.doNotUseDescription))
            {
                return;
            }

            this.doNotUseDescription = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public MatchingRule? NewRule { get; set; }

    public bool OrChecked
    {
        get => this.doNotUseOrChecked;
        [UsedImplicitly]
        set
        {
            if (value == this.doNotUseOrChecked)
            {
                return;
            }

            this.doNotUseOrChecked = value;
            OnPropertyChanged();
            this.doNotUseAndChecked = !OrChecked;
            OnPropertyChanged(nameof(AndChecked));
        }
    }

    public StringCriteria Reference1
    {
        get => this.doNotUseReference1;
        set
        {
            if (Equals(value, this.doNotUseReference1))
            {
                return;
            }

            this.doNotUseReference1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public StringCriteria Reference2
    {
        get => this.doNotUseReference2;
        set
        {
            if (Equals(value, this.doNotUseReference2))
            {
                return;
            }

            this.doNotUseReference2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public StringCriteria Reference3
    {
        get => this.doNotUseReference3;
        set
        {
            if (Equals(value, this.doNotUseReference3))
            {
                return;
            }

            this.doNotUseReference3 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public IEnumerable<SimilarMatchedRule>? SimilarRules { get; private set; }

    public bool SimilarRulesExist { get; private set; }

    public string Title => "New Matching Rule for: " + Bucket;

    public StringCriteria TransactionType
    {
        get => this.doNotUseTransactionType;
        set
        {
            if (Equals(value, this.doNotUseTransactionType))
            {
                return;
            }

            this.doNotUseTransactionType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExecuteSaveButton));
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public void Initialize()
    {
        SimilarRules = null;
        AndChecked = true;
        NewRule = null;
        Description.PropertyChanged -= OnCriteriaValuePropertyChanged;
        Reference1.PropertyChanged -= OnCriteriaValuePropertyChanged;
        Reference2.PropertyChanged -= OnCriteriaValuePropertyChanged;
        Reference3.PropertyChanged -= OnCriteriaValuePropertyChanged;
        Amount.PropertyChanged -= OnCriteriaValuePropertyChanged;
        TransactionType.PropertyChanged -= OnCriteriaValuePropertyChanged;

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

    public bool CanExecuteCancelButton => true;
    public bool CanExecuteOkButton => false;
    public bool CanExecuteSaveButton => Amount.Applicable || Description.Applicable || Reference1.Applicable || Reference2.Applicable || Reference3.Applicable || TransactionType.Applicable;
    public string ActionButtonToolTip => "Save the new rule.";
    public string CloseButtonToolTip => "Cancel";

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

    private void OnCriteriaValuePropertyChanged(object? sender, PropertyChangedEventArgs e)
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

        if (Bucket is null)
        {
            this.messageBoxService.Show("Bucket cannot be null.");
            return;
        }

        NewRule = this.rulesService.CreateNewRule(
            Bucket.Code,
            Description.Applicable ? Description.Value : null,
            [
                Reference1.Applicable ? Reference1.Value : null,
                Reference2.Applicable ? Reference2.Value : null,
                Reference3.Applicable ? Reference3.Value : null
            ],
            TransactionType.Applicable ? TransactionType.Value : null,
            Amount.Applicable ? Amount.Value : null,
            AndChecked);

        RuleCreated?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshSimilarRules()
    {
        if (SimilarRules is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(SimilarRules);
        view?.Refresh();
    }

    private void UpdateSimilarRules()
    {
        if (SimilarRules is null)
        {
            return;
        }

        this.logger.LogInfo(l => l.Format("UpdateSimilarRules1: Rules.Count() = {0}", SimilarRules.Count()));
        var view = CollectionViewSource.GetDefaultView(SimilarRules);
        view.Filter = item => this.rulesService.IsRuleSimilar((SimilarMatchedRule)item, Amount, Description, [Reference1, Reference2, Reference3], TransactionType);
        view.SortDescriptions.Add(new SortDescription(nameof(SimilarMatchedRule.SortOrder), ListSortDirection.Descending));

        SimilarRulesExist = !view.IsEmpty;
        OnPropertyChanged(nameof(SimilarRulesExist));
        OnPropertyChanged(nameof(SimilarRules));
        this.logger.LogInfo(l => l.Format("UpdateSimilarRules2: Rules.Count() = {0}", SimilarRules.Count()));
    }
}
