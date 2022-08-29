using System;
using System.Collections.Generic;
using System.Linq;
using Editor.AssetProcessors;
using Editor.Lib;
using UnityEditor;
using UnityEngine;
using VRC.FVPM.Internal;
using VRC.FVPM.Package;
using VRC.FVPM.ScriptableObjects;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedVariable

namespace VRC.FVPM.UI
{
    public class PackageReferenceCreatorWindow : EditorWindow
    {
        private static readonly string[] SourceSelection = new string[]
        {
            "FVPM",
            "VRChat Official",
            "Custom"
        };
        
        void Resize(Vector2 size)
        {
            minSize = size;
            maxSize = size;
            
            // Center the window on the screen
            position = new Rect(
                position.xMin + (Screen.currentResolution.width - size.x) / 2,
                position.yMin + (Screen.currentResolution.height - size.y) / 2,
                size.x,
                size.y
            );
        }
        
        [MenuItem("FVPM/Create FVPM Package Reference", false, 759)]
        private static void ShowWindow()
        {
            var window = GetWindow<PackageReferenceCreatorWindow>(true);
            window.titleContent = new GUIContent("Create FVPM Package Reference");
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(450, 225);
            maxSize = new Vector2(450, 225);

            _repositories = RepositoryManager.GetRepositories();
            _repositoryNames = _repositories.Select(r => r.name).ToArray();
        }
        
        private Repository[] _repositories;
        private string[] _repositoryNames;
        private int _repositoryIndex = -1;
        
        private Repository _previousRepository;
        private string[] _packageNames;
        private int _packageIndex = -1;

        private string[] _versions;
        private int _versionIndex = -1;
        
        private RepositoryPackageEntry _previousPackage;
        
        private void OnGUI()
        {
            // ToDo: Select repository, <download metadata>, select package, select version, press export
            
            // Show a helpbox explaining the purpose of this window
            EditorGUILayout.HelpBox(
                "This window allows you to create a FVPM Package Reference." +
                "\n" +
                "A FVPM Package Reference is a ScriptableObject that contains all the information " +
                "needed to download a package from any VPM repository." +
                "\n" +
                "You can then use this reference to easily share your package (or repository) with other users.",
                MessageType.Info
            );
            
            EditorGUILayout.Separator();
            
            // Repository selection
            _repositoryIndex = EditorGUILayout.Popup("Repository", _repositoryIndex, _repositoryNames);
            
            EditorGUILayout.Separator();

            if (_repositoryIndex == -1)
            {
                _previousRepository = null;
            }
            else
            {
                if (_previousRepository != _repositories[_repositoryIndex])
                {
                    _previousRepository = _repositories[_repositoryIndex];
                    _packageNames = _repositories[_repositoryIndex].packages
                        .Select(p => p.name)
                        .ToArray();
                    _packageIndex = -1;
                    _previousPackage = null;
                }
                
                // Package selection
                _packageIndex = EditorGUILayout.Popup("Package", _packageIndex, _packageNames);
                
                EditorGUILayout.Separator();

                if (_packageIndex == -1)
                {
                    _previousPackage = null;
                }
                else
                {
                    if (_previousPackage != _repositories[_repositoryIndex].packages[_packageIndex])
                    {
                        var packageEntry = _repositories[_repositoryIndex].packages[_packageIndex];

                        _versions = packageEntry.packages
                            .Select(p => p.Version.ToString())
                            .ToArray();
                        
                        _versionIndex = _versions.FindIndex(v => 
                            v == packageEntry.GetMostRecentPackage(false).Version
                        );
                    }
                    
                    // Version selection
                    _versionIndex = EditorGUILayout.Popup("Version", _versionIndex, _versions);
                    
                    EditorGUILayout.Separator();
                    
                    // ToDo: Collect dependencies and their origins.
                    // Make sure to warn the user if there are any dependencies
                    // that are not available in any local repositories.

                    var additionalRepositories = new List<string>();
                    var package = _repositories[_repositoryIndex].packages[_packageIndex].packages[_versionIndex];
                    
                    var scannedDependencies = new Dictionary<string, string>();
                    var missingDependencies = new Dictionary<string, string>();
                    var yetToScanDependencies = new Dictionary<string, string>();
                    
                    foreach (var dependency in package.VpmDependencies)
                    {
                        var repoEntry = PackageManager.GetPackage(dependency.packageName, dependency.version);
                        // ToDo : Dependency shit
                    }
                    
                    // Create button
                    if (GUILayout.Button("Create"))
                    {
                        var packageEntry = _repositories[_repositoryIndex].packages[_packageIndex];
                        var pkg = packageEntry.packages.First(p => p.Version == _versions[_versionIndex]);
                        if (CreatePackageReference(
                            pkg.DisplayName,
                            pkg.Name,
                            pkg.Version,
                            _repositories[_repositoryIndex].url
                        ))
                            Close();
                    }
                    
                    _previousPackage = _repositories[_repositoryIndex].packages[_packageIndex];
                }
                
                _previousRepository = _repositories[_repositoryIndex];
            }
        }

