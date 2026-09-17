# Code Cleanliness Plan

A sequenced plan for removing roughly **7,100 lines** (~12% of the repo) without changing behaviour.

- **Reviewed at:** commit `2b72fde`, branch `claude/laughing-goldberg-ka9fgw`
- **Repo at review time:** 632 `.cs` files, 59,244 lines, net10.0 / C# 14
- **Not compiled:** the review environment had no .NET SDK (and this is a Windows/WPF app), so line counts and usage claims come from reading and searching the tree. Tier 1 figures are measured; Tier 2 and 3 savings are estimates.

Each task has a checkbox, the exact files, and a verification step. Tasks within a phase are independent unless stated — do them in any order, one commit each.

---

## Before you start: one constraint

**1. "Least code" cannot mean terser syntax here.**

`.editorconfig` sets `csharp_style_expression_bodied_methods = false:error` and `csharp_prefer_braces = true:error`, and every project sets `TreatWarningsAsErrors`. So `=>` method bodies and brace-less `if`s are **build failures**, not preferences. Every snippet below keeps its braces and statement bodies.

Expression-bodied *properties*, *indexers* and *accessors* are explicitly preferred (`= true:error`), so those are fair game.


---

## Phase 1 — Delete outright

**~3,900 lines + 5.5 MB. Measured, not estimated.** Every item here was verified unreferenced by searching the whole tree including XAML resource keys (WPF hides usage behind string lookups, so a plain symbol search is not enough).

No behaviour to verify: the compiler confirms each deletion. This is the afternoon's work with the best ratio.

### 1.1 — Stop committing generated metrics files

- [ ] Add to `.gitignore`:
  ```gitignore
  *.Metrics.xml
  ```
- [ ] `git rm --cached` the six tracked files:
  - `BudgetAnalyser/BudgetAnalyser.Wpf.Metrics.xml` — 2.0 MB
  - `BudgetAnalyser.Engine/BudgetAnalyser.Engine.Metrics.xml` — 1.7 MB
  - `BudgetAnalyser.Engine.XUnit3/BudgetAnalyser.Engine.XUnit3.Metrics.xml` — 1.5 MB
  - `BudgetAnalyser.Wpf.XUnit3/BudgetAnalyser.Wpf.XUnit3.Metrics.xml` — 195 KB
  - `Rees.Wpf/Rees.Wpf.Metrics.xml` — 145 KB
  - `BudgetAnalyser.Encryption/BudgetAnalyser.Encryption.Metrics.xml` — 30 KB

**Why:** 5.5 MB of `dotnet build -t:Metrics` output is tracked in git. Every metrics run rewrites megabytes of XML into your diffs. Two lines of work.

**Verify:** `dotnet build -t:Metrics` then `git status` — should report nothing.

---

### 1.2 — Replace `MatchingRulesTestDataGenerated.cs` with a JSON resource

- [ ] Serialise the 167 rules in `BudgetAnalyser.Engine.XUnit3/TestData/MatchingRulesTestDataGenerated.cs` to a JSON embedded resource
- [ ] Load it in `MatchMakerTest` through the existing `MapperMatchingRuleToDto2`
- [ ] Delete the 2,612-line file

**Why:** 2,612 lines of `[GeneratedCode]` construct 167 `MatchingRule` objects in one method, consumed by exactly one test class (`MatchMakerTest`). The generator named in the attribute — `MatchingRulesTestData.ConvertToDomainAndGenerateCSharp` — no longer exists in the repo, so this is frozen output nobody can regenerate. The project already embeds JSON test data as `EmbeddedResource`, including `TestData/DemoMatchingRules.json`.

**Check first:** if `DemoMatchingRules.json` already covers these cases, you may be able to point `MatchMakerTest` at it and skip the serialisation step entirely.

**Verify:** `dotnet test BudgetAnalyser.Engine.XUnit3` — `MetaTest` enforces `MinimumTestCount = 1061`, so a lost test surfaces immediately.

---

### 1.3 — Delete the WPF test project's forked fixtures

