using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkupEditor;

/// <summary>
/// Provides static methods for converting Markdown (CommonMark + selected GFM-style extensions) to an HTML document.
/// </summary>
internal static class MarkupParser
{
    #region Constants and HTML document shell

    /// <summary>
    /// Pipeline: pipe/grid tables, task lists, autolinks, strikethrough, footnotes; raw HTML in source is escaped (not executed).
    /// </summary>
    private static readonly MarkdownPipeline MarkdownPipeline =
        new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseGridTables()
            .UseTaskLists()
            .UseAutoLinks()
            .UseEmphasisExtras()
            .UseFootnotes()
            .DisableHtml()
            .Build();

    /// <summary>
    /// Pipeline with raw HTML passthrough enabled (not safe for untrusted input).
    /// </summary>
    private static readonly MarkdownPipeline MarkdownPipelineWithHtml =
        new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseGridTables()
            .UseTaskLists()
            .UseAutoLinks()
            .UseEmphasisExtras()
            .UseFootnotes()
            .Build();

    /// <summary>
    /// Wraps an HTML body fragment in a complete, styled HTML document (includes script for caret-line highlight).
    /// </summary>
    /// <param name="body">The inner HTML body content.</param>
    /// <returns>A complete HTML document string ready for display in a browser control.</returns>
    internal static String BuildHtmlDocument(String body)
    {
        const String caretScript =
            "<script type=\"text/javascript\">" +
            "var mePreviewLastLineEl=null;" +
            "function MarkupSetActiveLine(n){" +
            "if(mePreviewLastLineEl){mePreviewLastLineEl.style.backgroundColor=\"\";mePreviewLastLineEl.style.borderRadius=\"\";}" +
            "var t=null;" +
            "for(var i=n;i>=0;i--){" +
            "t=document.getElementById(\"me-line-\"+i);" +
            "if(t)break;" +
            "}" +
            "if(t){t.scrollIntoView(true);t.style.backgroundColor=\"#fff3cd\";t.style.borderRadius=\"4px\";mePreviewLastLineEl=t;}" +
            "else{mePreviewLastLineEl=null;}" +
            "}</script>";

        return "<!doctype html>\n" +
               "<html><head><meta charset=\"utf-8\">\n" +
               "<style>" +
               "body{font-family:Segoe UI,Tahoma,sans-serif;margin:18px;color:#222;line-height:1.5;}" +
               "h1,h2,h3,h4,h5,h6{margin:0.8em 0 0.4em;}" +
               "p{margin:0 0 0.8em;}" +
               "ul,ol{margin:0 0 0.8em 1.4em;padding:0;}" +
               "li{margin:0 0 0.2em;}" +
               "li.task-list-item{list-style:none;margin-left:-1.2em;}" +
               "code{font-family:Consolas,monospace;background:#f3f3f3;padding:2px 4px;border-radius:3px;}" +
               "pre{font-family:Consolas,monospace;background:#f3f3f3;padding:10px;border-radius:4px;overflow-x:auto;}" +
               "pre code{background:transparent;padding:0;}" +
               "a{color:#0b63ce;}" +
               "hr{border:none;border-top:1px solid #ccc;margin:1.2em 0;}" +
               "blockquote{border-left:4px solid #ddd;margin:0 0 1em;padding-left:1em;color:#444;}" +
               "table{border-collapse:collapse;margin:0 0 1em;}" +
               "th,td{border:1px solid #ccc;padding:4px 8px;}" +
               "th{background:#f5f5f5;}" +
               "img{max-width:100%;height:auto;}" +
               "del{text-decoration:line-through;}" +
               ".footnotes{font-size:0.92em;border-top:1px solid #ddd;margin-top:2em;padding-top:0.8em;}" +
               "</style>\n" +
               "</head><body>" +
               body +
               caretScript +
               "</body></html>";
    }

    #endregion

    #region Markup to HTML body

