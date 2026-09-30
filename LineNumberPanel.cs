using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MarkupEditor;

/// <summary>
/// A panel that renders logical line numbers aligned to the visible rows of a sibling <see cref="TextBox"/>.
/// Line numbers are only painted when the associated <see cref="TextBox"/> has word wrap disabled.
/// </summary>
internal sealed class LineNumberPanel : Panel
{
    private const Int32 EmGetFirstVisibleLine = 0x00CE;
    private const Int32 EmLineIndex = 0x00BB;
    private const Int32 EmPosFromChar = 0x00D6;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam);

    private readonly TextBox _textBox;

    /// <summary>
    /// Initializes a new <see cref="LineNumberPanel"/> linked to the given editor <see cref="TextBox"/>.
    /// </summary>
    /// <param name="textBox">The editor text box whose line numbers are displayed.</param>
    internal LineNumberPanel(TextBox textBox)
    {
        _textBox = textBox;
        DoubleBuffered = true;
        BackColor = SystemColors.ControlLight;
        ForeColor = SystemColors.GrayText;
        Dock = DockStyle.Left;
        Width = 44;

        textBox.TextChanged += OnTextBoxStateChanged;
        textBox.MouseWheel += OnTextBoxStateChanged;
        textBox.KeyDown += OnTextBoxStateChanged;
        textBox.FontChanged += OnTextBoxStateChanged;
        textBox.Resize += OnTextBoxStateChanged;
    }

    /// <summary>
    /// Handles state changes in the linked text box that require the panel to be repainted.
    /// </summary>
    private void OnTextBoxStateChanged(Object sender, EventArgs e)
    {
        UpdateWidth();
        Invalidate();
    }

    /// <summary>
    /// Recalculates the gutter width when its independent font changes.
    /// </summary>
    /// <param name="e">Font change event data.</param>
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UpdateWidth();
        Invalidate();
    }

    /// <summary>
    /// Adjusts the panel width to accommodate the widest line number at the current font size.
    /// </summary>
    private void UpdateWidth()
    {
        Int32 digits = Math.Max(3, _textBox.Lines.Length.ToString().Length);
        Int32 w = TextRenderer.MeasureText(new String('9', digits), Font).Width + 10;
        if (Width != w) Width = w;
    }

    /// <summary>
    /// Paints line numbers aligned to the visible rows of the linked text box.
    /// Nothing is painted when word wrap is enabled (line positions would be inaccurate).
    /// </summary>
    /// <param name="e">Paint event data.</param>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_textBox.WordWrap || !_textBox.IsHandleCreated) return;

        Int32 firstLine = GetFirstVisibleLine();
        Int32 lineCount = _textBox.Lines.Length;

        if (lineCount == 0) return;

        Point startPos = GetCharPosition(GetLineCharIndex(firstLine));
        Int32 startY = startPos.Y;
        Int32 lineHeight = _textBox.Font.Height;

        if (lineHeight <= 0) return;

        Int32 visibleCount = (Height - startY) / lineHeight + 2;

        for (Int32 i = 0; i < visibleCount; i++)
        {
            Int32 lineIndex = firstLine + i;

            if (lineIndex >= lineCount) break;

            Int32 y = startY + i * lineHeight;
            Rectangle rect = new(0, y, Width - 4, lineHeight);

            TextRenderer.DrawText(e.Graphics, (lineIndex + 1).ToString(),
                Font, rect, ForeColor,
                TextFormatFlags.Right | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
        }
    }

    /// <summary>
    /// Returns the index of the first visible physical line in the linked text box.
    /// </summary>
    private Int32 GetFirstVisibleLine() =>
        (Int32)SendMessage(_textBox.Handle, EmGetFirstVisibleLine, IntPtr.Zero, IntPtr.Zero);

    /// <summary>
    /// Returns the character index of the first character on the given physical line.
    /// </summary>
    /// <param name="lineNumber">The zero-based physical line number.</param>
    private Int32 GetLineCharIndex(Int32 lineNumber) =>
        (Int32)SendMessage(_textBox.Handle, EmLineIndex, lineNumber, IntPtr.Zero);

    /// <summary>
    /// Returns the client-area pixel position of the given character index in the text box.
    /// </summary>
    /// <param name="charIndex">The character index to locate.</param>
    private Point GetCharPosition(Int32 charIndex)
    {
        IntPtr result = SendMessage(_textBox.Handle, EmPosFromChar, charIndex, IntPtr.Zero);
        Int32 value = (Int32)(result.ToInt64() & 0xFFFFFFFFL);

        return new Point((Int16)(value & 0xFFFF), (Int16)((value >> 16) & 0xFFFF));
    }
}