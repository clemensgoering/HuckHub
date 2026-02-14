// Cupdeos Hub — Central Module Overview
// Discovers all installed Cupdeos modules via TypeCache.
// (c) Cupdeos / c-huck.com

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cupdeos.Hub
{
    public class CupdeosHubWindow : EditorWindow
    {
        // ═══════════════════════════════════════════════════════════════════
        // CONSTANTS
        // ═══════════════════════════════════════════════════════════════════

        private const string WINDOW_TITLE = "Cupdeos Hub";
        private const string MENU_PATH = "Window/Cupdeos/Hub";
        private const float MODULE_CARD_HEIGHT = 90f;
        private const float SIDEBAR_WIDTH = 200f;
        private const float PADDING = 8f;

        // ═══════════════════════════════════════════════════════════════════
        // STATE
        // ═══════════════════════════════════════════════════════════════════

        private List<ICupdeosModule> m_Modules = new List<ICupdeosModule>();
        private ICupdeosModule m_SelectedModule;
        private Vector2 m_SidebarScroll;
        private Vector2 m_DetailScroll;
        private bool m_Initialized;
        private bool m_ShowWelcome = true;

        // ═══════════════════════════════════════════════════════════════════
        // STYLES (lazy init)
        // ═══════════════════════════════════════════════════════════════════

        private GUIStyle m_HeaderStyle;
        private GUIStyle m_SubHeaderStyle;
        private GUIStyle m_CardStyle;
        private GUIStyle m_SelectedCardStyle;
        private GUIStyle m_VersionBadgeStyle;
        private GUIStyle m_StatusOkStyle;
        private GUIStyle m_StatusWarnStyle;
        private GUIStyle m_DescriptionBoxStyle;
        private GUIStyle m_BodyTextStyle;
        private GUIStyle m_WelcomeCardStyle;

        // ═══════════════════════════════════════════════════════════════════
        // MENU
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem(MENU_PATH, priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<CupdeosHubWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(520, 340);
            window.Show();
        }

        // ═══════════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═══════════════════════════════════════════════════════════════════

        private void OnEnable()
        {
            RefreshModules();
        }

        private void OnFocus()
        {
            RefreshModules();
        }

        // ═══════════════════════════════════════════════════════════════════
        // MODULE DISCOVERY
        // ═══════════════════════════════════════════════════════════════════

        private void RefreshModules()
        {
            m_Modules.Clear();

            var types = TypeCache.GetTypesDerivedFrom<ICupdeosModule>();
            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;

                try
                {
                    var instance = (ICupdeosModule)Activator.CreateInstance(type);
                    m_Modules.Add(instance);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Cupdeos Hub] Failed to instantiate module {type.Name}: {e.Message}");
                }
            }

            m_Modules = m_Modules.OrderBy(m => m.DisplayName).ToList();

            if (m_SelectedModule != null)
            {
                m_SelectedModule = m_Modules.FirstOrDefault(
                    m => m.Abbreviation == m_SelectedModule.Abbreviation);
            }

            m_Initialized = true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // GUI
        // ═══════════════════════════════════════════════════════════════════

        private void OnGUI()
        {
            InitStyles();

            if (!m_Initialized)
                RefreshModules();

            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            {
                DrawSidebar();
                DrawDetailPanel();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ─────────────────────────────────────────────────────────────────
        // Toolbar
        // ─────────────────────────────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label(WINDOW_TITLE, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                GUILayout.Label($"{m_Modules.Count} module(s) installed",
                    EditorStyles.miniLabel);

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,
                    GUILayout.Width(60)))
                {
                    RefreshModules();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // ─────────────────────────────────────────────────────────────────
        // Sidebar — Module List
        // ─────────────────────────────────────────────────────────────────

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(SIDEBAR_WIDTH));
            {
                m_SidebarScroll = EditorGUILayout.BeginScrollView(m_SidebarScroll);
                {
                    // Welcome tab
                    DrawWelcomeCard();

                    if (m_Modules.Count > 0)
                    {
                        GUILayout.Space(4);
                        DrawSidebarSeparator("Installed Modules");
                    }

                    if (m_Modules.Count == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "No Cupdeos modules detected.\n\n" +
                            "Modules register automatically when installed. " +
                            "Check that your module's Editor assembly references Cupdeos.Common.Editor.",
                            MessageType.Info);
                    }

                    foreach (var module in m_Modules)
                    {
                        DrawModuleCard(module);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawWelcomeCard()
        {
            var style = m_ShowWelcome ? m_SelectedCardStyle : m_CardStyle;

            EditorGUILayout.BeginVertical(style, GUILayout.Height(44));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var iconRect = GUILayoutUtility.GetRect(28, 28, GUILayout.Width(28));
                    EditorGUI.DrawRect(iconRect, new Color(0.25f, 0.55f, 0.8f));
                    GUI.Label(iconRect, "?",
                        new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                        {
                            fontSize = 16,
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = Color.white }
                        });

                    GUILayout.Space(6);
                    GUILayout.Label("Welcome", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            var cardRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
            {
                m_ShowWelcome = true;
                m_SelectedModule = null;
                Event.current.Use();
                Repaint();
            }
        }

        private void DrawSidebarSeparator(string label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(4);
                GUILayout.Label(label, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModuleCard(ICupdeosModule module)
        {
            bool isSelected = m_SelectedModule != null &&
                              m_SelectedModule.Abbreviation == module.Abbreviation;

            var style = isSelected ? m_SelectedCardStyle : m_CardStyle;

            EditorGUILayout.BeginVertical(style, GUILayout.Height(MODULE_CARD_HEIGHT));
            {
                // Row 1: Icon + Name + Version
                EditorGUILayout.BeginHorizontal();
                {
                    if (module.Icon != null)
                    {
                        GUILayout.Label(module.Icon, GUILayout.Width(32), GUILayout.Height(32));
                    }
                    else
                    {
                        // Placeholder
                        var rect = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
                        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
                        GUI.Label(rect, module.Abbreviation.Substring(0,
                            Mathf.Min(3, module.Abbreviation.Length)),
                            new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                            {
                                fontSize = 10,
                                normal = { textColor = Color.white }
                            });
                    }

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.LabelField(module.Abbreviation,
                            EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"v{module.Version}",
                            EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndVertical();

                    // Status indicator
                    GUILayout.FlexibleSpace();
                    string status = module.GetStatusMessage();
                    var statusStyle = string.IsNullOrEmpty(status)
                        ? m_StatusOkStyle
                        : m_StatusWarnStyle;
                    GUILayout.Label(string.IsNullOrEmpty(status) ? "●" : "⚠",
                        statusStyle, GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);

                // Row 2: Description (truncated)
                string desc = module.Description;
                if (desc != null && desc.Length > 60)
                    desc = desc.Substring(0, 57) + "...";
                EditorGUILayout.LabelField(desc ?? "", EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndVertical();

            // Click handling
            var cardRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
            {
                m_SelectedModule = module;
                m_ShowWelcome = false;
                Event.current.Use();
                Repaint();
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Detail Panel
        // ─────────────────────────────────────────────────────────────────

        private void DrawDetailPanel()
        {
            EditorGUILayout.BeginVertical("box");
            {
                if (m_ShowWelcome)
                {
                    DrawWelcomePage();
                }
                else if (m_SelectedModule != null)
                {
                    DrawModuleDetail(m_SelectedModule);
                }
                else
                {
                    DrawWelcomePage();
                }
            }
            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────────────────────
        // Welcome Page
        // ─────────────────────────────────────────────────────────────────

        private void DrawWelcomePage()
        {
            m_DetailScroll = EditorGUILayout.BeginScrollView(m_DetailScroll);
            {
                GUILayout.Space(PADDING);

                GUILayout.Label("Cupdeos Hub", m_HeaderStyle);
                GUILayout.Space(4);
                GUILayout.Label("Your central overview for all Cupdeos modules.",
                    EditorStyles.wordWrappedLabel);

                GUILayout.Space(16);

                // What is this?
                DrawInfoCard("What is this?",
                    "The Cupdeos Hub automatically detects all installed Cupdeos modules " +
                    "in your project. Select a module from the sidebar to see its details, " +
                    "check its status, access settings, or open the documentation.\n\n" +
                    "Modules register themselves — no configuration needed. " +
                    "When you import a Cupdeos package, it will appear here automatically.");

                GUILayout.Space(8);

                // What do I see per module?
                DrawInfoCard("Module Details",
                    "For each module you can see:\n\n" +
                    "• Version and author information\n" +
                    "• A status indicator that warns you if something needs attention " +
                    "(e.g. a required Manager component missing from the scene)\n" +
                    "• Quick access buttons to open the module's settings window or documentation\n" +
                    "• Required and optional dependencies at a glance");

                GUILayout.Space(8);

                // Dependencies explained
                DrawInfoCard("Understanding Dependencies",
                    "Each module can declare two types of dependencies:\n\n" +
                    "Required Dependencies are packages this module needs to function — " +
                    "typically Game Creator 2 Core or specific GC2 add-on modules like Stats. " +
                    "These are not limited to Cupdeos products.\n\n" +
                    "Optional Integrations list other Cupdeos modules that enhance " +
                    "or complement this module. For example, the Companion System (CPS) " +
                    "works on its own but can leverage the Buff System (BSM) or " +
                    "Relationship System (RS) for richer gameplay. " +
                    "The Hub only checks whether these Cupdeos modules are installed — " +
                    "it does not enforce them.");

                GUILayout.Space(16);

                // Installed overview
                EditorGUILayout.LabelField("Installed Modules", m_SubHeaderStyle);
                GUILayout.Space(4);

                if (m_Modules.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No modules detected yet. Import a Cupdeos package to get started.",
                        MessageType.Info);
                }
                else
                {
                    foreach (var module in m_Modules)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string statusIcon = string.IsNullOrEmpty(module.GetStatusMessage())
                                ? "✓" : "⚠";
                            var statusStyle = string.IsNullOrEmpty(module.GetStatusMessage())
                                ? m_StatusOkStyle : m_StatusWarnStyle;

                            GUILayout.Label(statusIcon, statusStyle, GUILayout.Width(20));
                            GUILayout.Label($"{module.Abbreviation} — {module.DisplayName}");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label($"v{module.Version}", EditorStyles.miniLabel);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(16);

                // Footer
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("c-huck.com", EditorStyles.linkLabel))
                        Application.OpenURL("https://c-huck.com");
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(PADDING);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawInfoCard(string title, string body)
        {
            EditorGUILayout.BeginVertical(m_WelcomeCardStyle ?? "box");
            {
                GUILayout.Space(4);
                EditorGUILayout.LabelField(title, m_SubHeaderStyle);
                GUILayout.Space(2);
                EditorGUILayout.LabelField(body,
                    m_BodyTextStyle ?? EditorStyles.wordWrappedLabel);
                GUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────────────────────
        // Module Detail
        // ─────────────────────────────────────────────────────────────────
        private void DrawModuleDetail(ICupdeosModule module)
        {
            m_DetailScroll = EditorGUILayout.BeginScrollView(m_DetailScroll);
            {
                GUILayout.Space(PADDING);

                // Header
                EditorGUILayout.BeginHorizontal();
                {
                    if (module.Icon != null)
                    {
                        GUILayout.Label(module.Icon, GUILayout.Width(48), GUILayout.Height(48));
                        GUILayout.Space(8);
                    }

                    EditorGUILayout.BeginVertical();
                    {
                        GUILayout.Label(module.DisplayName, m_HeaderStyle);
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"v{module.Version}", m_VersionBadgeStyle);
                            GUILayout.Space(8);
                            GUILayout.Label($"by {module.Author}", EditorStyles.miniLabel);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Status
                string status = module.GetStatusMessage();
                if (!string.IsNullOrEmpty(status))
                {
                    EditorGUILayout.HelpBox(status, MessageType.Warning);
                    GUILayout.Space(8);
                }

                // Description
                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(module.Description, EditorStyles.wordWrappedLabel);
                GUILayout.Space(12);

                // Actions
                EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    if (module.HasSettingsWindow)
                    {
                        if (GUILayout.Button("Open Settings", GUILayout.Height(28)))
                        {
                            module.OpenSettingsWindow();
                        }
                    }

                    if (!string.IsNullOrEmpty(module.DocumentationUrl))
                    {
                        if (GUILayout.Button("Documentation", GUILayout.Height(28)))
                        {
                            Application.OpenURL(module.DocumentationUrl);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(12);

                // Dependencies
                DrawDependencySection(
                    "Required Dependencies",
                    "Packages and frameworks this module needs to compile and function. " +
                    "This can include Game Creator 2 Core, GC2 add-on modules, " +
                    "or other third-party packages.",
                    module.RequiredDependencies,
                    true);

                DrawDependencySection(
                    "Optional Integrations",
                    "Other Cupdeos modules that complement or enhance this module. " +
                    "The module works without them — installing them unlocks additional " +
                    "features or cross-module synergies. Only Cupdeos products are listed here.",
                    module.OptionalDependencies,
                    false);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawDependencySection(
            string title, string description, ICupdeosDependency[] deps, bool isRequired)
        {
            if (deps == null || deps.Length == 0) return;

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            // Description block
            EditorGUILayout.BeginVertical(m_DescriptionBoxStyle ?? EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField(description,
                    m_BodyTextStyle ?? EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(4);

            foreach (ICupdeosDependency dep in deps)
            {
                // For optional integrations (Cupdeos only), check by abbreviation.
                // For required deps, also check — but non-Cupdeos deps (e.g. "GC2 Core")
                // won't match any module and show as "External" instead of "Not found".
                bool installed = m_Modules.Any(m =>
                    m.Abbreviation.Equals(dep.Abbreviation, StringComparison.OrdinalIgnoreCase));


                bool isExternalDep = isRequired && !installed &&
                    dep.Abbreviation.StartsWith("GC2", StringComparison.OrdinalIgnoreCase);

                EditorGUILayout.BeginHorizontal();
                {
                    if (installed)
                    {
                        GUILayout.Label("✓", m_StatusOkStyle, GUILayout.Width(20));
                    }
                    else if (isExternalDep)
                    {
                        GUILayout.Label("—", EditorStyles.centeredGreyMiniLabel,
                            GUILayout.Width(20));
                    }
                    else
                    {
                        GUILayout.Label("○", m_StatusWarnStyle, GUILayout.Width(20));
                    }

                    GUILayout.Label(dep.DisplayName);

                    string statusLabel = installed ? "Installed"
                        : isExternalDep ? "External package"
                        : "Not found";
                    GUILayout.Label(statusLabel, EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
        }

        // ═══════════════════════════════════════════════════════════════════
        // STYLES
        // ═══════════════════════════════════════════════════════════════════

        private void InitStyles()
        {
            if (m_HeaderStyle != null) return;

            m_HeaderStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            m_SubHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13
            };

            m_CardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(8, 8, 6, 6),
                margin = new RectOffset(4, 4, 2, 2)
            };

            m_SelectedCardStyle = new GUIStyle(m_CardStyle);
            var selectedBg = new Texture2D(1, 1);
            selectedBg.SetPixel(0, 0, EditorGUIUtility.isProSkin
                ? new Color(0.17f, 0.36f, 0.53f, 0.8f)
                : new Color(0.24f, 0.48f, 0.9f, 0.3f));
            selectedBg.Apply();
            m_SelectedCardStyle.normal.background = selectedBg;

            m_VersionBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Italic
            };

            m_StatusOkStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.3f, 0.8f, 0.3f) },
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            m_StatusWarnStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.9f, 0.7f, 0.2f) },
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            m_DescriptionBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6),
                margin = new RectOffset(0, 0, 2, 2)
            };

            m_BodyTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                richText = true
            };

            m_WelcomeCardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(12, 12, 8, 8),
                margin = new RectOffset(0, 0, 2, 4)
            };
        }
    }
}
