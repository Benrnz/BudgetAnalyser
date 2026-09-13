# UX Redesign Plan: Material Design

## Goal

Re-skin the BudgetAnalyser WPF app with a Material Design visual language while
preserving its current ethos of simplicity and ease of use, and without
requiring changes to `BudgetAnalyser.Engine`.

## Current state

- WPF app targeting `net10.0-windows` (`BudgetAnalyser.Wpf.csproj`).
- Hand-rolled XAML styles under `BudgetAnalyser/UI/Style/*` (a single "OilLight"
  theme, no third-party UI toolkit).
- Shared behaviors/converters in the `Rees.Wpf` project.
- Controllers (MVVM-ish) act as `DataContext` for views; no changes needed
  here.
- Navigation is a tab strip of `ToggleButton`s (`MainMenuUserControl.xaml`)
  that toggles `Visibility` across five sections: Dashboard, Transactions,
  Ledger Book, Budget, Reports.
- Modal-style interactions use a reusable overlay (`ShellDialog/ShellDialogView.xaml`)
  per tab, not real dialog windows.
- Dashboard uses a custom "tile" style (`UI/Style/ModernTiles.xaml`).

## Constraints

- **Engine is off-limits**: all Controllers, commands, and data contracts stay
  as-is. This is a View/Style/Template layer change only, scoped to the
  `BudgetAnalyser` (WPF) and `Rees.Wpf` projects.
- **Keep it simple**: apply Material's visual language (color, elevation,
  typography, motion, spacing) to the *existing* interaction model
  (tab strip + single content area + modal overlay). No navigation drawer,
  no bottom sheets, no gesture-heavy patterns.

## Recommended approach

Adopt [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
rather than hand-rolling Material tokens. It's mature, actively maintained,
targets modern .NET WPF, and provides a `ResourceDictionary`-based
`BundledTheme` (Light/Dark, primary/secondary palette) plus ready styles for
buttons, cards, text fields, dialogs, and snackbars. It slots into the
existing `App.xaml` merged-dictionary pattern, allowing dictionaries under
`UI/Style/*` to be replaced incrementally rather than rewriting every control
by hand.

## Phased plan

### Phase 0 — Foundation (low risk, no visible behavior change)

- Add the MaterialDesign NuGet package(s) to `BudgetAnalyser.Wpf.csproj`.
- Add a `BundledTheme` to `App.xaml`, pick a primary/secondary palette
  (the app's current teal/steel-blue as primary is a reasonable anchor).
- Map existing semantic brush keys (`Brush.MainBackground`,
  `Brush.DialogBackground`, `Brush.ControlBorder`, etc. in `Colours.xaml`)
  onto the new Material color resources, so controls already referencing
  those keys pick up the new palette without template rewrites yet.
- Verify the app still builds/runs unchanged visually except color.

### Phase 1 — Shell & navigation chrome

- Replace the custom `TabButtonStyle` `ToggleButton`s in
  `MainMenuUserControl.xaml` with a Material top app bar look: flat,
  underline/pill indicator for the active tab, ripple on press (built into
  the toolkit). Keep it a tab strip, not a drawer — matches the current
  mental model.
- `Shell.xaml`'s window chrome gets a Material `Card`-style elevation on the
  content area; keep the same `ContentPresenter`/`Visibility`-toggle
  structure (no change to how tabs swap content).

### Phase 2 — Core controls library

- Swap `Buttons.xaml`, `TextBox.xaml`, `ComboBox.xaml`, `ListBox.xaml`,
  `ListView.xaml`, `DatePicker.xaml`, `Expander.xaml`, `RadioButton.xaml`,
  `ScrollBar.xaml`/`ScrollViewer.xaml` for MaterialDesign equivalents
  (`MaterialDesignOutlinedTextBox`, `MaterialDesignRaisedButton`/
  `MaterialDesignFlatButton`, filled/outlined combo boxes, Material data
  grids where used). This is mechanical: change `Style="{StaticResource ...}"`
  references or base new styles `BasedOn` the toolkit styles, screen by
  screen, so each screen stays shippable mid-migration.
- Retire the now-redundant custom style files once nothing references them.

### Phase 3 — Dialog and dashboard patterns

- `ShellDialogView.xaml`: replace the manual grey-rectangle + bordered box +
  `DropShadowEffect` with a proper Material `Card`/`DialogHost`-style
  elevation (`MaterialDesignShadowDepth`), same modal-overlay behavior, same
  button placement/commands.
- `ModernTiles.xaml` (Dashboard widgets): re-skin as Material `Card`s with
  elevation-on-hover instead of the current border-color VisualState
  animation — same tile content/commands, just a Material surface treatment.

### Phase 4 — Data-heavy screens (Transactions, Ledger Book, Budget, Reports)

These are the highest-risk screens for regressions since they're data-dense
grids/lists. Apply Material `DataGrid`/`ListView` styling, the typography
scale (`MaterialDesignHeadline*`, `MaterialDesignBody*` for
`TextBlock.Heading1` etc. in `TextBlock.xaml`), and consistent 8dp-based
spacing. Do this screen-by-screen, testing each against real data before
moving to the next.

### Phase 5 — Polish

- Optional light/dark theme toggle (near-free once Phase 0 is done) — a
  nice-to-have given today's single "OilLight" theme.
- Consistent iconography: swap the custom vector `UI/Assets/*Image.xaml`
  glyphs for `PackIcon` (Material icon font) where a like-for-like icon
  exists; keep custom ones (logos: Visa/Amex/Mastercard) as-is.
- Motion: subtle, toolkit-default transitions only (ripple, elevation),
  nothing that adds visual complexity.

## What explicitly does not change

- `BudgetAnalyser.Engine`, `BudgetAnalyser.Encryption`, all Controllers/
  Services and their public surface.
- Navigation model (tab strip + one visible section + overlay dialog).
- Any `x:Class` code-behind logic beyond template/style bindings.
- Test projects (`*.XUnit3`) — no UI logic changes expected to break them,
  but re-run `BudgetAnalyser.Wpf.XUnit3` after each phase as a safety net.

## Risks

- WPF has no live design-time hot reload as smooth as web, so Phase 4
  (data grids) benefits from testing with the real `TestData` sample files,
  not just Blend design-time data.
- Toolkit theme resource names will collide/shadow some current keys in
  `Colours.xaml` — Phase 0 needs a careful audit rather than a blind merge.
