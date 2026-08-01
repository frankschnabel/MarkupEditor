using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MarkupEditor.Tests;

[TestClass]
public sealed class MarkupParserTests
{
    /// <summary>
    /// Headings through level six render as HTML headings.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_Headings_AllLevels()
    {
        String html = MarkupParser.ConvertMarkupToHtml("###### Tiny\n");

        StringAssert.Contains(html, "<h6");
    }

    /// <summary>
    /// GFM pipe tables render a table element.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_PipeTable_ContainsTableTag()
    {
        const String md = "| a | b |\n|---|---|\n| 1 | 2 |\n";
        String html = MarkupParser.ConvertMarkupToHtml(md);

        StringAssert.Contains(html, "<table");
    }

    /// <summary>
    /// Fenced code blocks emit pre/code with optional language class.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_FencedCode_ContainsPre()
    {
        const String md = "```csharp\nint x = 1;\n```\n";
        String html = MarkupParser.ConvertMarkupToHtml(md);

        StringAssert.Contains(html, "<pre");
        StringAssert.Contains(html, "<code");
    }

    /// <summary>
    /// Strikethrough from emphasis extras renders a del element.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_Strikethrough_ContainsDel()
    {
        String html = MarkupParser.ConvertMarkupToHtml("~~gone~~\n");

        StringAssert.Contains(html, "<del");
    }

    /// <summary>
    /// Autolink extension turns bare URLs into links.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_AutoLink_ContainsHref()
    {
        String html = MarkupParser.ConvertMarkupToHtml("Visit https://example.com today.\n");

        StringAssert.Contains(html, "href=\"https://example.com\"");
    }

    /// <summary>
    /// Raw HTML in source is not interpreted as markup when DisableHtml is enabled.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_RawHtml_IsEscapedOrStripped()
    {
        String html = MarkupParser.ConvertMarkupToHtml("<script>alert(1)</script>\n");

        Assert.IsFalse(html.Contains("<script>alert"));
    }

    /// <summary>
    /// Reference-style links resolve in the body output.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_ReferenceLink_ContainsAnchor()
    {
        const String md = "[ref][id]\n\n[id]: https://example.com\n";
        String html = MarkupParser.ConvertMarkupToHtml(md);

        StringAssert.Contains(html, "href=\"https://example.com\"");
    }

    /// <summary>
    /// Images use img with src from reference definition.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_ImageReference_ContainsImg()
    {
        const String md = "![alt][pic]\n\n[pic]: https://example.com/x.png\n";
        String html = MarkupParser.ConvertMarkupToHtml(md);

        StringAssert.Contains(html, "<img");
        StringAssert.Contains(html, "src=\"https://example.com/x.png\"");
    }

    /// <summary>
    /// Block quotes render blockquote elements.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_BlockQuote_ContainsBlockquote()
    {
        String html = MarkupParser.ConvertMarkupToHtml("> quoted\n");

        StringAssert.Contains(html, "<blockquote");
    }

    /// <summary>
    /// Task list items render checkbox inputs.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_TaskList_ContainsCheckbox()
    {
        String html = MarkupParser.ConvertMarkupToHtml("- [ ] todo\n");

        StringAssert.Contains(html, "checkbox");
    }

    /// <summary>
    /// First block on a line receives a stable preview line id for caret sync.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToHtml_PreviewLineId_OnFirstBlock()
    {
        String html = MarkupParser.ConvertMarkupToHtml("# Title\n");

        StringAssert.Contains(html, "id=\"me-line-0\"");
    }

    /// <summary>
    /// TeX export emits a full document shell and basic inline formatting.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToLatexDocument_BasicFormatting()
    {
        const String md = "# Heading\n\nSome **bold** and [link](https://example.com).\n";
        String tex = MarkupParser.ConvertMarkupToLatexDocument(md);

        StringAssert.Contains(tex, "\\documentclass{article}");
        StringAssert.Contains(tex, "\\section{Heading}");
        StringAssert.Contains(tex, "\\textbf{bold}");
        StringAssert.Contains(tex, "\\href{https://example.com}{link}");
        StringAssert.Contains(tex, "\\end{document}");
    }

    /// <summary>
    /// TeX export escapes special characters that are significant to LaTeX.
    /// </summary>
    [TestMethod]
    public void ConvertMarkupToLatexDocument_EscapesSpecialCharacters()
    {
        String tex = MarkupParser.ConvertMarkupToLatexDocument("price is 100% & tax\n");

        StringAssert.Contains(tex, "100\\% \\& tax");
    }
}