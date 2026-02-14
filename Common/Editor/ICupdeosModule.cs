// Cupdeos Hub — Module Interface
// Every Cupdeos module implements this to register with the Hub.
// (c) Cupdeos / c-huck.com

using UnityEngine;

namespace Cupdeos.Hub
{
    /// <summary>
    /// Interface for Cupdeos module registration.
    /// Implement in your module's Editor assembly to appear in the Hub window.
    /// Discovery is automatic via TypeCache — no manual registration needed.
    /// </summary>
    public interface ICupdeosModule
    {
        /// <summary>Short display name (e.g., "BSM", "CPS").</summary>
        string Abbreviation { get; }

        /// <summary>Full display name (e.g., "Buff Status Effect Manager").</summary>
        string DisplayName { get; }

        /// <summary>Semantic version string (e.g., "1.0.0").</summary>
        string Version { get; }

        /// <summary>One-line description shown in the Hub.</summary>
        string Description { get; }

        /// <summary>Module icon (48x48 recommended). Null = default icon.</summary>
        Texture2D Icon { get; }

        /// <summary>Author or publisher name.</summary>
        string Author { get; }

        /// <summary>Documentation URL (opens in browser).</summary>
        string DocumentationUrl { get; }

        /// <summary>Whether this module has a dedicated settings/editor window.</summary>
        bool HasSettingsWindow { get; }

        /// <summary>Opens the module's settings window. Only called if HasSettingsWindow is true.</summary>
        void OpenSettingsWindow();

        /// <summary>
        /// Optional dependencies on other Cupdeos modules (by abbreviation).
        /// Example: new[] { "GC2 Core" } — purely informational for the Hub.
        /// </summary>
        ICupdeosDependency[] RequiredDependencies { get; }

        /// <summary>
        /// Optional soft dependencies (by abbreviation).
        /// Example: new[] { "BSM", "RS" } — modules that enhance this one.
        /// </summary>
        ICupdeosDependency[] OptionalDependencies { get; }

        /// <summary>
        /// Module health check. Return null/empty if OK, or a warning message
        /// if something needs attention (e.g., missing component in scene).
        /// </summary>
        string GetStatusMessage();
    }

    public interface ICupdeosDependency
    {
        /// <summary>Short display name (e.g., "BSM", "CPS").</summary>
        string Abbreviation { get; }

        /// <summary>Full display name (e.g., "Buff Status Effect Manager").</summary>
        string DisplayName { get; }
    }

    public readonly struct CupdeosDependency : ICupdeosDependency
    {
        public string Abbreviation { get; }
        public string DisplayName { get; }

        public CupdeosDependency(string abbreviation, string displayName = "")
        {
            Abbreviation = abbreviation;
            DisplayName = displayName;
        }
    }
}
