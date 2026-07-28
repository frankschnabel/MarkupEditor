# MarkupEditor

A small **Windows Forms** editor for **Markdown** with a **live HTML preview**. It targets **.NET 8 (Windows)**. Rendering uses **[Markdig](https://github.com/xoofx/markdig)** (CommonMark-oriented, with GFM-style extensions such as tables, task lists, and strikethrough).

## Features

- Split view with selectable orientation: **vertical** (editor left, preview right) or **horizontal** (editor top, preview bottom)
- **Debounced** automatic preview while typing, backed by **WebView2** for modern Chromium-based rendering
- **Tools** → **Settings...** (`Ctrl+,`) opens a custom dialog for:
	- Live Preview
	- Word Wrap
	- Horizontal Split
	- Line Numbers
	- Allow Raw HTML in Preview
	- Editor font size
- **File** menu: New, Open, Reload, Save, Save As, **Export as HTML**, **Recent documents** (last 10), Exit
- Unsaved-change prompts when closing, creating a new document, opening another file, or reloading from disk
- External file-change detection: if the open file is modified, renamed, or deleted by another process, the app alerts you and offers to reload when appropriate
- UTF-8 open/save; optional **command-line path** to open a file on startup (works with file associations)
- **Preview follows the caret**: the rendered block for the current source line is scrolled into view and highlighted
- Optional **line-number gutter** for the editor when word wrap is off
- Settings remembered between sessions: live preview, word wrap, split orientation, line numbers, raw HTML policy, font size, splitter distance, and window layout
- **Help** → Supported Markup summarizes the syntax and preview behavior

## Requirements

- **Windows**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows, or **Visual Studio 2022** with the .NET desktop workload
- **WebView2 Runtime** on the target machine for the preview pane (typically already present on Windows 10/11 systems with Microsoft Edge)

## Build and run

```powershell
dotnet build MarkupEditor.sln -c Debug
```

or

```powershell
msbuild MarkupEditor.sln /p:Configuration=Debug
```

Run the app:

```text
bin\Debug\net8.0-windows\MarkupEditor.exe
```

Open a specific file:

```text
bin\Debug\net8.0-windows\MarkupEditor.exe "C:\path\to\document.md"
```

Tests:

```powershell
dotnet test MarkupEditor.sln -c Debug
```

You can also open the solution in Visual Studio and press **F5**.

## Supported markup

The preview follows **CommonMark**-style rules plus enabled extensions (see **Help** in the app). Highlights include headings, block quotes, lists (including task lists), fenced code blocks, pipe/grid tables, links and images (inline and reference-style), footnotes, autolinks, and strikethrough.

By default, raw HTML in the source is **not** interpreted in the preview and is escaped for safety. If you enable **Allow Raw HTML in Preview** in **Tools** → **Settings...**, raw HTML passthrough is enabled for the current session and persisted in user settings.

## Limitations

- Exact behavior and edge cases follow **Markdig**'s pipeline and version.
- The preview depends on the installed **WebView2 Runtime**.
- Line numbers are only shown when **Word Wrap** is disabled; the gutter is hidden while wrapping is enabled.

## Repository layout

| Path | Role |
|------|------|
| `MarkupEditor.sln` | Solution |
| `MarkupEditor.cs` / `MarkupEditor.Designer.cs` | Main form and UI layout |
| `SettingsDialog.cs` / `SettingsDialog.Designer.cs` | Custom modal Settings dialog |
| `LineNumberPanel.cs` | Custom editor gutter for logical line numbers |
| `FindReplaceDialog.cs` / `FindReplaceDialog.Designer.cs` | Find / replace dialog |
| `MarkupParser.cs` / `PreviewLineAnchorUtility.cs` | Markdown → HTML (Markdig) and preview line ids |
| `MarkupEditor.Tests/` | Parser smoke tests (MSTest) |
| `Program.cs` | Entry point |
| `Properties/Settings.*` | User-scoped settings |
| `Assets/` | Application icon (see `Assets/ICON_ATTRIBUTION.txt`) |
| `Agents.md` | Maintainer / automation notes (behavior and coding conventions) |

## Contributing

If you change application behavior or add features, update **`Agents.md`** so it stays accurate for tooling and future work.
