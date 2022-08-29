using UnityEditor;
using UnityEngine;
using VRC.FVPM.ScriptableObjects;

namespace VRC.FVPM.UI
{
    [CustomEditor(typeof(FvpmPackage))]
    public class FvpmPackageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // ToDo: Make the inspector look good
            
            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Install Package"))
                PackageInstallationPopup.Show(target as FvpmPackage);
        }
    }
}