    /// <summary>
    /// Converts a Markdown source string into an HTML body fragment using Markdig (CommonMark + GFM-oriented extensions).
    /// </summary>
    /// <param name="source">The raw markup text to convert.</param>
    /// <param name="allowHtml"><see langword="true"/> to render raw HTML passthrough; <see langword="false"/> (default) to escape it.</param>
    /// <returns>An HTML string representing the formatted document body.</returns>
    internal static String ConvertMarkupToHtml(String source, Boolean allowHtml = false)
    {
        String normalized = source.Replace("\r\n", "\n").Replace("\r", "\n");

        if (normalized.Length == 0) return "<p id=\"me-line-0\"></p>";

        MarkdownPipeline pipeline = allowHtml ? MarkdownPipelineWithHtml : MarkdownPipeline;
        MarkdownDocument document = Markdown.Parse(normalized, pipeline);
        PreviewLineAnchorUtility.AssignPreviewLineAnchors(document);

        StringWriter writer = new(CultureInfo.InvariantCulture);
        HtmlRenderer renderer = new(writer);
        pipeline.Setup(renderer);
        renderer.Render(document);

        return writer.ToString();
    }

    #endregion

    #region Markup to LaTeX document

    /// <summary>
    /// Converts Markdown source into a standalone LaTeX document.
    /// </summary>
    /// <param name="source">The raw markup text to convert.</param>
    /// <returns>A complete UTF-8 LaTeX document string.</returns>
    internal static String ConvertMarkupToLatexDocument(String source)
    {
        String body = ConvertMarkupToLatexBody(source ?? String.Empty);

        return "\\documentclass{article}\n" +
               "\\usepackage[utf8]{inputenc}\n" +
               "\\usepackage[T1]{fontenc}\n" +
               "\\usepackage{hyperref}\n" +
               "\\usepackage{graphicx}\n" +
               "\\usepackage[normalem]{ulem}\n" +
               "\\begin{document}\n\n" +
               body +
               "\n\\end{document}\n";
    }

    /// <summary>
    /// Converts Markdown source to a LaTeX body fragment.
    /// </summary>
    /// <param name="source">The raw markup text.</param>
    /// <returns>A LaTeX body fragment.</returns>
    private static String ConvertMarkupToLatexBody(String source)
    {
        String normalized = source.Replace("\r\n", "\n").Replace("\r", "\n");
        String[] lines = normalized.Split('\n');
        StringBuilder builder = new();
        Boolean inCodeBlock = false;
        Boolean inItemize = false;
        Boolean inEnumerate = false;
        Boolean inQuote = false;

        foreach (String line in lines)
            AppendLatexLine(builder, line, ref inCodeBlock, ref inItemize, ref inEnumerate, ref inQuote);

        CloseLatexBlock(builder, ref inCodeBlock, ref inItemize, ref inEnumerate, ref inQuote);

        return builder.ToString();
    }

    /// <summary>
    /// Appends one source line to the LaTeX output, tracking open block-level environments.
    /// </summary>
    private static void AppendLatexLine(StringBuilder builder, String line, ref Boolean inCodeBlock,
        ref Boolean inItemize, ref Boolean inEnumerate, ref Boolean inQuote)
    {
        if (line.StartsWith("```", StringComparison.Ordinal))
        {
            ToggleCodeFence(builder, ref inCodeBlock, ref inItemize, ref inEnumerate, ref inQuote);

            return;
        }

        if (inCodeBlock)
        {
            builder.AppendLine(line);

            return;
        }

        if (String.IsNullOrWhiteSpace(line))
        {
            CloseLists(builder, ref inItemize, ref inEnumerate);
            CloseQuote(builder, ref inQuote);
            builder.AppendLine();

            return;
        }

        if (TryAppendHeading(builder, line, ref inItemize, ref inEnumerate, ref inQuote)) return;
        if (TryAppendQuoteLine(builder, line, ref inItemize, ref inEnumerate, ref inQuote)) return;
        if (TryAppendUnorderedItem(builder, line, ref inItemize, ref inEnumerate)) return;
        if (TryAppendOrderedItem(builder, line, ref inItemize, ref inEnumerate)) return;

        CloseLists(builder, ref inItemize, ref inEnumerate);
        CloseQuote(builder, ref inQuote);
        builder.AppendLine(ConvertInlineToLatex(line));
        builder.AppendLine();
    }

