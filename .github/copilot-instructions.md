# Copilot Instructions for BudgetAnalyser

## Project Overview

**Budget Analyser** is a 2-tier .NET 10.0 WPF desktop budgeting application with clean engine/UI separation. All business logic is in the **Engine** assembly; the UI is **WPF** using MVVM. Data is persisted as JSON locally; no databases are used.

### Key Architecture
- **Engine Assembly**: Contains all business logic, domain models, and services (knows nothing about WPF)
- **WPF UI Layer**: Controllers (ViewModels) translate Engine services for UI; uses MVVM CommunityToolkit
- **Data Persistence**: JSON files stored locally; optional encryption via `IFileEncryptor`
- **IoC Container**: Autofac with MVVM CommunityToolkit

### Key Architecture Files
- `CompositionRoot.cs` - IoC setup (Autofac + MVVM CommunityToolkit)
- `IUiContext.cs` / `UiContext.cs` - Ambient context for controllers + services
- `ApplicationDatabaseFacade.cs` - UI layer access to Engine services
- `ShellController.cs` - Top-level ViewModel/Controller

---

## Critical Patterns & Conventions

### 1. Automatic Dependency Injection via Attributes

Use `[AutoRegisterWithIoC]` attribute on concrete classes:

```csharp
[AutoRegisterWithIoC(SingleInstance = true)]
public class MyService : IMyService { }
```

- Scanned automatically by `DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly()`
- `SingleInstance = true` → Singleton; default is transient
- Set `Named = "instanceName"` for multiple implementations of same interface
- Classes are registered as both themselves and their interfaces
- The Composition Root scans `BudgetAnalyser.Engine`, `BudgetAnalyser.Encryption`, and `BudgetAnalyser.Wpf` assemblies

**Important**: Any new service must have this attribute or be manually registered in `CompositionRoot`.

### 2. Private Field Naming Convention: `doNotUse` Prefix

Use `doNotUse` prefix for private backing fields **ONLY in classes that implement or derive from**:
- `ControllerBase`
- `ObservableRecipient`
- `INotifyPropertyChanged`

```csharp
private string doNotUseDescription;

public string Description
{
    get => this.doNotUseDescription;
    set
    {
        this.doNotUseDescription = value;
        OnPropertyChanged(); // Only fires if value changed
    }
}
```

This convention signals "don't access directly; use the property." It's essential for MVVM-aware classes where direct field access bypasses change notification. **Do NOT use this prefix in regular service or utility classes** — use standard naming conventions instead (e.g., `_description`).

Use pascal casing for any other type of private field.
Do NOT prefix private fields with _ (underscore).

### 3. MVVM Structure: Controllers = ViewModels

Controllers inherit from `ControllerBase` (from Rees.Wpf):

- Extend `ObservableRecipient` (MVVM CommunityToolkit)
- Named with `Controller` suffix (e.g., `BudgetController`, `LedgerBookController`)
- Singleton instances managed by `IUiContext`
- Access other controllers via `uiContext.Controller<T>()`
- Use `Messenger.Register<>()` for cross-controller messaging

**Critical MVVM Rules:**
- ❌ **NEVER** place logic in code-behind of XAML views — this breaks MVVM
- ❌ **DO NOT** create new implementations of `ICommand`; use `CommunityToolkit.Mvvm.Input.RelayCommand` instead
- ✅ Confirm with user before adding any logic to view code-behind

Example:

```csharp
[AutoRegisterWithIoC(SingleInstance = true)]
public class MyController : ControllerBase, IShowableController
{
    private readonly IUiContext uiContext;

    public MyController(IUiContext uiContext, IMessenger messenger)
        : base(messenger)
    {
        this.uiContext = uiContext;
    }

    // Access other controllers when needed
    var batchController = this.uiContext.Controller<AnotherController>();
}
```

### 4. Ambient Context Pattern (IUiContext)

Common services are injected via `IUiContext` instead of individual constructor parameters:

```csharp
public class SomeController : ControllerBase
{
    public SomeController(IUiContext uiContext, IMessenger messenger) : base(messenger)
    {
        var logger = uiContext.Logger; // Not in ctor!
        var prompts = uiContext.UserPrompts;
        var otherController = uiContext.Controller<OtherController>();
    }
}
```

---

## Core Data Flow & Key Components

### Engine Architecture (BudgetAnalyser.Engine)

The Engine is domain-logic only; it doesn't know about WPF or messaging:

- **Models**: `TransactionsListViewModel`, `BudgetModel`, `LedgerEntryModel`, `MatchingRule` - domain entities
- **Services**: `ITransactionManagerService`, `IBudgetService`, `ILedgerService`, `IReconciliationService`, `IMatchingRuleService`
- **Persistence**: `IApplicationDatabaseRepository` (JSON, loaded by `JsonOnDiskApplicationDatabaseRepository`)
- **GlobalFilterCriteria**: Date range filter applied across the app (changed centrally, affects all views)

### UI Layer (BudgetAnalyser.Wpf)

- **Controllers** (ViewModels) translate Engine services for UI
- **Facades**: `IApplicationDatabaseFacade` wraps `IApplicationDatabaseService` for UI use; notifies commands when data changes
- **Messaging**: Controllers use `Messenger` to send events like `BudgetReadyMessage`, `TransactionsListModelReadyMessage`
- **Shell**: `ShellController` + `ShellWindow` (main window); contains `ShellDialogView` for modal dialogs

### Data Persistence

