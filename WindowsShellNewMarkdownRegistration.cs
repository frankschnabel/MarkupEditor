using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MarkupEditor;

/// <summary>
/// Applies or removes the per-user Windows Explorer "New > Markdown Document" integration.
/// </summary>
internal static class WindowsShellNewMarkdownRegistration
{
    private const String ClassesRoot = @"Software\Classes";
    private const String MarkdownExtensionKey = @"Software\Classes\.md";
    private const String MarkdownShellNewKey = @"Software\Classes\.md\ShellNew";
    private const String MarkdownOpenWithProgidsKey = @"Software\Classes\.md\OpenWithProgids";
    private const String ExplorerShellNewCacheKey =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew";
    private const String MarkdownProgId = @"MarkupEditor.MarkdownDocument";
    private const String MarkdownProgIdKey = @"Software\Classes\MarkupEditor.MarkdownDocument";
    private const String MarkdownUserChoiceKey =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.md\UserChoice";
    private const String MarkdownUserChoiceLatestProgIdKey =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.md\UserChoiceLatest\ProgId";
    private const String ManagedFlagName = "MarkupEditorManaged";
    private const String ManagedFlagValue = "1";
    private const UInt32 ShcneAssocChanged = 0x08000000;
    private const UInt32 ShcnfIdList = 0x0000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(UInt32 wEventId, UInt32 uFlags, IntPtr dwItem1, IntPtr dwItem2);

    /// <summary>
    /// Enables or disables the per-user Windows "New > Markdown Document" menu entry.
    /// </summary>
    /// <param name="enabled">Whether integration should be present.</param>
    public static void Apply(Boolean enabled)
    {
        if (enabled)
        {
            EnableRegistration();
            EnsureExplorerShellNewCacheContainsMarkdown();
        }
        else
            DisableRegistration();

        RefreshShellAssociations();
    }

    /// <summary>
    /// Creates required HKCU\Software\Classes keys for shell-new markdown support.
    /// </summary>
    private static void EnableRegistration()
    {
        using RegistryKey classesRoot = Registry.CurrentUser.CreateSubKey(ClassesRoot);

        if (classesRoot == null) throw new InvalidOperationException("Unable to access HKCU\\Software\\Classes.");

        using RegistryKey markdownProgId = classesRoot.CreateSubKey(MarkdownProgId);
        if (markdownProgId == null) throw new InvalidOperationException("Unable to create markdown ProgID key.");

        markdownProgId.SetValue(String.Empty, "Markdown Document", RegistryValueKind.String);
        markdownProgId.SetValue("FriendlyTypeName", "Markdown Document", RegistryValueKind.String);

        using RegistryKey openWithProgids = Registry.CurrentUser.CreateSubKey(MarkdownOpenWithProgidsKey);
        if (openWithProgids == null) throw new InvalidOperationException("Unable to create OpenWithProgids key.");

        openWithProgids.SetValue(MarkdownProgId, String.Empty, RegistryValueKind.String);

        foreach (String shellNewKeyPath in GetManagedShellNewKeyTargets())
            EnsureManagedShellNewKey(shellNewKeyPath);

        foreach (String progId in GetManagedProgIdTargets())
            EnsureFriendlyTypeNameForProgId(progId);
    }

    /// <summary>
    /// Removes keys/values added for shell-new markdown support.
    /// </summary>
    private static void DisableRegistration()
    {
        foreach (String shellNewKeyPath in GetManagedShellNewKeyTargets())
            DeleteManagedShellNewKey(shellNewKeyPath);

        Registry.CurrentUser.DeleteSubKeyTree(MarkdownProgIdKey, throwOnMissingSubKey: false);

        using RegistryKey openWithProgids = Registry.CurrentUser.OpenSubKey(MarkdownOpenWithProgidsKey, writable: true);
        if (openWithProgids == null) return;

        openWithProgids.DeleteValue(MarkdownProgId, throwOnMissingValue: false);

        if (openWithProgids.ValueCount == 0) Registry.CurrentUser.DeleteSubKey(MarkdownOpenWithProgidsKey, throwOnMissingSubKey: false);

        using RegistryKey markdown = Registry.CurrentUser.OpenSubKey(MarkdownExtensionKey, writable: true);
        if (markdown is { SubKeyCount: 0, ValueCount: 0 })
            Registry.CurrentUser.DeleteSubKey(MarkdownExtensionKey, throwOnMissingSubKey: false);
    }