    /// <summary>
    /// Opens or closes a LaTeX verbatim environment for fenced code blocks.
    /// </summary>
    private static void ToggleCodeFence(StringBuilder builder, ref Boolean inCodeBlock, ref Boolean inItemize,
        ref Boolean inEnumerate, ref Boolean inQuote)
    {
        if (!inCodeBlock)
        {
            CloseLists(builder, ref inItemize, ref inEnumerate);
            CloseQuote(builder, ref inQuote);
            builder.AppendLine("\\begin{verbatim}");
            inCodeBlock = true;

            return;
        }

        builder.AppendLine("\\end{verbatim}");
        builder.AppendLine();
        inCodeBlock = false;
    }

    /// <summary>
    /// Appends a heading command when the line starts with Markdown heading markers.
    /// </summary>
    private static Boolean TryAppendHeading(StringBuilder builder, String line, ref Boolean inItemize,
        ref Boolean inEnumerate, ref Boolean inQuote)
    {
        Match match = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
        if (!match.Success) return false;

        CloseLists(builder, ref inItemize, ref inEnumerate);
        CloseQuote(builder, ref inQuote);

        Int32 level = match.Groups[1].Value.Length;
        String title = ConvertInlineToLatex(match.Groups[2].Value.Trim());
        builder.AppendLine($"\\{GetHeadingCommand(level)}{{{title}}}");
        builder.AppendLine();

        return true;
    }

    /// <summary>
    /// Appends a quote line when the source line starts with the Markdown quote marker.
    /// </summary>
    private static Boolean TryAppendQuoteLine(StringBuilder builder, String line, ref Boolean inItemize,
        ref Boolean inEnumerate, ref Boolean inQuote)
    {
        Match quoteMatch = Regex.Match(line, @"^>\s?(.*)$");
        if (!quoteMatch.Success)
        {
            CloseQuote(builder, ref inQuote);

            return false;
        }

        CloseLists(builder, ref inItemize, ref inEnumerate);

        if (!inQuote)
        {
            builder.AppendLine("\\begin{quote}");
            inQuote = true;
        }

        String quoteText = ConvertInlineToLatex(quoteMatch.Groups[1].Value);
        builder.AppendLine(quoteText + "\\\\");

        return true;
    }

    /// <summary>
    /// Appends an unordered list item when the line starts with a Markdown bullet marker.
    /// </summary>
    private static Boolean TryAppendUnorderedItem(StringBuilder builder, String line, ref Boolean inItemize,
        ref Boolean inEnumerate)
    {
        Match itemMatch = Regex.Match(line, @"^[-*+]\s+(.+)$");
        if (!itemMatch.Success) return false;

        if (inEnumerate)
        {
            builder.AppendLine("\\end{enumerate}");
            inEnumerate = false;
        }

        if (!inItemize)
        {
            builder.AppendLine("\\begin{itemize}");
            inItemize = true;
        }

        builder.AppendLine($"\\item {ConvertInlineToLatex(itemMatch.Groups[1].Value)}");

        return true;
    }

    /// <summary>
    /// Appends an ordered list item when the line starts with a numeric Markdown list marker.
    /// </summary>
    private static Boolean TryAppendOrderedItem(StringBuilder builder, String line, ref Boolean inItemize,
        ref Boolean inEnumerate)
    {
        Match itemMatch = Regex.Match(line, @"^\d+\.\s+(.+)$");
        if (!itemMatch.Success) return false;

        if (inItemize)
        {
            builder.AppendLine("\\end{itemize}");
            inItemize = false;
        }

        if (!inEnumerate)
        {
            builder.AppendLine("\\begin{enumerate}");
            inEnumerate = true;
        }

        builder.AppendLine($"\\item {ConvertInlineToLatex(itemMatch.Groups[1].Value)}");

        return true;
    }

