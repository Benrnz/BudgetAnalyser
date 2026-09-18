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