        public static bool CreatePackageReference(string displayName, string name, string version, string repository)
        {
            
            var path = EditorUtility.SaveFilePanel(
                "Save FVPM Package Reference",
                "",
                $"{displayName}",
                "asset"
            );
            
            if (string.IsNullOrEmpty(path))
                return false;
            
            // ToDo: Collect dependencies and their origins.
            
            var packageReference = CreateInstance<FvpmPackage>();
            packageReference.packageName = name;
            packageReference.packageVersion = version;
            packageReference.repositoryUrl = repository;
            
            InstallerPostprocessor.AddToIgnoreList(path.ToRelativePath());
            
            AssetDatabase.CreateAsset(packageReference, path.ToRelativePath());
            AssetDatabase.Refresh();
            
            Selection.activeObject = packageReference;
            // Highlight the newly created asset
            EditorGUIUtility.PingObject(packageReference);

            return true;
        }

        #region Legacy - Draw icon selection, reuse later
        // void DrawIconSelection()
        // {
        //     const int iconSize = 128;
        //     const int offset = 3;
        //     const int buttonHeight = 32;
        //     var rect = EditorGUILayout.GetControlRect(false, iconSize);
        //     
        //     var iconRect = new Rect(rect.x + (EditorGUI.indentLevel * 18), rect.y, iconSize, iconSize);
        //     
        //     // Place a button next to the icon.
        //
        //     var spaceRemaining = rect.width - iconSize - offset;
        //     var buttonRect = new Rect(
        //         rect.x + iconSize + offset + (spaceRemaining * .175f),
        //         rect.y + (rect.height - 3 - (buttonHeight)*2) / 2,
        //         (spaceRemaining * .75f),
        //         buttonHeight
        //     );
        //     var bottomButtonRect = new Rect(
        //         rect.x + iconSize + offset + (spaceRemaining * .175f),
        //         rect.y + (rect.height - 3 - (buttonHeight)*2) / 2 + 3 + buttonHeight,
        //         (spaceRemaining * .75f),
        //         buttonHeight
        //     );
        //     
        //     if (previousBase64 != target.iconBase64)
        //     {
        //         previousBase64 = target.iconBase64;
        //         textureCache = string.IsNullOrEmpty(target.iconBase64)
        //             ? Texture2D.grayTexture
        //             : target.iconBase64.ToTexture2D();
        //     }
        //     
        //     // Draw the icon.
        //     GUI.DrawTexture(iconRect, textureCache);
        //     
        //     // Draw the buttons.
        //     
        //     // Select icon from file.
        //     if (GUI.Button(buttonRect, "Select Icon"))
        //     {
        //         var path = EditorUtility.OpenFilePanel("Select Icon", "", "png");
        //         if (!string.IsNullOrEmpty(path))
        //         {
        //             var text = new Texture2D(iconSize, iconSize);
        //             text.LoadImage(System.IO.File.ReadAllBytes(path));
        //             const int sizeLimit = 1024;
        //             if (text.height > sizeLimit || text.width > sizeLimit)
        //             {
        //                 // Ask the user if they want to cancel or resize the image.
        //                 if (EditorUtility.DisplayDialog(
        //                     "Image too large",
        //                     "The image you selected is too large." +
        //                     "\nWould you like to resize it?" +
        //                     "\n\nYour resolution: \t\t" + text.width + "x" + text.height +
        //                     "\nMaximum resolution: \t" + sizeLimit + "x" + sizeLimit,
        //                     "Resize",
        //                     "Cancel"
        //                 ))
        //                 {
        //                     if (text.height > text.width)
        //                         text = text.ScaleTexture(sizeLimit, sizeLimit * (text.height / text.width));
        //                     else
        //                         text = text.ScaleTexture(sizeLimit * (text.width / text.height), sizeLimit);
        //                     target.iconBase64 = text.ToBase64();
        //                 }
        //             }
        //             else
        //                 target.iconBase64 = text.ToBase64();
        //         }
        //     }
        //     
        //     // Clear button
        //     if (GUI.Button(bottomButtonRect, "Clear"))
        //         target.iconBase64 = string.Empty;
        // }
        #endregion
    }
}