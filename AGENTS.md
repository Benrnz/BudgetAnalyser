# AGENTS.md - AI Agent Guide for Budget Analyser

## Project Overview

**Budget Analyser** is a 2-tier .NET 10.0 WPF desktop budgeting application with clean engine/UI separation. All business logic is in the **Engine** assembly; the UI is **WPF** using MVVM. Data is
persisted as JSON locally; no databases used.

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

**Important**: The Composition Root scans `BudgetAnalyser.Engine`, `BudgetAnalyser.Encryption`, and `BudgetAnalyser.Wpf` assemblies. Any new service must have this attribute or be manually registered
in `CompositionRoot`.

### 2. Private Field Naming Convention

Use `doNotUse` prefix for private backing fields **only in classes that implement `ControllerBase`, `ObservableRecipient`, or `INotifyPropertyChanged`**:

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

This convention signals "don't access directly; use the property." It's essential for MVVM-aware classes (Controllers, ViewModels, Models with property change notification, and Widgets) where direct
field access bypasses the change notification mechanism. **Do not use this prefix in regular service or utility classes** — use standard naming conventions there (e.g., `_description` or
`description`).

### 3. MVVM Structure: Controllers = ViewModels

Controllers inherit from `ControllerBase` (from Rees.Wpf):

- Extend `ObservableRecipient` (MVVM CommunityToolkit)
- Named with `Controller` suffix (e.g., `BudgetController`, `LedgerBookController`)
- Singleton instances managed by `IUiContext`
- Access other controllers via `uiContext.Controller<T>()`
- Use `Messenger.Register<>()` for cross-controller messaging
- AVOID placing any logic in the code-behind of XAML views, this breaks the MVVM pattern. Confirm with the user before adding any logic.

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

- **Models**: `StatementModel`, `BudgetModel`, `LedgerEntryModel`, `MatchingRule` - domain entities
- **Services**: `IStatement​Service`, `IBudgetService`, `ILedgerService`, `IReconciliationService`, `IMatchingRuleService`
- **Persistence**: `IApplicationDatabaseRepository` (JSON, loaded by `JsonOnDiskApplicationDatabaseRepository`)
- **GlobalFilterCriteria**: Date range filter applied across the app (changed centrally, affects all views)

### UI Layer (BudgetAnalyser.Wpf)

- **Controllers** (ViewModels) translate Engine services for UI
- **Facades**: `IApplicationDatabaseFacade` wraps `IApplicationDatabaseService` for UI use; notifies commands when data changes
- **Messaging**: Controllers use `Messenger` to send events like `BudgetReadyMessage`, `StatementReadyMessage`
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

- **Framework**: MSTest (not XUnit; ignore XUnit3 project)
- **Mocking**: Moq
- **Test Projects**:
    - `BudgetAnalyser.Engine.UnitTest` - Engine logic (uses embedded CSV test data)
    - `BudgetAnalyser.Wpf.UnitTest` - UI layer (uses Moq for engine services)
- **Test Data**: Either embedded as `EmbeddedResource` or in `../TestData/` shared folder
- **Key Test Utility**: `Rees.UnitTestUtilities` package

Example test usage:

```csharp
[TestClass]
public class MyServiceTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MethodName_ConditionDescription_ShouldThrowWhenNullInput() { }
}
```

### Running Tests from Terminal

```powershell
dotnet test BudgetAnalyser.Engine.UnitTest
dotnet test BudgetAnalyser.Wpf.UnitTest
```

---

## Key Files & Where to Find Patterns

| Pattern                     | File(s)                                                                                                  |
|-----------------------------|----------------------------------------------------------------------------------------------------------|
| IoC Registration            | `CompositionRoot.cs`, `DefaultIoCRegistrations.cs`, `AutoRegisterWithIoCAttribute.cs`                    |
| Controller/ViewModel        | `LedgerBook/LedgerBookController.cs`, `Statement/StatementController.cs`                                 |
| Model validation            | `BudgetAnalyser.Engine/IModelValidate.cs`                                                                |
| Messaging                   | `ConcurrentMessenger.cs` (wraps `WeakReferenceMessenger.Default`)                                        |
| MVVM base class             | `Rees.Wpf/ControllerBase.cs`                                                                             |
| Data persistence            | `Engine/Persistence/JsonOnDiskApplicationDatabaseRepository.cs`                                          |
| `doNotUse` field convention | `Budget/BudgetModel.cs`, `*Controller.cs`, `Widgets/Widget.cs` (for INotifyPropertyChanged classes only) |
| Widget/Dashboard binding    | `Engine/Widgets/Widget.cs`                                                                               |
| File dialogs/prompts        | `UserPrompts.cs`                                                                                         |

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

Optional; if encrypted files needed:

1. Users set credential via `ApplicationDatabaseFacade.SetCredential()`
2. `IFileEncryptor` handles read/write
3. `LocalDiskReaderWriterSelector` chooses encrypted or unencrypted reader

---

## New Feature Checklist

1. **Add Engine service** → Create in `Engine/` folder + `[AutoRegisterWithIoC]` attribute
2. **Add Controller** → Extend `ControllerBase`, add `[AutoRegisterWithIoC(SingleInstance = true)]`, register in `CompositionRoot.ConstructUiContext()` type array
3. **Add XAML View** → Place in UI folder, bind to Controller via `DataContext`
4. **If global filter impacts view** → Subscribe to `GlobalFilterController` messages
5. **Save/load data** → Use `IApplicationDatabaseFacade.NotifyOfChange()` to mark unsaved changes
6. **Add tests** → Create parallel test project, use MSTest + Moq, embed test data as `EmbeddedResource`

---

## Anti-Patterns to Avoid

❌ Accessing Engine services directly from Views (bypass Controller abstraction)
❌ Using Service Locator outside CompositionRoot (use constructor injection)
❌ Threading exceptions silently (use `ILogger`)
❌ Public mutable fields (use private `doNotUse` + property)
❌ Storing references to Controllers outside `IUiContext` (breaks singleton pattern)