    /// <summary>
    /// Returns the set of HKCU classes ShellNew key paths that should be managed for markdown.
    /// </summary>
    /// <returns>Distinct target key paths for ShellNew registration.</returns>
    private static IEnumerable<String> GetManagedShellNewKeyTargets()
    {
        HashSet<String> targets = new(StringComparer.OrdinalIgnoreCase)
        {
            MarkdownShellNewKey,
            BuildProgIdShellNewKeyPath(MarkdownProgId)
        };

        AddProgIdTarget(targets, GetEffectiveMarkdownProgId());
        AddProgIdTarget(targets, GetUserChoiceMarkdownProgId());

        return targets;
    }

    /// <summary>
    /// Returns the set of ProgIDs for markdown that are candidates for managed metadata updates.
    /// </summary>
    /// <returns>Distinct ProgIDs discovered from static and user/effective mappings.</returns>
    private static IEnumerable<String> GetManagedProgIdTargets()
    {
        HashSet<String> progIds = new(StringComparer.OrdinalIgnoreCase)
        {
            MarkdownProgId
        };

        AddProgId(progIds, GetEffectiveMarkdownProgId());
        AddProgId(progIds, GetUserChoiceMarkdownProgId());

        return progIds;
    }

    /// <summary>
    /// Adds a non-empty ProgID to the supplied set.
    /// </summary>
    /// <param name="progIds">Target set to mutate.</param>
    /// <param name="progId">ProgID candidate.</param>
    private static void AddProgId(ISet<String> progIds, String progId)
    {
        if (String.IsNullOrWhiteSpace(progId)) return;

        progIds.Add(progId);
    }

    /// <summary>
    /// Adds a ProgID-based ShellNew target path when the ProgID is non-empty.
    /// </summary>
    /// <param name="targets">Target set to mutate.</param>
    /// <param name="progId">ProgID to map to a ShellNew path.</param>
    private static void AddProgIdTarget(ISet<String> targets, String progId)
    {
        if (String.IsNullOrWhiteSpace(progId)) return;

        targets.Add(BuildProgIdShellNewKeyPath(progId));
    }

    /// <summary>
    /// Ensures friendly markdown type metadata exists for ProgIDs managed by MarkupEditor.
    /// </summary>
    /// <param name="progId">ProgID to inspect and potentially update.</param>
    private static void EnsureFriendlyTypeNameForProgId(String progId)
    {
        if (!ShouldManageProgIdMetadata(progId)) return;

        String progIdKeyPath = $@"Software\Classes\{progId}";
        using RegistryKey progIdKey = Registry.CurrentUser.CreateSubKey(progIdKeyPath);
        if (progIdKey == null) return;

        String defaultName = progIdKey.GetValue(String.Empty) as String;
        if (String.IsNullOrWhiteSpace(defaultName))
            progIdKey.SetValue(String.Empty, "Markdown Document", RegistryValueKind.String);

        String friendly = progIdKey.GetValue("FriendlyTypeName") as String;
        if (String.IsNullOrWhiteSpace(friendly))
            progIdKey.SetValue("FriendlyTypeName", "Markdown Document", RegistryValueKind.String);
    }

