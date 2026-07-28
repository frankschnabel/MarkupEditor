using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace MarkupEditor;

/// <summary>
/// Modal find/replace UI for a single multiline <see cref="TextBox"/>.
/// </summary>
internal sealed partial class FindReplaceDialog : Form
{
    private readonly TextBox _targetTextBox;
    private readonly bool _initialFocusOnReplace;

    /// <summary>
    /// Initializes a new instance of the <see cref="FindReplaceDialog"/> class.
    /// </summary>
    /// <param name="targetTextBox">The editor text box to search and modify.</param>
    /// <param name="initialFocusOnReplace">When true, focus moves to the Replace field on load.</param>
    public FindReplaceDialog(TextBox targetTextBox, bool initialFocusOnReplace)
    {
        _targetTextBox = targetTextBox ?? throw new ArgumentNullException(nameof(targetTextBox));
        _initialFocusOnReplace = initialFocusOnReplace;
        InitializeComponent();
    }

    /// <summary>
    /// Moves keyboard focus to the requested field after the window is shown.
    /// </summary>
    private void FindReplaceDialog_Shown(object sender, EventArgs e)
    {
        if (_initialFocusOnReplace)
        {
            _replaceTextBox.SelectAll();
            _replaceTextBox.Focus();
        }
        else
        {
            _findTextBox.SelectAll();
            _findTextBox.Focus();
        }
    }

    /// <summary>
    /// Runs find-next from the current selection, wrapping at the end of the document.
    /// </summary>
    private void _findNextButton_Click(object sender, EventArgs e) => TryFindNext(wrap: true);

    /// <summary>
    /// Replaces the current selection when it matches Find, or finds the next match first.
    /// </summary>
    private void _replaceButton_Click(object sender, EventArgs e)
    {
        if (!ValidateFindText()) return;

        if (SelectionMatchesFind())
        {
            ReplaceCurrentSelection();
            TryFindNext(wrap: true);

            return;
        }

        TryFindNext(wrap: true);
    }

    /// <summary>
    /// Replaces all matches in the target text box.
    /// </summary>
    private void _replaceAllButton_Click(object sender, EventArgs e)
    {
        if (!ValidateFindText()) return;

        string find = _findTextBox.Text;
        string replace = _replaceTextBox.Text;
        bool matchCase = _matchCaseCheckBox.Checked;
        bool wholeWord = _wholeWordCheckBox.Checked;
        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        string text = _targetTextBox.Text;
        StringBuilder result = new(text.Length);
        int last = 0;
        int replaced = 0;

        for (int i = 0; i <= text.Length - find.Length;)
        {
            if (string.Compare(text, i, find, 0, find.Length, comparison) != 0)
            {
                i++;

                continue;
            }

            if (wholeWord && !IsWholeWordMatch(text, i, find.Length))
            {
                i++;

                continue;
            }

            result.Append(text, last, i - last);
            result.Append(replace);
            last = i + find.Length;
            i = last;
            replaced++;
        }

        result.Append(text, last, text.Length - last);

        if (replaced == 0)
        {
            MessageBox.Show(this, "The search text was not found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        _targetTextBox.Text = result.ToString();
        _targetTextBox.Select(0, 0);
        _targetTextBox.Focus();

        MessageBox.Show(this,
            string.Format(CultureInfo.CurrentCulture, "Replaced {0} occurrence(s).", replaced),
            Text,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    private void _closeButton_Click(object sender, EventArgs e) => Close();

    /// <summary>
    /// Returns true when the find field is non-empty; otherwise shows a prompt.
    /// </summary>
    private bool ValidateFindText()
    {
        if (!string.IsNullOrEmpty(_findTextBox.Text)) return true;

        MessageBox.Show(this, "Please enter search text.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        _findTextBox.Focus();

        return false;
    }

    /// <summary>
    /// Returns true when the current selection matches the find string using the chosen options.
    /// </summary>
    private bool SelectionMatchesFind()
    {
        if (!ValidateFindText()) return false;

        string find = _findTextBox.Text;
        int selLen = _targetTextBox.SelectionLength;

        if (selLen != find.Length) return false;

        string slice = _targetTextBox.Text.Substring(_targetTextBox.SelectionStart, selLen);
        bool matchCase = _matchCaseCheckBox.Checked;
        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (!string.Equals(slice, find, comparison)) return false;

        return !_wholeWordCheckBox.Checked || IsWholeWordMatch(_targetTextBox.Text, _targetTextBox.SelectionStart, find.Length);
    }

    /// <summary>
    /// Replaces the current selection with the replace string.
    /// </summary>
    private void ReplaceCurrentSelection()
    {
        int start = _targetTextBox.SelectionStart;
        string replace = _replaceTextBox.Text;

        _targetTextBox.SelectedText = replace;
        _targetTextBox.Select(start + replace.Length, 0);
        _targetTextBox.ScrollToCaret();
    }

    /// <summary>
    /// Selects the next occurrence of the find string, optionally wrapping from the start.
    /// </summary>
    /// <param name="wrap">When true, continues from the beginning if no match is found after the caret.</param>
    /// <returns>True when a match was selected.</returns>
    private void TryFindNext(Boolean wrap)
    {
        if (!ValidateFindText()) return;

        string find = _findTextBox.Text;
        bool matchCase = _matchCaseCheckBox.Checked;
        bool wholeWord = _wholeWordCheckBox.Checked;
        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        string text = _targetTextBox.Text;
        int startFrom = _targetTextBox.SelectionStart + _targetTextBox.SelectionLength;

        int found = FindNextIndex(text, find, startFrom, text.Length, comparison, wholeWord);

        if (found < 0 && wrap && startFrom > 0)
            found = FindNextIndex(text, find, 0, startFrom, comparison, wholeWord);

        if (found < 0)
        {
            MessageBox.Show(this, "The search text was not found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            return;
        }

        _targetTextBox.Select(found, find.Length);
        _targetTextBox.ScrollToCaret();
        _targetTextBox.Focus();
    }

    /// <summary>
    /// Finds the next index of <paramref name="find"/> in <paramref name="text"/> within [<paramref name="rangeStart"/>, <paramref name="rangeEndExclusive"/>).
    /// </summary>
    private static int FindNextIndex(string text, string find, int rangeStart, int rangeEndExclusive,
        StringComparison comparison, bool wholeWord)
    {
        int max = rangeEndExclusive - find.Length;

        for (int i = rangeStart; i <= max; i++)
        {
            if (string.Compare(text, i, find, 0, find.Length, comparison) != 0) continue;

            if (wholeWord && !IsWholeWordMatch(text, i, find.Length)) continue;

            return i;
        }

        return -1;
    }

    /// <summary>
    /// Returns true when the match is bounded by non-word characters (or start/end of string).
    /// </summary>
    private static bool IsWholeWordMatch(string text, int index, int length)
    {
        if (index > 0 && IsWordCharacter(text[index - 1])) return false;

        return index + length >= text.Length || !IsWordCharacter(text[index + length]);
    }

    private static bool IsWordCharacter(char c) => char.IsLetterOrDigit(c) || c == '_';
}