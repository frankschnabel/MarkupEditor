# MarkupEditor

A small **Windows Forms** editor for **Markdown** with a **live HTML preview**. It targets **.NET 8 (Windows)**. Rendering uses **[Markdig](https://github.com/xoofx/markdig)** (CommonMark-oriented, with GFM-style extensions such as tables, task lists, and strikethrough).

## Features

- Split view: **editor** (left) and **HTML preview** (right)
- **Debounced** automatic preview while typing (optional **Live Preview** in Tools → Settings)
- **File** menu: New, Open, Save, Save As, **Recent documents** (last 10), Exit
- Unsaved-change prompts when closing or switching documents
- UTF-8 open/save; optional **command-line path** to open a file on startup (works with file associations)
- **Preview follows the caret**: scroll and highlight the block for the current line
- Settings remembered between sessions: live preview, word wrap, font size, splitter distance, window layout
- **Help** → Supported Markup summarizes the syntax and policies (including safe preview: raw HTML in the source is not executed)

## Requirements

- **Windows**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows, or **Visual Studio 2022** with the .NET desktop workload

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

The preview follows **CommonMark**-style rules plus enabled extensions (see **Help** in the app). Highlights include headings, block quotes, lists (including task lists), fenced code blocks, pipe/grid tables, links and images (inline and reference-style), footnotes, autolinks, and strikethrough. Raw HTML in the document is **not** interpreted in the preview (escaped for safety).

## Limitations

- Exact behavior and edge cases follow **Markdig**’s pipeline and version.
- Preview uses the WinForms **`WebBrowser`** control (**legacy MSHTML/IE** host surface). Rendering and script behavior reflect that environment.

## Repository layout

| Path | Role |
|------|------|
| `MarkupEditor.sln` | Solution |
| `MarkupEditor.cs` / `MarkupEditor.Designer.cs` | Main form and UI layout |
| `MarkupParser.cs` / `PreviewLineAnchorUtility.cs` | Markdown → HTML (Markdig) and preview line ids |
| `MarkupEditor.Tests/` | Parser smoke tests (MSTest) |
| `Program.cs` | Entry point |
| `Properties/Settings.*` | User-scoped settings |
| `Assets/` | Application icon (see `Assets/ICON_ATTRIBUTION.txt`) |
| `Agents.md` | Maintainer / automation notes (behavior and coding conventions) |

## Contributing

If you change application behavior or add features, update **`Agents.md`** so it stays accurate for tooling and future work.
