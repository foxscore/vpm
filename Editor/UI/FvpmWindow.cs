using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Editor.Lib;
using UnityEngine;
using UnityEditor;
using VRC.FVPM.Internal;
using VRC.FVPM.Package;
using Debug = UnityEngine.Debug;

namespace VRC.FVPM.UI
{
    public class FvpmWindow : EditorWindow
    {
        [Serializable]
        private class RepoEntry
        {
            public string Name;
            public bool Foldout;
            public PackageEntry[] Packages;
            
            public RepoEntry(string name, PackageEntry[] packages)
            {
                Foldout = true;
                Name = name;
                Packages = packages;
            }
        }

        [Serializable]
        private class PackageEntry
        {
            public FullPackage Current;
            public bool Foldout;
            public FullPackage[] Versions;
            public PackageState State;
            
            public PackageEntry(FullPackage current, FullPackage[] versions, PackageState state)
            {
                Current = current;
                Foldout = false;
                Versions = versions;
                State = state;
            }
        }

        private enum PackageState
        {
            NotInstalled,
            Installed,
            UpdateAvailable,
        }
        
        private static string DidAutoRefreshPrefsKey = "FVPM_DidAutoRefresh";
        private static readonly Vector2 MinSize = new Vector2(750, 500);
        
        private static GUIStyle _foldoutStyle;
        private static GUIStyle _invisibleButtonStyle;
        private static GUIStyle _tagStyle;
        private static GUIStyle _deprecatedStyle;
        private static GUIStyle _betaStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _sectionTitleStyle;
        private static GUIStyle _versionSectionStyle;

        private static Texture2D _bgRepo;
        private static Texture2D _bgPackage;
        private static Texture2D _bgVersion;
        private static Texture2D _bgSelected;

        // ToDo: Get custom icons - Unity is missing some, and some are missing light icons
        private static Texture _iconRefresh;
        private static Texture _iconInstalled;
        private static Texture _iconUpdate;
        private static Texture _iconDeprecated;

        private string _query = "";
        private RepoEntry[] _repos;
        private FullPackage _selection = null;
        private PackageEntry _selectionEntry = null;
        private Vector2 _scrollPosition = Vector2.zero;
        
        [MenuItem("FVPM/Package Manager", false, 750)]
        public static void ShowWindow()
        {
            var window = GetWindow<FvpmWindow>(true);
            window.minSize = MinSize;
            window.Show();
        }

        private static void OnQuit() => EditorPrefs.SetBool(DidAutoRefreshPrefsKey, false);

        private void OnEnable()
        {
            titleContent = new GUIContent(
                "Foxy's VPM",
                EditorGUIUtility.IconContent("Asset Store").image
            );
            minSize = MinSize;
            
            EditorApplication.quitting -= OnQuit;
            EditorApplication.quitting += OnQuit;

            if (EditorPrefs.GetBool(DidAutoRefreshPrefsKey, false))
                Refresh();
            else
                FilterPackages();
        }

        void Refresh()
        {
            // Refresh repos
            EditorUtility.DisplayProgressBar(
                "Foxy's VPM",
                "Refreshing repositories...", 
                0
            );
            RepositoryManager.UpdateRepositories();
            
            // Filter packages
            EditorUtility.DisplayProgressBar(
                "Foxy's VPM",
                "Filtering packages...", 
                0
            );
            FilterPackages();
            
            EditorUtility.ClearProgressBar();
        }

        void OnGUI()
        {
            StaticCheck();
            
            if (_repos == null) FilterPackages();
            
            DrawToolbar();
            
            // ToDo: Show error message if there are no packages
            
            using (new Toolbox.HorizontalScope())
            {
                // 291
                using (new Toolbox.VerticalScope(GUILayout.Width(293)))
                    DrawList();
                
                DrawVerticalLine();
                
                using (new Toolbox.VerticalScope())
                using (new Toolbox.IndentLevelScope())
                    DrawDetails();
            }
        }

        void DrawLine(Vector2 start, Vector2 end)
        {
            Handles.BeginGUI();
            var color = EditorGUIUtility.isProSkin ? 0.1098039216f : 0.3803921569f;
            Handles.color = new Color(color, color, color);
            Handles.DrawLine(start, end);
            Handles.EndGUI();
        }

        void DrawVerticalLine()
        {
            const float x = 294;
            DrawLine(
                new Vector3(x, 21),
                new Vector3(x, 550)
            );
        }