- [ ] `BudgetAnalyser.Wpf.XUnit3/TestData/TransactionsListModelTestData.cs` — 559 lines
- [ ] `BudgetAnalyser.Wpf.XUnit3/TestData/TestDataConstants.cs` — 32 lines
- [ ] `BudgetAnalyser.Wpf.XUnit3/FakeLogger.cs` — identical to the Engine copy but for `virtual`
- [ ] Add `using BudgetAnalyser.Engine.XUnit.TestData;` and `using BudgetAnalyser.Engine.XUnit.TestHarness;` where the compiler asks

**Why:** `BudgetAnalyser.Wpf.XUnit3.csproj` already has a `ProjectReference` to `BudgetAnalyser.Engine.XUnit3`, so it can use those fixtures directly. Instead it carries copies that have drifted — the WPF copy still sets `LastImport = new DateTime(2013, 08, 15)` where the Engine copy has moved to `new DateTime(new DateOnly(2013, 8, 14), new TimeOnly(12, 0, 0), DateTimeKind.Utc)`. Two sets of test data claiming to describe the same scenario is worse than one.

**Leave alone:** `MetaTest.cs` is also duplicated but legitimately differs (per-project `MinimumTestCount`). Optional: give it a shared base with an abstract count.

**Watch for:** the Engine `FakeLogger` methods are `virtual`, the WPF one's are not. If a WPF test overrides them, keep that working.

---

### 1.4 — Delete the `MapperGeneration` project

- [ ] Delete the `MapperGeneration/` directory (12 files, 261 lines of `.cs`)

