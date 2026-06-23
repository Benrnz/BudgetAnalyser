Plan: Fix Split Transaction auto-calculation bug

Problem
- The Split Transaction UI automatically attempts to recalculate the other amount while the user types into one amount field, causing surprising edits. The two split amounts must always sum to the original transaction amount, and automatic edits while typing should be removed.

Goal
- Remove any automatic live recalculation. Add a small button to the right of each amount text box. When clicked, the button calculates the adjacent field's amount based on the other field. The two amounts must sum exactly to the original transaction amount (accounting for currency rounding to 2 decimal places).

Files to change
- BudgetAnalyser\Statement\SplitTransactionController.cs
- BudgetAnalyser\Statement\SplitTransactionView.xaml
- BudgetAnalyser\Statement\SplitTransactionView.xaml.cs
- DO NOT CHANGE ANY OTHER FILES THAN THE ABOVE LISTED FILES.

Approach
1. Investigate current calculation flow: find handlers that update the other field on property/text changes. Identify the single place(s) causing automatic updates.
2. Remove or disable any automatic recalculation logic triggered by TextChanged/PropertyChanged while editing.
3. Add two ICommand implementations in SplitTransactionController: CalculateLeftFromRightCommand and CalculateRightFromLeftCommand (or a single command with a parameter to indicate target side). These commands compute target = originalAmount - sourceAmount (with rounding) and set the target field.
4. Update XAML: place a small calculate button to the right of each amount TextBox. Bind its Command to the controller commands. Ensure focus/selection remains sensible after clicking.
5. Ensure arithmetic uses decimal (not double). Calculation rules: parse source amount as decimal, round source to 2 decimal places, compute target = originalAmount - roundedSource, then round target to 2 decimal places. Assign target so total = originalAmount when both rounded to 2 decimals.
6. Add validation: total sums to originalAmount exactly (to 2 decimal places) — otherwise show invalid state (existing behavior kept).
7. Add unit tests for controller logic covering edge cases: negative amounts, cents rounding (e.g., original -0.03 with source -0.02 should produce target -0.01), and parsing invalid input.
8. Manual UI verification with screenshots and user test steps.

Acceptance criteria
- No automatic live updates occur while typing into either amount field.
- Clicking a calculate button sets the adjacent field so the two amounts sum exactly to the original transaction amount (to 2 decimal places).
- Validation shows valid when total equals original, invalid otherwise.
- Unit tests added and passing.

Notes and decisions
- Use decimal arithmetic for currency and round only when assigning to the other field to avoid intermediate rounding surprises.
- Keep UI small and unobtrusive: a compact icon button (e.g., ↔ or a calculator glyph) is preferred unless the user requests textual label.
- Preserve existing styling and validation visuals. Keep the save/cancel flow unchanged.

Testing checklist
- Run all tests in the solution (dotnet test for the repository) when validating changes; all tests currently pass.
- Ensure SplitTransactionControllerTest.NegativeAmounts_Splinter1_Neg50_Splinter2_Neg50Point22_IsValid passes after modifications.
- Run unit tests for the controller.
- Start WPF and manually verify: edit left amount (ensure other does not change), click right-side button -> verify other updated and total equals original. Repeat for opposite side and edge rounding cases.

Dev notes
- Follow AGENTS.md guidance: new or modified controllers should use [AutoRegisterWithIoC(SingleInstance = true)] if applicable, inherit ControllerBase, and use doNotUse backing fields for INotifyPropertyChanged classes. Register types in CompositionRoot if manual registration needed.
- Use decimal arithmetic for currency and round only when assigning the calculated field to avoid intermediate rounding surprises.
- Tests: use MSTest conventions present in the repo; add/modify unit tests under the appropriate test project (BudgetAnalyser.Engine.UnitTest or BudgetAnalyser.Wpf.UnitTest).
- If automatic updates are wired in XAML bindings (two-way with converters), remove converter auto-calculation and move logic to commands on the controller.
- If multiple places implement logic, centralise calculation into controller methods to avoid duplication.
- Validate that SplitTransactionControllerTest.NegativeAmounts_Splinter1_Neg50_Splinter2_Neg50Point22_IsValid passes after changes; include this test in the run-all-tests validation step.