        void StaticCheck()
        {
            #region Textures
            
            if (_bgRepo == null)
                _bgRepo = Toolbox.GenerateTexture(EditorGUIUtility.isProSkin ? 41 : 213);
            
            if (_bgPackage == null)
                _bgPackage = Toolbox.GenerateTexture(EditorGUIUtility.isProSkin ? 56 : 194);
            
            if (_bgVersion == null)
                _bgVersion = Toolbox.GenerateTexture(EditorGUIUtility.isProSkin ? 63 : 213);
            
            if (_bgSelected == null)
                _bgSelected = EditorGUIUtility.isProSkin
                ? Toolbox.GenerateTexture(62, 95, 150)
                : Toolbox.GenerateTexture(62, 125, 231);
            
            if (_iconRefresh == null)
                _iconRefresh = EditorGUIUtility.IconContent("Refresh").image;
            
            if (_iconInstalled == null)
                _iconInstalled = EditorGUIUtility.IconContent("Installed").image;
            
            if (_iconUpdate == null)
                _iconUpdate = EditorGUIUtility.IconContent("SceneLoadIn").image;
            
            if (_iconDeprecated == null)
                _iconDeprecated = EditorGUIUtility.IconContent("ol_minus_act").image;
            
            #endregion

            #region Styles
            if (_foldoutStyle == null)
                _foldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    // fontStyle = FontStyle.Bold,
                    fontSize = 14
                };

            if (_invisibleButtonStyle == null)
            {
                var texture = Toolbox.GenerateTexture(1, 0);
                _invisibleButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = texture },
                    hover = { background = texture },
                    active = { background = texture },
                    focused = { background = texture },
                    onNormal = { background = texture },
                    onHover = { background = texture },
                    onActive = { background = texture },
                    onFocused = { background = texture },
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }
            