**Why, in full — this is safe:**
- Not referenced by `BudgetAnalyser.sln` (`grep -i mapper BudgetAnalyser.sln` returns nothing)
- Old non-SDK project format targeting `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`
- Resolves every dependency through `..\packages\`, a folder that does not exist in the repo
- Its `ProjectReference` names Engine GUID `{c0bab6f9-…}`; the solution says `{8348F53B-…}` — stale for years
- The mapper library it drove, `Rees.TangyFruitMapper`, is gone. Mapping is now hand-written `IDtoMapper` implementations in `BudgetAnalyser.Engine/*/Data/`

It is the only .NET Framework artefact left in the tree.

---

### 1.5 — Retire `DateTimeExtension` in favour of `DateOnlyExt`

- [ ] In `BudgetAnalyser/ReportsCatalog/TopReportsCatalogController.cs`, lines **97** and **112**: change `DateTime.Today.FirstDateInMonth()` to `DateOnlyExt.Today().FirstDateInMonth()`
- [ ] Delete `BudgetAnalyser.Engine/DateTimeExtension.cs` — 68 lines
- [ ] Delete `BudgetAnalyser.Engine.XUnit3/DateTimeExtensionTest.cs` — 153 lines

**Why:** the domain migrated to `DateOnly`, and `DateOnlyExt` re-implements all five methods — `DurationInMonths`, `DurationInWeeks`, `FindNextWeekday`, `FirstDateInMonth`, `LastDateInMonth`. After the two edits above, `DateTimeExtension` has zero production callers. `DateOnlyExtensionTest.cs` already covers the `DateOnly` equivalents case for case, so no coverage is lost.

**Note:** both call sites feed `.ToString("yyyy-MM-dd")`, so check the format string still applies to `DateOnly` (it does, but confirm the rendered value in the report's date pickers).

### 1.5b — Two smaller items in the same files

- [ ] Delete `EnumerableExtension.SafeAverage` in `BudgetAnalyser.Engine/IEnumerableExtension.cs` — zero callers in production *and* zero in tests
- [ ] Replace the body of `DateOnlyExt.Today()`:
  ```csharp
  public static DateOnly Today()
  {
      return DateOnly.FromDateTime(DateTime.Today);
  }
  ```

---

### 1.6 — Delete three unbound converters

Their resource keys appear exactly once across all XAML and C# — the declaration itself. Nothing binds to them.

- [ ] `Rees.Wpf/Converters/MultiBoolToVisibilityConverter.cs` (70 lines) + key `Converter.MultiBoolToVisibility` from `BudgetAnalyser/UI/ConvertersDictionary.xaml`
- [ ] `BudgetAnalyser/Converters/ImagePathConverter.cs` (30 lines) + key `Converter.ImagePath`
- [ ] `Rees.Wpf/Converters/DebuggerConverter.cs` (42 lines) + key `Converter.Debugger` — **your call:** this is a binding-debug aid, so keeping the class is defensible. If you keep it, still drop the dictionary entry; a converter you reach for while debugging doesn't need instantiating at startup.

**Verify:** run the app and click through each view. Missing converter keys are runtime `XamlParseException`s, not compile errors — the compiler will not catch a mistake here.

---

### 1.7 — Delete `Rees.Wpf/GlobalSuppressions.cs`

- [ ] Delete the file — all 33 lines

**Why:** it suppresses CA1020 for namespaces `Rees.Wpf.RecentFiles` and `Rees.Wpf.ApplicationState`, and CA1006 for four members of `IRecentFileManager`. None of those types or namespaces exist anywhere in the repo. The entries also use legacy FxCop `Target = "…#Method(System.String)"` syntax, which modern Roslyn analysers ignore regardless.

**Keep:** `BudgetAnalyser.Engine/GlobalSuppressions.cs` is still valid — `BudgetAnalyser.Engine.Matching.Data` does exist.

---

## Phase 2 — Consolidate the build

**~95 lines, and it closes a real gap.** Do this before Phase 3 so the sweeps that follow are checked by the same compiler settings everywhere.

### 2.1 — Add `Directory.Build.props`

- [ ] Create `Directory.Build.props` at the repo root with the settings currently pasted across six `.csproj` files: `Nullable`, `ImplicitUsings`, `LangVersion`, `TreatWarningsAsErrors`, and the `4.0.0` version triplet
- [ ] Remove those properties from the individual project files
- [ ] Optional: add `Directory.Packages.props` for central package versions — `JetBrains.Annotations` 2026.2.0 and `Microsoft.CodeAnalysis.Metrics` 5.6.0 are currently listed per-project and kept in step by hand

**The gap this closes:** `BudgetAnalyser.Wpf.XUnit3.csproj` is the one project missing **both** `ImplicitUsings` and `Nullable`. That single omission is why its 19 files carry 33 lines of `using System;` / `using System.Linq;` / `#nullable enable` that no other project in the solution needs — and why its code is the only code in the repo not null-checked by the compiler.

- [ ] After the props file applies `Nullable` to the WPF test project, delete those 33 redundant lines
- [ ] Expect new nullable warnings in `BudgetAnalyser.Wpf.XUnit3` — with `TreatWarningsAsErrors` these are build errors. Budget time for this; it is the one part of Phase 2 that is not mechanical.

### 2.2 — Collapse the embedded resource list

- [ ] In `BudgetAnalyser.Engine.XUnit3.csproj`, replace the 11 five-line `EmbeddedResource` entries with one wildcard item:
  ```xml
  <EmbeddedResource Include="..\TestData\*">
      <Link>TestData\%(Filename)%(Extension)</Link>
  </EmbeddedResource>
  ```

**Verify:** tests that load embedded resources by name must still find them — `JsonOnDiskBudgetRepositoryTest` and `JsonOnDiskLedgerBookRepositoryTest` are the ones to watch. Confirm the generated resource names are unchanged.

---

## Phase 3 — Collapse the forked copies

**~1,550 lines.** Five sets of near-identical files. In each case the variation is *data* — a column index, a DTO type, a comparison function — and the structure around it was copied verbatim. These are the refactors that stop the next bug fix from needing to be applied three times.

One commit per item, tests between. Start with 3.1 and 3.2: they are the largest and the most mechanical.

### 3.1 — One base class for the four bank importers

**~450 of 910 lines.**

- [ ] Add `CsvBankExtractImporterBase : IBankExtractImporter` in `BudgetAnalyser.Engine/Transactions/`
- [ ] Move `LoadAsync`, `TasteTestAsync`, `ReadLinesAsync`, `ReadTextChunkAsync`, `ReadFirstTwoLinesAsync` and `VerifyColumnHeaderLine` onto it
- [ ] Reduce each importer to its columns, header line, column count and `ParseLine`

```csharp
internal abstract class CsvBankExtractImporterBase : IBankExtractImporter
{
    protected abstract string ExpectedHeaderLine { get; }
    protected abstract int ExpectedColumnCount { get; }
    protected abstract Transaction ParseLine(string[] split, Account account);
    protected abstract bool VerifyFirstDataLine(string[] split);

    // LoadAsync, TasteTestAsync, ReadLinesAsync, ReadTextChunkAsync,
    // ReadFirstTwoLinesAsync, VerifyColumnHeaderLine — written once.
}
```

**Evidence:** `diff AnzAccountExtractImporterV1.cs WestpacAccountExtractImporterV1.cs` reports differences in only the class name, the doc comment, the seven column-index constants, the expected header string, the expected column count, and one local renamed from `cachedType` to `type`. Two 204-line files.

**The two wrinkles:**
- `AsbAccountExtractImporterV1` (284 lines) handles a header line that can be row 5 *or* row 6 — see commit `7ebfa78`. Keep that as an override.
- `AnzVisaExtractImporterV1` (218 lines) flips the sign from a debit/credit type column and seeds `TransactionTypes` with two fixed entries. Its `ParseLine` needs the transaction type before the amount, so have `ParseLine` own that ordering rather than the base.

**Keep overridable:** the existing `*ImporterV1TestHarness` classes override the read methods. Keep them `protected virtual` on the base and the harnesses work unchanged.

**Verify:** `BankImportUtilitiesTest` (898 lines) and the per-bank importer tests. `TestData/DemoTransactions.csv` exercises the real path.

---

### 3.2 — One base class for the five JSON repositories

**~350 of 753 lines.**

- [ ] Add a generic base over `<TDto, TModel>` holding `LoadJsonFromDiskAsync`, `MapToDto`, `SaveToDiskAsync`, `SerialiseAndWriteToStream`, the blank-key guard, the file-exists guard, the `DataFormatException` wrapper and the `LogInfo` bookends
- [ ] Migrate: `JsonOnDiskWidgetRepository`, `JsonOnDiskMatchingRuleRepository`, `JsonOnDiskBudgetRepository`, `JsonOnDiskLedgerBookRepository`, `JsonOnDiskApplicationDatabaseRepository`

**Two drifts this fixes for free:**
- Budget and LedgerBook cache `private static readonly JsonSerializerOptions Options = new();`. Widget and Matching allocate a fresh `JsonSerializerOptions` on **every call**, which defeats `System.Text.Json`'s metadata cache. Put the cached instance on the base.
- `JsonOnDiskWidgetRepository` still reports *"an exception was thrown by the **Xaml** deserialiser"* — left over from before the JSON migration.

**Not uniform — plan for it:** `JsonOnDiskLedgerBookRepository` adds a checksum (`CalculateChecksum`, `UpdateModifiedDate`) and `JsonOnDiskBudgetRepository` caches `currentBudgetCollection` + `isEncryptedAtLastAccess` and has a parameterless `SaveAsync()`. Those stay on the subclasses. The shared part is the stream/serialise/guard layer, not the whole interface.

**Also keep overridable:** each repository has a `*TestHarness` that overrides the read/write methods.

- [ ] While in `JsonOnDiskMatchingRuleRepository`, rewrite `PreventDuplicates` — currently 30 lines and a `do/while` that restarts the scan after each removal (O(n²)):

```csharp
private IList<MatchingRule> PreventDuplicates(IList<MatchingRule> model)
{
    var seen = new HashSet<Guid>();
    var result = new List<MatchingRule>(model.Count);
    foreach (var rule in model)
    {
        if (seen.Add(rule.RuleId))
        {
            result.Add(rule);
            continue;
        }

        this.logger.LogWarning(_ => $"Duplicate RuleID found and will be removed: {rule.RuleId} …");
    }

    return result;
}
```

**Verify:** `JsonOnDiskBudgetRepositoryTest` (355 lines), `JsonOnDiskLedgerBookRepositoryTest` (339 lines), and the encrypted round-trip via `DemoBudget.json.secure` / `DemoLedgerBook.json.secure`.

---

### 3.3 — One base for the value converters

**~375 of 707 lines.**

- [ ] Add `OneWayValueConverter` in `Rees.Wpf/Converters/` and derive all 13 from it:

```csharp
public abstract class OneWayValueConverter : IValueConverter
{
    public abstract object Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    /// <summary>Not supported — these converters are one-way.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

- [ ] Merge `NullToBoolConverter` / `NotNullToBoolConverter` into one type with an `Invert` property — they are `value is null` and `value is not null`
- [ ] Merge `NullToVisibilityConverter` / `NotNullToVisibilityConverter` the same way. XAML can set properties on a converter instance, so both existing keys stay and **no bindings change**:
  ```xml
  <converters1:NullToVisibilityConverter x:Key="Converter.NullToVis" />
  <converters1:NullToVisibilityConverter x:Key="Converter.NotNullToVis" Invert="True" />
  ```
- [ ] Simplify `ZeroToVisibilityConverter`'s five sequential type tests to one pattern:
  ```csharp
  return value is null or 0 or 0d or 0m or 0L ? Visibility.Hidden : Visibility.Visible;
  ```

**Why the base earns its keep:** every converter ends with the same `ConvertBack` throwing `NotSupportedException` under the same six-line XML comment — 13 copies of about 20 lines.

**Careful:** the two null-to-visibility converters do **not** currently agree on how they read the `parameter` string. `NullToVisibilityConverter` switches its test on whether the *value* is an empty string; `NotNullToVisibilityConverter` switches on whether the *parameter* is `""` or `"Empty"`. Before merging, check the 12 + 4 call sites in XAML to see which behaviour each relies on. This is the one converter change that is not purely mechanical.

---

### 3.4 — Unify `MatchingRule`'s two match methods

**~105 of 140 lines.** ⚠️ **Read [Decision 1](#decision-1--an-empty-matchingrule-matches-every-transaction) first — this forces a behaviour choice.**

- [ ] Replace `IsWholeFieldMatch` and `IsRegexMatch` in `BudgetAnalyser.Engine/Matching/MatchingRule.cs` with one method taking the comparison as a parameter:

```csharp
private bool Match(Transaction transaction, Func<string?, string?, bool> compare)
{
    (string? Criterion, string? Actual)[] fields =
    [
        (Description,     transaction.Description),
        (Reference1,      transaction.Reference1),
        (Reference2,      transaction.Reference2),
        (Reference3,      transaction.Reference3),
        (TransactionType, transaction.TransactionType.Name)
    ];

    var comparisons = 0;
    var matches = 0;
    foreach (var (criterion, actual) in fields)
    {
        if (string.IsNullOrWhiteSpace(criterion))
        {
            continue;
        }

        comparisons++;
        if (compare(actual, criterion))
        {
            matches++;
        }
    }

    if (Amount is not null)
    {
        comparisons++;
        if (transaction.Amount == Amount.Value)
        {
            matches++;
        }
    }

    if (comparisons == 0)
    {
        return false;
    }

    var matched = And ? matches == comparisons : matches >= 1;
    if (matched)
    {
        LastMatch = DateTime.Now;
        MatchCount++;
    }

    return matched;
}

private bool IsWholeFieldMatch(Transaction transaction)
{
    return Match(transaction, static (actual, criterion) => actual == criterion);
}

private bool IsRegexMatch(Transaction transaction)
{
    return Match(transaction, RegexIsMatch);
}
```

**Verify:** `MatchMakerTest` — which is exactly the test that depends on Phase 1.2, so do 1.2 first or you will be rewriting its fixture mid-refactor.

**Separate, optional:** `RegexIsMatch` calls the static `Regex.IsMatch` with a user-supplied pattern on every transaction × every rule, wrapped in `try/catch (ArgumentException)` to swallow invalid patterns. Consider compiling the pattern once when `Description`/`Reference*` changes and validating it at edit time in `EditRulesController` instead. That is a feature change, not cleanup — note it and move on.

---

### 3.5 — One `INotifyPropertyChanged` base in the Engine

**~90 lines across 11 classes.**

- [ ] Add an internal base holding the event and `OnPropertyChanged([CallerMemberName])`
- [ ] Derive: `BudgetBucket`, `BudgetItem`, `BudgetModel`, `GlobalFilterCriteria`, `TransferFundsCommand`, `Widget`, `MatchingRule`, `Criteria<T>`, `TransactionsListModel`, `Transaction`, and `BudgetAnalyser/LedgerBook/LedgerBookControllerFileOperations.cs`
- [ ] Drop the 18 `var handler = PropertyChanged; handler?.Invoke(…)` instances — `?.` already does that null-safe read

**Inconsistency this removes:** five of the eleven still use the `var handler` idiom and six have dropped it, so the file you open decides which you see.

**Constraint — the 11 signatures are not uniform.** Four are `protected virtual` (`BudgetBucket`, `TransferFundsCommand`, `Criteria<T>`, `LedgerBookControllerFileOperations`), four are `protected` (`BudgetItem`, `BudgetModel`, `MatchingRule`, `Widget`), and three are `private` (`GlobalFilterCriteria`, `Transaction`, `TransactionsListModel`). `protected virtual` on the base satisfies all of them — it only widens access on the three private ones, which is harmless since nothing outside those classes called them.

**Blocker for some:** `BudgetBucket` already inherits nothing, but check each class's existing base before assuming it is free. `Widget` is abstract with its own constructor logic — fine. `Transaction` implements `IComparable` and `ICloneable<Transaction>` — also fine.

**Worth knowing:** `CommunityToolkit.Mvvm`'s `ObservableObject` is platform-neutral and would do this job, but only `Rees.Wpf` references the toolkit today. Adding the package to the Engine is a judgement call about the Engine's dependency surface — the hand-written base avoids the question.

---

### 3.6 — Five smaller duplications

**~185 lines. Each is a single sitting.**

- [ ] **`WindowsOpenFileDialog` / `WindowsSaveFileDialog`** (182 lines, `Rees.Wpf/UserInteraction/`). Same seven properties, same `ShowDialog` with the same `try { ShowDialog(MainWindow) } catch (InvalidOperationException) { ShowDialog() }` fallback. A `FileDialogBase<TDialog> where TDialog : FileDialog, new()` leaves each subclass at ~15 lines. Note the Open variant has one extra property (`CheckFileExists`) the Save variant lacks.

- [ ] **`EngineIocRegistrations.AddAutoRegistrations`**. The singleton and transient branches are structurally identical — self-registration, keyed self-registration, then the same two for each interface. Hoist the lifetime into a variable and use `ServiceDescriptor`; ~50 lines becomes ~20. In the same file, `GetNamedInstance` is a 15-line loop that `FirstOrDefault` plus `?? throw` says in four.
  **Verify:** `EngineIocRegistrationsTest`, and launch the app — a missed registration surfaces as a `NullReferenceException` with no inner exception at startup (see the comment in `App.xaml.cs`).

- [ ] **`MapperIncomeToDto2` / `MapperExpenseToDto2`** (`BudgetAnalyser.Engine/Budget/Data/`). Identical but for the model type and the word "Income"/"Expense" in the error message. Both map `BudgetItem` subclasses with the same two fields. One generic base over `TModel : BudgetItem, new()` covers both and any future sibling. `IncomeDto` and `ExpenseDto` are also identical records — leave those separate, the persisted shape is not yours to merge.

- [ ] **`MapperWidgetToDto.ToDto`** (`BudgetAnalyser.Engine/Widgets/Data/`). The `BudgetBucketMonitorWidget` and `FixedBudgetMonitorWidget` cases are byte-identical apart from the pattern variable. Both expose `BucketCode` as their `IUserDefinedWidget.Id`, so one `case IUserDefinedWidget u` arm handles both — and the next multi-instance widget too.

- [ ] **`LedgerBookGridBuilderFactory`** (21 lines). It reaches into `TopLedgerBookController` for six `ICommand`s and passes them positionally to a constructor that stores them in six fields. Six adjacent same-typed parameters is a transposition the compiler cannot catch. Pass the controller — or a small `ILedgerBookCommands` it implements — and the factory becomes a single `new`, possibly deletable.

---

## Phase 4 — Idiom sweeps

**~1,700 lines.** Low-risk per site but each touches many files. Do one category at a time, on its own commit, with the test suite between. Never mix two sweeps in one commit — a regression becomes very hard to bisect.

### 4.1 — `[ObservableProperty]` on partial properties

**~1,200 lines. The biggest single lever in the repo.**

- [ ] Convert the hand-written property bodies in the WPF layer (115 of the 185 `OnPropertyChanged()` calls)
- [ ] Then the Engine's 70 — **after** 3.5, since they need the shared base first

`CommunityToolkit.Mvvm` 8.4.2 is already referenced and 8.4 supports `[ObservableProperty]` on **partial properties**, not just fields. Every controller derives from `ControllerBase : ObservableRecipient`, so the generator applies directly.

```csharp
// from
public string ValidationMessage
{
    get;
    private set
    {
        if (value == field)
        {
            return;
        }

        field = value;
        OnPropertyChanged();
    }
} = string.Empty;

// to
[ObservableProperty]
public partial string ValidationMessage { get; private set; } = string.Empty;
```

**Leave bodies in place** where the setter has real side effects, or move the extra work into the generated `OnXxxChanged` partial hook. Several setters in this codebase fire `OnPropertyChanged` for *other* properties too — those need `[NotifyPropertyChangedFor]`, not a blind conversion.

**Highest-density files:** `LedgerTransactionsController` (9), `NewRuleController` (8), `TopTransactionsListController` (7), `ShellDialogController` (7), `TopBudgetController` (7).

---

### 4.2 — `ArgumentNullException.ThrowIfNull`

**~480 lines across 486 sites.** Currently used exactly once in the whole repo.

- [ ] Replace the four-line guard blocks that mandatory braces force:
  ```csharp
  ArgumentNullException.ThrowIfNull(validationMessages);
  ```

Nullable reference types are enabled on every project, so a guard on a non-nullable parameter defends against callers the compiler already refuses. Keep them on public API surface and where an `object[]` crosses a boundary; consider deleting the rest.

- [ ] **Delete rather than rewrite:** `Widget.Update(params object[] input)` guards `input is null`, which `params` cannot produce
- [ ] **Fix the exception type:** repositories throw `ArgumentNullException` for `storageKey.IsNothing()` when the key is *blank* rather than null — that is `ArgumentException`. This one is a visible behaviour change; check no test asserts on the type.
- [ ] **Fix two constructors that guard after they assign** — see [Decision 3](#decision-3--two-constructors-guard-after-they-assign)

---

### 4.3 — Finish the `field` migration

**~100 lines across 12 files.**

44 files already use C# 14's `field` keyword; 12 still declare `doNotUse`-prefixed backing fields, 99 references in all. `ProgressBarWidget` shows both in one class — `Enabled` on `doNotUseEnabled`, the four properties below it on `field`.

- [ ] Convert the remaining 12 files, heaviest first: `EncryptFileController` (18), `BudgetModel` (15), `BudgetBucket` (14), `SplitTransactionController` (12), `NewRuleController` (10), `TopLedgerBookController` (5), `GlobalFilterController` (5), `ProgressBarWidget` (5), `GlobalFilterCriteria` (5), `ChooseBudgetBucketController` (4), `TransactionsListModel` (4)
- [ ] **Amend `.github/copilot-instructions.md`**, which still prescribes `doNotUse` as the rule — otherwise the doc keeps pulling new code back toward the old style

`field` makes the convention's purpose moot: there is no accessible field left to touch by mistake.

**Note:** if you do 4.1 first, most of these properties disappear into `[ObservableProperty]` anyway. Consider doing 4.1 and checking what is left.

---

### 4.4 — Leftovers from earlier framework versions

**~55 lines.** All small, all independent.

- [ ] **11 `GetTypeInfo()` calls.** A .NET Core 1.x necessity; `Type` has carried `Assembly`, `IsAbstract` and `GetCustomAttribute` directly since .NET Standard 2.0. `typeof(CompositionHelper).GetTypeInfo().Assembly` is just `typeof(CompositionHelper).Assembly`. In `EngineIocRegistrations.cs` (4), `StandardWidgetCatalog.cs` (2), `App.xaml.cs` (2), `CompositionHelper.cs` (3).

- [ ] **`CompositionHelper.LoadApplicationStateIntoControllers`.** It selects distinct `LoadSequence` values then re-filters the whole list once per value — what `GroupBy(m => m.LoadSequence).OrderBy(g => g.Key)` does in one pass. It also keeps a `var sequenceCopy = sequence;` loop-capture workaround that stopped being necessary in C# 5, and calls `CreateNewDefaultApplicationState()` — a five-line private method whose entire body returns an empty list, so `rehydratedState = []` says it.

- [ ] **Constant strings in `readonly` fields.** Seven in the widgets alone — `this.disabledToolTip`, `this.standardStyle` and friends are assigned literals in the constructor. `const` is the same text with no field and no constructor line, matching how `Widget` already declares `WidgetStandardStyle` and its siblings. In `BudgetBucketMonitorWidget`, `DateFilterWidget`, `FixedBudgetMonitorWidget` (3), `RemainingActualSurplusWidget`, `RemainingBudgetBucketWidget`.

- [ ] **`Array.Empty<Type>()` (11 sites) and `= new List<T>()` (36 sites)**, all expressible as `[]`. The codebase already uses collection expressions elsewhere — `Dependencies = [typeof(TransactionsListModel), …]`.

- [ ] **`StringExtension.AnOrA`.** Three branches over two booleans, and `instance.ToCharArray(0, 1)[0]` allocates an array to read `instance[0]`.

- [ ] **`FixedBudgetMonitorWidget`.** `Id` and `BucketCode` are two fields holding the same value, each setter writing the other. `BudgetBucketMonitorWidget` already does this correctly — `Id` is a facade over `BucketCode` with no second field. Also: the constructor sets `BucketCode = NotSet` *and* the property has an `= NotSet` initialiser, and `Update` repeats `ToolTip = disabledToolTip; Enabled = false; return;` three times.

- [ ] **`Widget.ValidateUpdateInput`.** Builds `dependencies` via `ToList()` then iterates `Dependencies` again instead, calls `.Count()` on a `List` twice, and `Dependencies` is typed `IEnumerable<Type>` so every call re-enumerates. Change the property to `IReadOnlyList<Type>` and use the list.

- [ ] **Typo:** `OnShellDiaglogResponseMessageReceived` → `OnShellDialogResponseMessageReceived`, 2 occurrences.

---

## Progress checks

Re-measure at any point. These use `git grep`, so they work the same in PowerShell, cmd and bash.

```sh
# Total lines of C#
git ls-files '*.cs' | xargs wc -l | tail -1

# 4.1 — hand-written property notifications remaining (was 185)
git grep -c 'OnPropertyChanged()' -- '*.cs' | awk -F: '{s+=$2} END {print s}'

# 4.1 — conversions done (was 0)
git grep -c '\[ObservableProperty\]' -- '*.cs' | awk -F: '{s+=$2} END {print s}'

# 4.2 — manual null guards remaining (was 486)
git grep -c 'throw new ArgumentNullException(nameof(' -- '*.cs' | awk -F: '{s+=$2} END {print s}'

# 4.3 — doNotUse references remaining (was 99 in 12 files)
git grep -l 'doNotUse' -- '*.cs'

# 3.5 — vestigial event-handler copies remaining (was 18)
git grep -n 'var handler = ' -- '*.cs'

# 4.4 — GetTypeInfo calls remaining (was 11)
git grep -n 'GetTypeInfo()' -- '*.cs'

# 1.1 — tracked metrics files remaining (was 6)
git ls-files '*.Metrics.xml'
```

**Finding dead XAML converters.** A converter can only be reached by resource key, so search the key, not the type name. A count of 1 means the declaration in `ConvertersDictionary.xaml` and nothing else:

```sh
git grep -c 'Converter.SomeKey' -- '*.xaml' '*.cs'
```

---

## Suggested order

1. **Phase 1** end to end — one afternoon, nothing to verify beyond a clean build and a click through the views for 1.6
2. **Phase 2.1** — `Directory.Build.props`, then fix the nullable warnings it surfaces in `BudgetAnalyser.Wpf.XUnit3`
3. **Phase 3.1 and 3.2** — the importers and repositories, where the duplication actually costs maintenance
4. **Decisions 1 and 2** — with tests written first
5. **Phase 3.3–3.6**, then **Phase 4**, one sweep per commit

Save **4.1** for last. It is the largest line reduction available but touches the most files, and it reads much better on top of an already tidy tree.
