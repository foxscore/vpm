using System.Collections.Generic;
using UnityEditor;
using VRC.FVPM.ScriptableObjects;
using VRC.FVPM.UI;

namespace Editor.AssetProcessors
{
    public class InstallerPostprocessor : AssetPostprocessor
    {
        private static InstallerPostprocessor _instance;
        
        private List<string> _tmpIgnoreList = new List<string>();
        
        public InstallerPostprocessor()
        {
            _instance = this;
        }
        
        public static void AddToIgnoreList(string assetPath)
        {
            _instance._tmpIgnoreList.Add(assetPath);
        }
        
        public static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            // Look for assets that were imported
            // if they are in the ignore list, remove them from the ignore list and don't process them
            foreach (var assetPath in importedAssets)
            {
                if (_instance._tmpIgnoreList.Contains(assetPath))
                {
                    _instance._tmpIgnoreList.Remove(assetPath);
                    continue;
                }
                
                // If the asset is a FvpmPackage, process it
                if (!assetPath.EndsWith(".asset")) return;
                
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(FvpmPackage)) return;
                
                PackageInstallationPopup.Show(AssetDatabase.LoadAssetAtPath<FvpmPackage>(assetPath));
            }
        }
    }
}