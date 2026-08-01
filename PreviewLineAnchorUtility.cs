using Markdig.Renderers.Html;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarkupEditor;

/// <summary>
/// Assigns stable <c>id="me-line-*"</c> attributes to block nodes so the preview can scroll and highlight by
/// source line (first block starting on a given line wins).
/// </summary>
internal static class PreviewLineAnchorUtility
{
    /// <summary>
    /// Attaches preview line ids to blocks that participate in HTML rendering, at most one id per source line.
    /// </summary>
    /// <param name="document">The parsed markdown document.</param>
    internal static void AssignPreviewLineAnchors(MarkdownDocument document)
    {
        HashSet<Int32> assignedLines = [];

        foreach (MarkdownObject descendant in document.Descendants())
        {
            if (descendant is not Block block) continue;

            switch (block)
            {
                case MarkdownDocument:
                case LinkReferenceDefinition:
                    continue;
            }

            HtmlAttributes attributes = descendant.GetAttributes();

            if (!String.IsNullOrEmpty(attributes.Id)) continue;

            Int32 line = block.Line;

            if (line < 0) continue;

            if (assignedLines.Contains(line)) continue;

            attributes.Id = "me-line-" + line.ToString(CultureInfo.InvariantCulture);
            assignedLines.Add(line);
        }
    }
}