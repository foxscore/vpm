// using System;
// using System.IO;
// using System.Net;
// using SimpleJSON;
// using UnityEditor;
// using UnityEngine;
// using VRC.FVPM.Internal;
// using VRC.FVPM.Package;
// using VRC.FVPM.ScriptableObjects;
//
// namespace VRC.FVPM.Dev
// {
//     public class DownloadRepositoryWindow : EditorWindow
//     {
//         [MenuItem("FVPM/Dev/Download Repository", false, 900)]
//         private static void ShowWindow()
//         {
//             var window = GetWindow<DownloadRepositoryWindow>(true);
//             window.titleContent = new GUIContent("Download repository");
//             window.Show();
//         }
//         
//         private string path = "";
//         private string url = "";
//
//         private void OnEnable()
//         {
//             const float windowHeight = 21 * 3 + 8;
//             const float windowWidth = 500;
//             minSize = new Vector2(windowWidth, windowHeight);
//             maxSize = new Vector2(windowWidth, windowHeight);
//             
//             // Center window on screen
//             position = new Rect(
//                 (Screen.currentResolution.width - windowWidth) / 2,
//                 (Screen.currentResolution.height - windowHeight) / 2,
//                 windowWidth,
//                 windowHeight
//             );
//         }
//
//         private void OnGUI()
//         {
//             using (new Toolbox.HorizontalScope())
//             {
//                 using (new Toolbox.DisabledScope())
//                     EditorGUILayout.TextField("Save to", path);
//
//                 if (EditorGUILayout.DropdownButton(new GUIContent("Select"), FocusType.Keyboard, GUILayout.Width(64)))
//                 {
//                     var path = EditorUtility.OpenFolderPanel("Select folder", "Assets/", "");
//                     if (!string.IsNullOrEmpty(path))
//                         this.path = path;
//                 }
//             }
//
//             url = EditorGUILayout.TextField("URL", url);
//
//             EditorGUILayout.Separator();
//
//             using (new Toolbox.DisabledScope(
//                 path == "" ||
//                 url == "" ||
//                 !Directory.Exists(path) ||
//                 !Uri.IsWellFormedUriString(url, UriKind.Absolute)
//             ))
//                 if (GUILayout.Button("Download"))
//                 {
//                     var client = new WebClient();
//                     var content = client.DownloadString(url);
//                     var rootNode = JSON.Parse(content);
//
//                     if (rootNode == null)
//                     {
//                         Debug.LogError("Failed to parse JSON");
//                         return;
//                     }
//
//                     var instance = CreateInstance<RepositoryInstance>();
//                     instance.Repository = new Repository();
//                     instance.Repository.ApplyJson(rootNode);
//                     instance.name = instance.Repository.Name;
//                     AssetDatabase.CreateAsset(
//                         instance,
//                         Path
//                             .Combine(path, instance.name + ".asset")
//                             .ToRelativePath()
//                     );
//                     AssetDatabase.Refresh();
//                     Selection.activeObject = instance;
//
//                     // ToDo: When saving a repository file, the name should be the hash to the name, as to avoid character conflicts.
//                 }
//         }
//     }
// }