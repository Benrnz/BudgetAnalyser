using System.Security;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Mobile;

public class UploadMobileDataController : ControllerBase, IShellDialogInteractivity
{
    private readonly string[] amazonRegions =
    {
        "ap-southeast-2",
        "ap-southeast-1",
        "cn-north-1",
        "eu-central-1",
        "eu-central-1",
        "eu-west-1",
        "sa-east-1",
        "us-east",
        "us-west-1",
        "us-west-2",
        "ap-northeast-1",
        "ap-northeast-2"
    };

    private readonly IApplicationDatabaseFacade appDbService;
    private readonly Guid correlationId = Guid.NewGuid();
    private readonly IMobileDataExporter dataExporter;
    private readonly ILogger logger;
    private readonly IUserMessageBox messageBoxService;
    private readonly IMobileDataUploader uploader;
    private string doNotUseAccessKeyId = string.Empty;
    private string doNotUseAccessKeySecret = string.Empty;
    private string doNotUseAmazonRegion = string.Empty;
    private UpdateMobileDataWidget? widget;

    public UploadMobileDataController(
        IUiContext uiContext,
        IMobileDataExporter dataExporter,
        IMobileDataUploader uploader,
        IApplicationDatabaseFacade appDbService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.dataExporter = dataExporter ?? throw new ArgumentNullException(nameof(dataExporter));
        this.uploader = uploader ?? throw new ArgumentNullException(nameof(uploader));
        this.appDbService = appDbService ?? throw new ArgumentNullException(nameof(appDbService));
        this.messageBoxService = uiContext.UserPrompts.MessageBox;
        this.logger = uiContext.Logger;

        Messenger.Register<UploadMobileDataController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
        Messenger.Register<UploadMobileDataController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogMessageReceived(m));
    }

    public string AccessKeyId
    {
        get => this.doNotUseAccessKeyId;
        set
        {
            if (value == this.doNotUseAccessKeyId)
            {
                return;
            }

            this.doNotUseAccessKeyId = value;
            OnPropertyChanged();
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public string AccessKeySecret
    {
        get => this.doNotUseAccessKeySecret;
        set
        {
            if (value == this.doNotUseAccessKeySecret)
            {
                return;
            }

            this.doNotUseAccessKeySecret = value;
            OnPropertyChanged();
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public string AmazonRegion
    {
        get => this.doNotUseAmazonRegion;
        set
        {
            if (value == this.doNotUseAmazonRegion)
            {
                return;
            }

            this.doNotUseAmazonRegion = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<string> AmazonRegions => this.amazonRegions;

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteCancelButton => true;

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteOkButton => AccessKeyId.IsSomething() && AccessKeySecret.IsSomething();

    /// <summary>
    ///     Will be called to ascertain the availability of the button.
    /// </summary>
    public bool CanExecuteSaveButton => false;

    private async Task AttemptUploadAsync(BudgetModel budget, StatementModel statement, Engine.Ledger.LedgerBook ledgerBook, GlobalFilterCriteria filter)
    {
        if (this.widget is null)
        {
            this.logger.LogError(_ => "Widget cannot be null when attempting to Upload mobile data.");
            return;
        }

        try
        {
            this.widget.LockWhileUploading(true);
            // This validation should already be done by the Widget itself, but double check here.
            var export = this.dataExporter.CreateExportObject(statement, budget, ledgerBook, filter);

            await Task.Run(() => this.dataExporter.SaveCopyAsync(export));

            await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export), AccessKeyId, AccessKeySecret, AmazonRegion));
        }
        catch (SecurityException ex)
        {
            this.logger.LogError(ex, _ => "A Security Exception occured attempting to upload Mobile Summary Data.");
            this.messageBoxService.Show(ex.Message, "Invalid Credentials");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, _ => "An unexpected Exception occured attempting to upload Mobile Summary Data.");
            this.messageBoxService.Show(ex.Message, "Upload Failed");
        }
        finally
        {
            this.widget.LockWhileUploading(false);
        }
    }

    private async void OnShellDialogMessageReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.correlationId))
        {
            return;
        }

        try
        {
            if (message.Response == ShellDialogButton.Cancel || this.widget is null)
            {
                return;
            }

            var budget = this.widget.BudgetCollection?.CurrentActiveBudget ?? throw new InvalidOperationException("Budget cannot be null when attempting to Upload mobile data.");
            var statement = this.widget.StatementModel ?? throw new InvalidOperationException("StatementModel cannot be null when attempting to Upload mobile data.");
            var ledgerBook = this.widget.LedgerBook ?? throw new InvalidOperationException("LedgerBook cannot be null when attempting to Upload mobile data.");
            var filter = this.widget.Filter ?? throw new InvalidOperationException("Filter cannot be null when attempting to Upload mobile data.");
            var mobileSettings = this.widget.LedgerBook.MobileSettings ??
                                 throw new InvalidOperationException("LedgerBook.MobileSettings cannot be null when attempting to Upload mobile data. Mobile has not been configured.");
            var changed = AccessKeyId != mobileSettings.AccessKeyId;
            changed |= AccessKeySecret != mobileSettings.AccessKeySecret;
            changed |= AmazonRegion != mobileSettings.AmazonS3Region;
            if (changed)
            {
                mobileSettings.AccessKeyId = AccessKeyId;
                mobileSettings.AccessKeySecret = AccessKeySecret;
                mobileSettings.AmazonS3Region = AmazonRegion;
                this.appDbService.NotifyOfChange(ApplicationDataType.Ledger);
            }

            await AttemptUploadAsync(budget, statement, ledgerBook, filter);
        }
        finally
        {
            this.widget = null;
        }
    }

    private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
    {
        this.widget = message.Widget as UpdateMobileDataWidget;
        if (this.widget is not null && this.widget.Enabled)
        {
            var ledgerBook = this.widget.LedgerBook ?? throw new InvalidOperationException("LedgerBook cannot be null when attempting to Upload mobile data.");
            var mobileSettings = ledgerBook.MobileSettings ??
                                 throw new InvalidOperationException("LedgerBook.MobileSettings cannot be null when attempting to Upload mobile data. Mobile has not been configured.");
            AccessKeyId = mobileSettings.AccessKeyId;
            AccessKeySecret = mobileSettings.AccessKeySecret;
            AmazonRegion = mobileSettings.AmazonS3Region;
            Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.correlationId,
                Title = "Upload Mobile Summary Data"
            });
        }
    }
}
