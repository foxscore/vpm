using System;
using Editor.Lib;
using UnityEditor;
using UnityEngine;
using VRC.FVPM.ScriptableObjects;
using VRC.FVPM.UI.InstallerPages;

namespace VRC.FVPM.UI
{
    public class PackageInstallationPopup : EditorWindow
    {
        const int Width = 800;
        const int Height = 350;
        
        public static void Show(FvpmPackage package)
        {
            var window = GetWindow<PackageInstallationPopup>(true);
            window._package = package;
            window.Show();
        }
        
        private FvpmPackage _package;
        private bool _didInit;

        private void OnEnable()
        {
            minSize = new Vector2(Width, Height);
            maxSize = new Vector2(Width, Height);
            
            titleContent = new GUIContent("FVPM Package Installation");
            
            _didInit = false;
        }

        void Init()
        {
            if (!_didInit)
            {
                _didInit = true;
                
                // Set the window position to the center of the screen
                position = new Rect(
                    (Screen.currentResolution.width - Width) * .5f,
                    (Screen.currentResolution.height - Height) * .5f,
                    Width,
                    Height
                );
            }
        }

        private void OnGUI()
        {
            Init();
            
            EditorGUILayout.LabelField("Install package " + _package.packageName + "?");
            
            if (GUILayout.Button("Install"))
            {
                PackageManager.InstallPackage(_package);
                Close();
            }
            
            // ToDo: EditorUpdate
            // ToDo: Draw page
            
            // ToDo: Check repositories, ask for repo installation (if needed) check dependencies

            if (GUILayout.Button("Cancel"))
                Close();
        }

#pragma warning disable CS0169
        private InstallerPage _currentPage;
#pragma warning restore CS0169
    }
}