- **AppState** (UI metadata): Saved to JSON by `PersistBaxAppStateAsJson` (window size, last file loaded)
- **Engine State** (budget data): Saved by `IApplicationDatabaseService` as JSON in `ApplicationDatabase`
- Files encrypted using `IFileEncryptor` (optional); credentials stored in `ICredentialStore`

---

## Developer Workflows

### Building

```powershell
dotnet build                          # Normal build
dotnet build -t:Metrics              # Update metrics XML files (complexity, maintainability)
```

### Testing

- **Framework**: MSTest or XUnit
- **Mocking**: NSubstitute (mandatory; do NOT use Moq)
- **Assertions**: Shouldly (preferred over plain Assert statements)
- **Test Projects**:
    - `BudgetAnalyser.Engine.UnitTest` - Engine logic (uses embedded CSV test data)
    - `BudgetAnalyser.Wpf.UnitTest` - UI layer (uses NSubstitute for engine services)
- **Test Data**: Either embedded as `EmbeddedResource` or in `../TestData/` shared folder
- **Key Test Utility**: `Rees.UnitTestUtilities` package

Example test usage:

```csharp
[TestClass]
public class MyServiceTest
{
    [TestMethod]
    public void MethodName_ConditionDescription_ShouldThrowWhenNullInput()
    {
        var mockService = Substitute.For<IService>();
        var sut = new MyClass(mockService);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => sut.DoSomething(null));
        ex.ParamName.ShouldBe("parameter");
    }
}
```

**Rules and Guidance for Unit Testing**:
- Use **NSubstitute** for mocking — do not use Moq
- Use **Shouldly** for assertions instead of plain `Assert` for more fluent, readable test code
- Arrange-Act-Assert (AAA) pattern should be clearly evident in test structure

### Running Tests from Terminal

```powershell
dotnet test BudgetAnalyser.Engine.UnitTest
dotnet test BudgetAnalyser.Wpf.UnitTest
```

---

## Cross-Cutting Concerns

### Logging

```csharp
private readonly ILogger logger;
logger.LogInfo(_ => "Message"); // Error, Warning, Info levels
```

### Validation

Implement `IModelValidate`:

```csharp
public bool Validate(StringBuilder messages)
{
    if (invalid) messages.Append("Error details");
    return messages.Length == 0;
}
```

### Change Detection

Implement `IDataChangeDetection` to track dirty state for save operations.

### Encryption

Optional; if encrypted files are needed:

1. Users set credential via `ApplicationDatabaseFacade.SetCredential()`
2. `IFileEncryptor` handles read/write
3. `LocalDiskReaderWriterSelector` chooses encrypted or unencrypted reader

---

## Code Style & Standards

- Use C# coding conventions and best practices
- Follow existing project structure and naming conventions
- Maintain consistency with existing code patterns
- Use appropriate access modifiers (private by default unless public API needed)
- Use meaningful variable and method names that clearly describe intent

### Comments & Documentation

- Add XML documentation comments (`///`) to public methods and classes
- Include meaningful comments for complex logic or non-obvious algorithm choices
- Document parameter constraints and expected return values
- Comment the "why", not just the "what"

---

## Project Terminology

- **Bucket**: A container representing a budget category with a set monthly allocation
- **Spent-Monthly-Bucket**: Automatically empties remaining funds to Surplus at month-end
- **Surplus**: Remaining unallocated or saved funds
- **Ledger Book**: The financial record that tracks transactions and reconciliation

---

## New Feature Checklist

1. **Add Engine service** → Create in `Engine/` folder + `[AutoRegisterWithIoC]` attribute
2. **Add Controller** → Extend `ControllerBase`, add `[AutoRegisterWithIoC(SingleInstance = true)]`, register in `CompositionRoot.ConstructUiContext()` type array
3. **Add XAML View** → Place in UI folder, bind to Controller via `DataContext`
4. **If global filter impacts view** → Subscribe to `GlobalFilterController` messages
5. **Save/load data** → Use `IApplicationDatabaseFacade.NotifyOfChange()` to mark unsaved changes
6. **Add tests** → Create parallel test project, use MSTest + NSubstitute + Shouldly, embed test data as `EmbeddedResource`

---

## Anti-Patterns to Avoid

❌ Accessing Engine services directly from Views (bypass Controller abstraction)
❌ Using Service Locator outside CompositionRoot (use constructor injection)
❌ Threading exceptions silently (use `ILogger`)
❌ Public mutable fields (use private `doNotUse` + property when the containing class derives from `ObservableRecipient` and `INotifyPropertyChanged`, or use private fields with camelcasing for any
other type)
❌ Storing references to Controllers outside `IUiContext` (breaks singleton pattern)
❌ Using Moq for mocking (use NSubstitute)
❌ Using plain Assert statements (use Shouldly)
❌ Using custom ICommand implementations (use RelayCommand)

---

## Dependencies

- **NuGet**: `Rees.UserInteraction.Contracts`, `Rees.Wpf`, `Rees.UnitTestUtilities`
- **Testing**: MSTest, NSubstitute, Shouldly
- **Framework**: .NET 10.0 SDK or runtime
- **Platform**: Windows OS (due to WPF dependency)

---

## Security & Privacy

- Never suggest uploading user data to external services
- Remember that all user financial data must remain local to the user's machine
- No telemetry or network communication should be added without explicit consideration
- Validate and sanitize all imported data (e.g., CSV bank statements)
- This application does not upload any information to the Internet