    /// <summary>
    /// Returns whether metadata updates are safe for the provided ProgID.
    /// </summary>
    /// <param name="progId">ProgID to evaluate.</param>
    /// <returns><c>true</c> for ProgIDs owned by MarkupEditor; otherwise <c>false</c>.</returns>
    private static Boolean ShouldManageProgIdMetadata(String progId)
    {
        if (String.Equals(progId, MarkdownProgId, StringComparison.OrdinalIgnoreCase)) return true;

        if (String.Equals(progId, "md_auto_file", StringComparison.OrdinalIgnoreCase)) return true;

        if (progId.StartsWith(@"Applications\", StringComparison.OrdinalIgnoreCase) &&
            progId.EndsWith("MarkupEditor.exe", StringComparison.OrdinalIgnoreCase)) return true;

        return progId.IndexOf("MarkupEditor", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Returns the currently effective ProgID for <c>.md</c>, preferring the user-specific class mapping.
    /// </summary>
    /// <returns>The resolved ProgID, or an empty string when none is present.</returns>
    private static String GetEffectiveMarkdownProgId()
    {
        using RegistryKey markdownUser = Registry.CurrentUser.OpenSubKey(MarkdownExtensionKey);
        String userProgId = markdownUser?.GetValue(String.Empty) as String;

        if (!String.IsNullOrWhiteSpace(userProgId)) return userProgId;

        using RegistryKey markdownMerged = Registry.ClassesRoot.OpenSubKey(@".md");
        return markdownMerged?.GetValue(String.Empty) as String ?? String.Empty;
    }

    /// <summary>
    /// Returns the active user-choice ProgID for <c>.md</c> when one is present.
    /// </summary>
    /// <returns>User-choice ProgID, or an empty string when unavailable.</returns>
    private static String GetUserChoiceMarkdownProgId()
    {
        using RegistryKey userChoice = Registry.CurrentUser.OpenSubKey(MarkdownUserChoiceKey);
        String userChoiceProgId = userChoice?.GetValue("ProgId") as String;
        if (!String.IsNullOrWhiteSpace(userChoiceProgId)) return userChoiceProgId;

        using RegistryKey userChoiceLatestProgId = Registry.CurrentUser.OpenSubKey(MarkdownUserChoiceLatestProgIdKey);
        return userChoiceLatestProgId?.GetValue("ProgId") as String ?? String.Empty;
    }

    /// <summary>
    /// Builds the HKCU classes key path for a ProgID shell-new subkey.
    /// </summary>
    /// <param name="progId">The ProgID to target.</param>
    /// <returns>The corresponding <c>Software\Classes\...\ShellNew</c> path.</returns>
    private static String BuildProgIdShellNewKeyPath(String progId) => $@"Software\Classes\{progId}\ShellNew";

    /// <summary>
    /// Ensures a shell-new key exists with a managed marker and a NullFile template value.
    /// </summary>
    /// <param name="shellNewKeyPath">The absolute HKCU software-classes path to ShellNew.</param>
    private static void EnsureManagedShellNewKey(String shellNewKeyPath)
    {
        using RegistryKey shellNew = Registry.CurrentUser.CreateSubKey(shellNewKeyPath);
        if (shellNew == null) throw new InvalidOperationException($"Unable to create {shellNewKeyPath} key.");

        shellNew.SetValue("NullFile", String.Empty, RegistryValueKind.String);
        shellNew.SetValue("ItemName", "Markdown Document", RegistryValueKind.String);
        shellNew.SetValue(ManagedFlagName, ManagedFlagValue, RegistryValueKind.String);
    }

    /// <summary>
    /// Deletes a shell-new key only when it was previously created by this application.
    /// </summary>
    /// <param name="shellNewKeyPath">The absolute HKCU software-classes path to ShellNew.</param>
    private static void DeleteManagedShellNewKey(String shellNewKeyPath)
    {
        using RegistryKey shellNew = Registry.CurrentUser.OpenSubKey(shellNewKeyPath, writable: true);
        if (shellNew == null) return;

        String managed = shellNew.GetValue(ManagedFlagName) as String;
        if (!String.Equals(managed, ManagedFlagValue, StringComparison.Ordinal)) return;

        Registry.CurrentUser.DeleteSubKeyTree(shellNewKeyPath, throwOnMissingSubKey: false);
    }

    /// <summary>
    /// Ensures Explorer's ShellNew class cache includes <c>.md</c> so the New submenu can surface markdown entries.
    /// </summary>
    private static void EnsureExplorerShellNewCacheContainsMarkdown()
    {
        using RegistryKey shellNewCache = Registry.CurrentUser.CreateSubKey(ExplorerShellNewCacheKey);
        if (shellNewCache == null) return;

        String[] classes = shellNewCache.GetValue("Classes") as String[] ?? Array.Empty<String>();
        foreach (String entry in classes)
        {
            if (String.Equals(entry, ".md", StringComparison.OrdinalIgnoreCase)) return;
        }

        String[] updated = new String[classes.Length + 1];
        Array.Copy(classes, updated, classes.Length);
        updated[classes.Length] = ".md";
        shellNewCache.SetValue("Classes", updated, RegistryValueKind.MultiString);
    }

    /// <summary>
    /// Asks Explorer to refresh cached file-type/shell association state.
    /// </summary>
    private static void RefreshShellAssociations() =>
        SHChangeNotify(ShcneAssocChanged, ShcnfIdList, IntPtr.Zero, IntPtr.Zero);
}