    /// <summary>
    /// Converts inline Markdown emphasis, links, images, and code to LaTeX-safe text.
    /// </summary>
    private static String ConvertInlineToLatex(String text)
    {
        List<String> codeParts = [];
        String working = Regex.Replace(text, @"`([^`]+)`", match => StoreCodePart(match, codeParts));

        working = EscapeLatex(working);
        working = Regex.Replace(working, @"!\[([^\]]*)\]\(([^)]+)\)", @"\\includegraphics{$2}");
        working = Regex.Replace(working, @"\[([^\]]+)\]\(([^)]+)\)", @"\\href{$2}{$1}");
        working = Regex.Replace(working, @"~~(.+?)~~", @"\\sout{$1}");
        working = Regex.Replace(working, @"\*\*(.+?)\*\*", @"\\textbf{$1}");
        working = Regex.Replace(working, @"\*(.+?)\*", @"\\emph{$1}");

        for (Int32 i = 0; i < codeParts.Count; i++)
            working = working.Replace($"@@CODE{i}@@", $"\\texttt{{{EscapeLatex(codeParts[i])}}}");

        return working;
    }

    /// <summary>
    /// Stores inline code text and returns a stable placeholder token.
    /// </summary>
    private static String StoreCodePart(Match match, IList<String> codeParts)
    {
        Int32 index = codeParts.Count;
        codeParts.Add(match.Groups[1].Value);

        return $"@@CODE{index}@@";
    }

    /// <summary>
    /// Escapes LaTeX special characters in plain text.
    /// </summary>
    private static String EscapeLatex(String value)
    {
        StringBuilder builder = new();

        foreach (Char c in value)
            builder.Append(c switch
            {
                '\\' => "\\textbackslash{}",
                '{' => "\\{",
                '}' => "\\}",
                '$' => "\\$",
                '&' => "\\&",
                '#' => "\\#",
                '_' => "\\_",
                '%' => "\\%",
                '^' => "\\textasciicircum{}",
                '~' => "\\textasciitilde{}",
                _ => c.ToString()
            });

        return builder.ToString();
    }

    /// <summary>
    /// Returns the LaTeX heading command name for a Markdown heading level.
    /// </summary>
    private static String GetHeadingCommand(Int32 level) =>
        level switch
        {
            1 => "section",
            2 => "subsection",
            3 => "subsubsection",
            4 => "paragraph",
            5 => "subparagraph",
            _ => "textbf"
        };

    /// <summary>
    /// Closes all open block environments at end-of-document.
    /// </summary>
    private static void CloseLatexBlock(StringBuilder builder, ref Boolean inCodeBlock, ref Boolean inItemize,
        ref Boolean inEnumerate, ref Boolean inQuote)
    {
        if (inCodeBlock)
        {
            builder.AppendLine("\\end{verbatim}");
            inCodeBlock = false;
        }

        CloseLists(builder, ref inItemize, ref inEnumerate);
        CloseQuote(builder, ref inQuote);
    }

    /// <summary>
    /// Closes any open list environments.
    /// </summary>
    private static void CloseLists(StringBuilder builder, ref Boolean inItemize, ref Boolean inEnumerate)
    {
        if (inItemize)
        {
            builder.AppendLine("\\end{itemize}");
            inItemize = false;
        }

        if (inEnumerate)
        {
            builder.AppendLine("\\end{enumerate}");
            inEnumerate = false;
        }
    }

    /// <summary>
    /// Closes an open quote environment.
    /// </summary>
    private static void CloseQuote(StringBuilder builder, ref Boolean inQuote)
    {
        if (!inQuote) return;

        builder.AppendLine("\\end{quote}");
        inQuote = false;
    }

    #endregion
}