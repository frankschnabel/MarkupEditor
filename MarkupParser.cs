using System.Globalization;
using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;

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
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
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
    private static readonly MarkdownPipeline MarkdownPipelineWithHtml = new MarkdownPipelineBuilder()
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
    internal static string BuildHtmlDocument(string body)
    {
        const string caretScript =
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
    internal static string ConvertMarkupToHtml(string source, bool allowHtml = false)
    {
        string normalized = source.Replace("\r\n", "\n").Replace("\r", "\n");

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
}