            // The tag should have a rounded border
            // Border and text color should be the same
            // The color is gray
            if (_tagStyle == null)
            {
                _tagStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset(2, 2, 2, 2),
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.gray },
                };
            }
            
            // Deprecated tag should be orange
            if (_deprecatedStyle == null)
            {
                _deprecatedStyle = new GUIStyle(_tagStyle)
                {
                    normal = { textColor = Color.red }
                };
            }
            
            // Beta tag should be blue
            if (_betaStyle == null)
            {
                _betaStyle = new GUIStyle(_tagStyle)
                {
                    normal = { textColor = Color.blue }
                };
            }
            
            // Title should be bold
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 21,
                };
            }
            
            if (_sectionTitleStyle == null)
            {
                _sectionTitleStyle = new GUIStyle(_titleStyle)
                {
                    fontSize = 14,
                };
            }
            
            if (_versionSectionStyle == null)
            {
                _versionSectionStyle = new GUIStyle(_sectionTitleStyle)
                {
                    alignment = TextAnchor.MiddleRight
                };
            }
            #endregion
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // Refresh button
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                    Refresh();
                
                GUILayout.FlexibleSpace();
                
                // Settings button
                if (GUILayout.Button("Preferences", EditorStyles.toolbarDropDown))
                    ShowDropdownSettings();
                
                GUILayout.Space(2);
                
                // Search field
                EditorGUI.BeginChangeCheck();
                _query = GUILayout.TextField(_query, EditorStyles.toolbarSearchField, GUILayout.Width(450));
                if (EditorGUI.EndChangeCheck())
                    FilterPackages();
            }
            EditorGUILayout.EndHorizontal();
        }

        void ShowDropdownSettings()
        {
            // Options:
            // - Manage repositories
            // - Show beta packages
            // - Show deprecated packages
            // - Allow downgrading

            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Manage repositories"), false, () =>
            {
                // ToDo: Add manage repositories functionality
            });

            menu.AddSeparator("");
            
            menu.AddItem(
                new GUIContent("Show beta packages"),
                EditorPrefs.GetBool("FVPM_ShowBetaPackages", false),
                () =>
                {
                    EditorPrefs.SetBool("FVPM_ShowBetaPackages", !EditorPrefs.GetBool("FVPM_ShowBetaPackages", false));
                    FilterPackages();
                }
            );
            
            menu.AddItem(
                new GUIContent("Show deprecated packages"),
                EditorPrefs.GetBool("FVPM_ShowDeprecatedPackages", false),
                () =>
                {
                    EditorPrefs.SetBool("FVPM_ShowDeprecatedPackages", !EditorPrefs.GetBool("FVPM_ShowDeprecatedPackages", false));
                    FilterPackages();
                }
            );
            
            menu.AddSeparator("");
            
            menu.AddItem(
                new GUIContent("Allow downgrading"),
                EditorPrefs.GetBool("FVPM_AllowDowngrading", false),
                () =>
                {
                    EditorPrefs.SetBool("FVPM_AllowDowngrading", !EditorPrefs.GetBool("FVPM_AllowDowngrading", false));
                }
            );
            
            menu.ShowAsContext();
        }

        void FilterPackages()
        {
            // ToDo: implement filtering
            var showDeprecated = EditorPrefs.GetBool("FVPM_ShowDeprecatedPackages", false);
            var showBeta = EditorPrefs.GetBool("FVPM_ShowBetaPackages", false);
            var lowerQuery = _query.ToLower();
            var repos = RepositoryManager.GetRepositories();
            var cache = new List<RepoEntry>();

            foreach (var repo in repos)
            {
                var packages = new List<PackageEntry>();
                var repoPackages = repo.packages.OrderBy(p => p.name);

                foreach (var package in repoPackages)
                {
                    var pkgs = package.GetPackages(showBeta);
                    FullPackage installed = null;

                    // Check if the package is installed
                    try
                    {
                        installed = PackageManager.GetInstalled(package.name);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Invalid package manifest for {package.name}");
                        Debug.LogException(e);
                        continue;
                    }

                    // Check if it's only deprecated packages
                    if (!showDeprecated && package.packages.All(p => p.Deprecated))
                    {
                        if (installed != null)
                        {
                            pkgs = new[] { installed };
                            goto Filter;
                        }

                        continue;
                    }

                    // Check if it's only beta packages
                    if (!showBeta && package.packages.All(p => p.IsBeta))
                    {
                        if (installed != null)
                        {
                            pkgs = new[] { installed };
                            goto Filter;
                        }

                        continue;
                    }

                    if (!showDeprecated)
                        pkgs = pkgs.Where(p => !p.Deprecated).ToArray();

                    Filter:
                    // Filter with Query
                    if (!lowerQuery.IsNullOrEmpty())
                        pkgs = pkgs.Where(
                            p =>
                                p.Name.ToLower().Contains(lowerQuery) ||
                                p.DisplayName.ToLower().Contains(lowerQuery) ||
                                p.Description.ToLower().Contains(lowerQuery)
                        ).ToArray();

                    if (pkgs.Length == 0)
                        continue;

                    pkgs = pkgs.OrderByDescending(p => p.Version).ToArray();
                    
                    foreach (var pkg in pkgs)
                    {
                        if (installed == null || pkg.Version != installed.Version) continue;
                        
                        installed = pkg;
                        break;
                    }

                    packages.Add(
                        new PackageEntry(
                            installed ?? pkgs.First(),
                            pkgs,
                            installed == null
                                ? PackageState.NotInstalled
                                : installed.Version == pkgs.First().Version
                                    ? PackageState.Installed
                                    : PackageState.UpdateAvailable
                        )
                    );
                }

                if (!packages.Any()) continue;

                cache.Add(
                    new RepoEntry(
                        repo.name,
                        packages.OrderBy(p => p.Current.Name).ToArray()
                    )
                );
            }

            _repos = cache.OrderBy(re => re.Name).ToArray();
            
            // Find previous selected package by name, repository, and version
            FindPrevious();
            void FindPrevious()
            {
                if (_selection == null) return;

                foreach (var repo in _repos)
                {
                    if (
                        !_selection.Repository.IsNullOrEmpty() &&
                        repo.Packages[0].Current.Repository != _selection.Repository
                    )
                        continue;
                    
                    foreach (var package in repo.Packages)
                    {
                        if (package.Current.Name != _selection.Name)
                            continue;
                        
                        if (_selection.Version == package.Current.Version)
                        {
                            _selection = package.Current;
                            _selectionEntry = package;
                            return;
                        }
                        
                        foreach (var pkg in package.Versions)
                        {
                            if (pkg.Version != _selection.Version) continue;
                            
                            _selection = pkg;
                            package.Foldout = true;
                            _selectionEntry = package;
                            return;
                        }
                    }
                }
            }
        }

        void DrawList()
        {
            if (_repos == null) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var repo in _repos)
            {
                if (!RepoHeader(repo)) continue;
                
                foreach (var package in repo.Packages)
                    if (PackageHeader(package))
                        
                        foreach (var version in package.Versions)
                            PackageVersion(version, package);
            }
            
            EditorGUILayout.EndScrollView();

            bool RepoHeader(RepoEntry entry)
            {
                // Draw a collapsible header for the repo.
                // The header background should should be dark grey.
                // It has a foldout arrow and the repo name.
                // Return true if the repo is expanded.

                var rect = EditorGUILayout.GetControlRect(false, 22);

                rect.y -= 3;
                rect.height += 2;

                rect.x -= 3;
                rect.width += 6;

                GUI.DrawTexture(rect, _bgRepo);
                
                rect.x += 3;
                rect.width -= 3;
                
                DrawListLine(rect);

                rect.height -= 2;
                
                entry.Foldout = EditorGUI.Foldout(rect, entry.Foldout, entry.Name, _foldoutStyle);
                return entry.Foldout;
            }
            
            bool PackageHeader(PackageEntry entry)
            {
                var rect = EditorGUILayout.GetControlRect(false, 22);

                rect.y -= 3;
                rect.height += 2;

                rect.x -= 3;
                rect.width += 6;

                GUI.DrawTexture(rect, _selection == entry.Current ? _bgSelected : _bgPackage);
                
                rect.x += 3;
                rect.width -= 3;
                
                DrawListLine(rect);
                
                rect.height -= 2;
                
                entry.Foldout = EditorGUI.Foldout(rect, entry.Foldout, entry.Current.DisplayName, _foldoutStyle);

                var iconRect = new Rect(
                    rect.x + rect.width - 22,
                    rect.y + 4,
                    16,
                    16
                );
                
                if (entry.Current.Deprecated)
                    GUI.DrawTexture(iconRect, _iconDeprecated);
                
                else switch (entry.State)
                {
                    case PackageState.UpdateAvailable:
                        GUI.DrawTexture(iconRect, _iconUpdate);
                        break;
                    case PackageState.Installed:
                        GUI.DrawTexture(iconRect, _iconInstalled);
                        break;
                }

                rect.x += 14;
                rect.width -= 14;
                if (GUI.Button(rect, "", _invisibleButtonStyle))
                {
                    _selection = entry.Current;
                    _selectionEntry = entry;
                }

                // ToDo: Draw version & state icon
                
                return entry.Foldout;
            }
            
            void PackageVersion(FullPackage entry, PackageEntry parent)
            {
                var rect = EditorGUILayout.GetControlRect(false, 22);

                rect.y -= 3;
                rect.height += 2;

                rect.x -= 3;
                rect.width += 6;

                GUI.DrawTexture(rect, _selection == entry ? _bgSelected : _bgVersion);
                
                rect.x += 3;
                rect.width -= 3;
                
                DrawListLine(rect);
                
                rect.height -= 2;
                
                EditorGUI.LabelField(rect, entry.Version + "  ", _versionSectionStyle);

                if (GUI.Button(rect, "", _invisibleButtonStyle))
                {
                    _selection = entry;
                    _selectionEntry = parent;
                }
            }

            void DrawListLine(Rect rect)
            {
                DrawLine(new Vector2(rect.x - 3, rect.y + rect.height), new Vector2(rect.x + rect.width, rect.y + rect.height));
            }
        }

        void DrawDetails()
        {
            if (_selection == null) return;
            
            EditorGUILayout.Separator();

            #region Header

            //  Title: displayName
            //  Subtitle 1: name
            //  Subtitle 2: version
            //  Icon (if available)

            void DrawHeaderLabels()
            {
                EditorGUILayout.LabelField(_selection.DisplayName, _titleStyle, GUILayout.Height(_titleStyle.fontSize));
                using (new Toolbox.IndentLevelScope())
                {
                    EditorGUILayout.LabelField($"{_selection.Name}", EditorStyles.label);
                    EditorGUILayout.LabelField($"{_selection.Version}", EditorStyles.label);
                }
            }

            if (_selection.Icon == null || _selection.Icon.Texture == null)
                DrawHeaderLabels();
            else
            {
                using (new Toolbox.HorizontalScope())
                {
                    var rect = EditorGUILayout.GetControlRect(false, 100);
                    rect.height = EditorGUIUtility.singleLineHeight * 3 + 4;
                    GUI.DrawTexture(rect, _selection.Icon);
                    
                    using (new Toolbox.VerticalScope())
                        DrawHeaderLabels();
                }
            }

            #endregion

            EditorGUILayout.Separator();

            #region Buttons
            using (new Toolbox.HorizontalScope())
            {
                switch (_selectionEntry.State)
                {
                    case PackageState.NotInstalled:
                        if (Button("Install"))
                        {
                            PackageManager.InstallPackage(_selection);
                            Refresh();
                        }
                        break;
                    
                    case PackageState.Installed:
                    case PackageState.UpdateAvailable:
                        if (_selection.Version == _selectionEntry.Current.Version)
                        {
                            if (_selection.Version != _selectionEntry.Versions[0].Version)
                                if (Button($"Update to {_selectionEntry.Versions[0].Version}"))
                                {
                                    PackageManager.InstallPackage(_selectionEntry.Versions[0]);
                                    Refresh();
                                }
                            if (Button("Uninstall"))
                            {
                                PackageManager.UninstallPackage(_selection);
                                Refresh();
                            }
                        }
                        else if (_selection.Version > _selectionEntry.Current.Version)
                        {
                            if (Button($"Upgrade"))
                            {
                                PackageManager.InstallPackage(_selection);
                                Refresh();
                            }
                        }
                        else
                        {
                            using (new Toolbox.DisabledScope(
                                !EditorPrefs.GetBool("FVPM_AllowDowngrading", false)
                            ))
                                if (Button($"Downgrade"))
                                {
                                    PackageManager.InstallPackage(_selection);
                                    Refresh();
                                }
                        }
                        break;
                }

                if (GUILayout.Button("Share"))
                {
                    // ToDo: Share button
                    PackageReferenceCreatorWindow.CreatePackageReference(
                        _selection.DisplayName,
                        _selection.Name,
                        _selection.Version,
                        _selection.Repository
                    );
                }

                bool Button(string label)
                {
                    return EditorGUILayout.DropdownButton(
                        new GUIContent(label),
                        FocusType.Keyboard,
                        GUI.skin.button
                    );
                }
            }
            
            #endregion

            EditorGUILayout.Separator();

            #region Tags

            // Deprecated tag (if applicable) [orange]
            // Beta tag (if applicable) [blue]
            // Tags (if available) [gray]

            var hasLabels = _selection.Deprecated || _selection.IsBeta || _selection.Tags.Any();
            
            if (hasLabels)
                EditorGUILayout.LabelField("Tags", _sectionTitleStyle);

            if (_selection.Deprecated)
                EditorGUILayout.LabelField("Deprecated", _deprecatedStyle);

            if (_selection.IsBeta)
                EditorGUILayout.LabelField("Beta", _betaStyle);

            if (_selection.Tags.Any())
                foreach (var tag in _selection.Tags)
                    EditorGUILayout.LabelField(tag, _tagStyle);
            
            if (hasLabels)
                EditorGUILayout.Separator();

            #endregion
            
            #region Author
            
            EditorGUILayout.LabelField("Author", _sectionTitleStyle);
            using (new Toolbox.IndentLevelScope())
            {
                EditorGUILayout.LabelField(_selection.Author.Name.IsNullOrEmpty() ? "Unknown" : _selection.Author.Name);
                
                if (_selection.Author.Email.IsNullOrEmpty())
                    EditorGUILayout.LabelField(_selection.Author.Email);
                
                if (_selection.Author.Url.IsNullOrEmpty())
                    if (
                        EditorGUILayout.DropdownButton(
                            new GUIContent(_selection.Author.Url),
                            FocusType.Passive,
                            EditorStyles.linkLabel)
                    )
                    {
                        var url = _selection.Author.Url;

                        if (url.StartsWith("http://"))
                        {
                            // Remove http://
                            url = url.Substring(7);
                            
                            // Add https://
                            url = "https://" + url;
                        }
                        else if (!url.StartsWith("https://"))
                            url = "https://" + url;
                        
                        Process.Start(url);
                    }
            }

            #endregion
            
            EditorGUILayout.Separator();

            // Description
            if (!_selection.Description.IsNullOrEmpty())
            {
                EditorGUILayout.LabelField("Description", _sectionTitleStyle);
                using (new Toolbox.IndentLevelScope())
                    EditorGUILayout.LabelField(_selection.Description, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Separator();
            }

            // Samples (if available & installed)

            // Dependencies (if applicable)
        }
    }
}