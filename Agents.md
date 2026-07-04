# MarkupEditor Agent Notes

## Overview
MarkupEditor is a lightweight Windows Forms application (.NET 8 for Windows) for editing Markdown and seeing an instant HTML preview.

The app now includes:
- A left editor pane for writing Markdown text.
- A right preview pane rendered as HTML.
- A full menu bar with File, Edit, Tools (with Settings), and Help menus.
- Automatic preview refresh (debounced) as the user types.
- Unsaved-change protection when creating, opening, or closing documents.
- File/status indicators in the status bar.
- User settings (live preview, word wrap, editor font size, splitter distance, window size/position/state) remembered between runs via `Properties/Settings.settings`.

## Supported Markup
Rendering uses **[Markdig](https://github.com/xoofx/markdig)** with a pipeline aligned to **CommonMark** plus **GitHub-Flavored Markdown–style** extensions:

- **Headings:** `#` … `######`
- **Block quotes:** `>` …
- **Lists:** unordered (`-`, `*`, `+`), ordered (`1.`), **task lists** (`- [ ]` / `- [x]`)
- **Thematic breaks:** `---`, `***`, etc. (per CommonMark rules)
- **Fenced code blocks** with optional language identifier
- **Pipe and grid tables**
- **Bold / italic / strikethrough:** `**`, `*`, `~~` (emphasis extras)
- **Inline code:** `` `...` ``
- **Links and images:** inline and **reference-style**; **footnotes** (`[^id]` with definitions)
- **Autolinks** for bare `http`/`https` URLs
- **HTML passthrough policy:** raw HTML in the source is **not** executed in the preview; the pipeline uses Markdig’s **DisableHtml** so angle-bracket markup is treated as text (safe preview).

For full syntax rules, see the [CommonMark spec](https://spec.commonmark.org/) and [GitHub Flavored Markdown](https://github.github.com/gfm/).

## Implementation Details

### Files
- `Program.cs` — entry point.
- `MarkupEditor.cs` — application logic and event wiring.
- `MarkupEditor.Designer.cs` — declarative UI layout.
- `FindReplaceDialog.cs` / `FindReplaceDialog.Designer.cs` — modal find/replace for the editor `TextBox` only.
- `MarkupParser.cs` — static `MarkupParser` class; builds the Markdig pipeline and converts Markdown to an HTML body fragment.
- `PreviewLineAnchorUtility.cs` — assigns `id="me-line-*"` on block nodes for preview scroll/highlight (first block per source line).
- `Properties/Settings.settings` and `Properties/Settings.Designer.cs` — user-scoped application settings (persisted under the user profile).
- `MarkupEditor.Tests/` — MSTest project for parser smoke tests.
- `Assets/app2.ico` — Windows application icon (multi-size build from Noto Emoji; see `Assets/ICON_ATTRIBUTION.txt`).

## Coding Conventions
- **Variable types:** Always declare an **explicit type** on the left (`StringBuilder sb`, `List<string> items`, etc.). **Do not use `var`.** On the right, prefer **target-typed `new()`** when the type is obvious: e.g. `StringBuilder sb = new()`, `MarkupEditor f = new(path)`, `_editorTextBox.Font = new(family, size, style)`. Use a full type name only when there is no left-hand type to infer from (e.g. `Application.Run(new MarkupEditor(path))`, `new object[] { ... }`). This is for new code only. Do not update existing code.
- Remove unused variables, methods, etc.
- Event handlers use the naming convention **`ControlName_Event`**: match the **Designer field name** of the control and the **event** name (e.g. `fileNew_Click`, `_editorTextBox_TextChanged`, `MarkupEditor_FormClosing` for the form). Dynamically created menu items may use a descriptive suffix (e.g. `fileRecentDocumentsEntry_Click`).
- Prefer the BCL keyword **`object`** over **`System.Object`** for handler parameters.
- Always update Agents.md when making a change to the application code.
- Always use a separate file for each class.
- UI layout is built in `MarkupEditor.Designer.cs`; event handlers are wired in `MarkupEditor.cs`.
- All methods must include a header block in the standard Microsoft format.
- Avoid having methods longer than approx. 50 lines.
- Use Expression Body where appropriate.
- **`MarkupEditor.cs`** and **`MarkupParser.cs`** use **`#region` / `#endregion`** to group related methods (form lifecycle, preview sync, file/recent documents, parser lists vs. inline, etc.) for IDE navigation.
- **Designer-generated fields** in `MarkupEditor.Designer.cs` intentionally omit the leading underscore (e.g. `fileMenu`); hand-authored fields in `MarkupEditor.cs` use the `_prefix` convention.
- **`Properties/Resources.Designer.cs`** is auto-generated; the `CA1811` suppression on the parameterless constructor is expected for the StronglyTypedResourceBuilder pattern and should not be removed without a coordinated tooling change.

### Application icon
- `MarkupEditor.csproj` sets `ApplicationIcon` to `app2.ico` (embedded in the `.exe` for Explorer, taskbar, etc.).
- The main form copies that icon for the title bar via `Icon.ExtractAssociatedIcon(Application.ExecutablePath)`.
- The glyph is Apache 2.0–licensed Noto Emoji (`emoji_u1f4c4`); full notice in `Assets/ICON_ATTRIBUTION.txt`.

### Entry Point
- `Program.cs` passes optional command-line arguments to `MarkupEditor` (first argument is treated as a file path to open).

### Main Form
- `MarkupEditor.cs` handles application logic and event wiring:
	- Constructor calls `InitializeComponent()` (from designer) then wires event handlers (all named **`ControlName_Event`** per conventions), loads editor settings from `Settings.Default`, and on `Load` restores the saved window layout when present; if `Program` passes a startup file path, that file is opened instead of the sample document.
	- `FormClosing` runs the unsaved-changes prompt; if the user cancels, the close is aborted. **File → Exit** calls `Close()` so the same path runs (single prompt, no duplicate dialogs). After a successful prompt chain, settings are saved to `Settings.Default` via `Save()`.
	- Document workflow methods (New, Open, Save, SaveAs).
	- Delegates rendering to `MarkupParser` (Markdig).
	- UI behavior (live preview toggle, word wrap, font size controls).
- `MarkupEditor.Designer.cs` builds the full UI declaratively:
	- `MenuStrip` with File, Edit, Tools/Settings, Help menus.
	- Designer Click handlers wire File/Edit/Tools/Help menu commands directly to form methods.
	- `SplitContainer` (left editor, right preview).
	- `TextBox` multiline editor (`Consolas`, 11pt, tabs, no wrap).
	- `WebBrowser` for HTML preview.
	- `StatusStrip` with file path and modified state.
	- `Timer` (300 ms debounce for live preview).
	- Menu items include:
	  - **File**: New, Open, Recent Documents (up to 10 paths, persisted in `Settings.RecentDocuments`), Save, Save As, Exit.
	  - **Edit**: Undo, Redo, Cut, Copy, Paste, **Find** (Ctrl+F) and **Replace** (Ctrl+H) via `FindReplaceDialog` (match case / whole word; affects the editor pane only), Select All.
	  - **Tools**: Render Preview and **Settings** (Live Preview toggle, Word Wrap toggle, font-size controls).
	  - **Help**: Supported Markup and About dialogs.

### Document Workflow
- New/Open operations prompt to save if there are unsaved changes.
- Save supports first-time Save As behavior.
- Open/Save use UTF-8 text file IO.
- Successfully opened files are pushed to a MRU list (max 10, newline-delimited paths in `RecentDocuments` user setting); missing files are removed when open fails or shown disabled in the submenu until dropped on refresh.

### Rendering Pipeline
Implemented with **Markdig** in `MarkupParser` / `PreviewLineAnchorUtility`:

1. `ConvertMarkupToHtml` normalizes line endings, parses the document with the configured pipeline (pipe/grid tables, task lists, autolinks, emphasis extras/strikethrough, footnotes; **DisableHtml** for safe preview).
2. `PreviewLineAnchorUtility.AssignPreviewLineAnchors` sets `id="me-line-{n}"` on the **first** block starting on each source line `n` (0-based), skipping link reference definitions and the document root.
3. `HtmlRenderer` writes the HTML body fragment.
4. `BuildHtmlDocument` wraps the body in a styled HTML document and injects `MarkupSetActiveLine(n)` (clears the previously highlighted element, then finds the nearest `me-line-*` at or above `n` and highlights it with a light-amber background).
5. After each preview update and on caret moves (key/mouse), the host calls `SyncPreviewToCaretLine`: `HtmlElement.ScrollIntoView` plus `WebBrowser.InvokeScript("MarkupSetActiveLine", …)` so the block for the caret line stays visible. Deferred work uses `BeginInvoke` / `HandleCreated` where needed so startup via file association does not throw.

## Build and Run
Environment assumptions:
- Windows
- .NET 8 SDK / Visual Studio build tools

Typical commands:
- `dotnet build MarkupEditor.sln -c Debug` or `msbuild MarkupEditor.sln /p:Configuration=Debug`
- Run `bin\Debug\net8.0-windows\MarkupEditor.exe`
- Tests: `dotnet test MarkupEditor.sln -c Debug`
- Open a file on startup: `bin\Debug\net8.0-windows\MarkupEditor.exe path\to\file.md` (use quotes if the path contains spaces)

## Limitations
- Full spec compliance is delegated to Markdig’s version and enabled extensions; edge cases may differ slightly from a specific CommonMark/GFM reference implementation.
- `WebBrowser` uses the legacy IE-based engine provided by WinForms.

## Extension Ideas
- Replace `WebBrowser` with a modern embedded browser control for better rendering.
- Add keyboard shortcuts (Ctrl+N/Ctrl+O/Ctrl+S).
- Add line numbers.
- Add export to standalone HTML.
- Optional setting to allow raw HTML in preview (currently disabled for safety).
