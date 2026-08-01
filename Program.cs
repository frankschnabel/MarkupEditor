using System;
using System.Windows.Forms;

namespace MarkupEditor;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(String[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        String initialPath = null;
        if (args is { Length: > 0 }) initialPath = args[0];

        MarkupEditor mainForm = new(initialPath);
        Application.Run(mainForm);
    }
}