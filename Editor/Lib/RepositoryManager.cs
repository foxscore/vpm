using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SimpleJSON;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VRC.FVPM.Internal;
using VRC.FVPM.Package;

namespace Editor.Lib
{
    [InitializeOnLoad]
    public static class RepositoryManager
    {
        private static bool _isInit;
        
        private const string FullRepoCachePath = "Assets/Cache~/FVPM/Repositories";
        private static List<Repository> _repositories;
        private const string PerformedAutomaticRepositoryUpdateKey = "FVPM_PERFORMED_REPO_AUTOUPDATE";

        static RepositoryManager()
        {
            Init();
        }

        public static void Init()
        {
            if (_isInit) return;
            _isInit = true;
            
            EditorApplication.quitting -= OnExit;
            EditorApplication.quitting += OnExit;
            
            GlobalPreferences.Init();
            
            _repositories = new List<Repository>();
            _repositories.Clear();

            // Assets/Cache~/FVPM/Repositories
            Mkdir("Assets/Cache~");
            Mkdir("Assets/Cache~/FVPM");
            Mkdir(FullRepoCachePath);
            
            // ToDo: Check if has already run this session, if so, load them from cache instead
            
            if (
                !CheckForLocalRepoDifferences() &&
                EditorPrefs.HasKey(PerformedAutomaticRepositoryUpdateKey)
            )
                ReloadRepositories();
            else
            {
                UpdateRepositories();
                EditorPrefs.SetBool(PerformedAutomaticRepositoryUpdateKey, true);
            }
        }

        private static void OnExit()
        {
            EditorPrefs.DeleteKey(PerformedAutomaticRepositoryUpdateKey);
        }

        private static bool CheckForLocalRepoDifferences()
        {
            var repos = GlobalPreferences.GetRepositories();
            
            // Check if there are any differences between the local repo and the global repo
            var files = Directory.GetFiles(FullRepoCachePath, "*.asset", SearchOption.TopDirectoryOnly);
            return repos.Length != files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Count(f => repos.Any(r => r.GetHashString() == f));
        }
        
        public static Repository[] GetRepositories() => _repositories.ToArray();

        private static void Mkdir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void UpdateRepositories()
        {
            GlobalPreferences.Init();
            
            var repos = GlobalPreferences.GetRepositories();
            
            var files = Directory.GetFiles(FullRepoCachePath, "*.asset", SearchOption.TopDirectoryOnly);
            foreach (var deleteMe in files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(f => repos.Any(r => r.GetHashString() == f))
            )
                AssetDatabase.DeleteAsset(Path.Combine(FullRepoCachePath, deleteMe + ".json"));

            var client = new WebClient();
            _repositories.Clear();
            
            foreach (var repo in repos)
            {
                try
                {
                    var raw = client.DownloadString(repo);
                    var instance = new Repository();
                    var json = JSON.Parse(raw);
                    instance.ApplyJson(json);
                    File.WriteAllText(Path.Combine(FullRepoCachePath, repo.GetHashString() + ".json"), raw);
                    _repositories.Add(instance);
                }
                catch (Exception e)
                {
                    // Notify the user that there was an error while updating the repository
                    Debug.LogError($"Error while updating repository {repo} ({e.Message})");
                    Debug.LogException(e);
                }
            }
            
            client.Dispose();
        }
        
        public static void ReloadRepositories()
        {
            _repositories.Clear();
            var files = Directory.GetFiles(FullRepoCachePath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var repo = new Repository();
                    repo.ApplyJson(json);
                    _repositories.Add(repo);
                }
                catch (Exception e)
                {
                    // Notify the user that there was an error while updating the repository
                    Debug.LogError($"Error while loading repository with GUID {file} ({e.Message})");
                }
            }
        }
        
        // public static Dependency[] BuildDependencies(FullPackage package) =>
        //     BuildDependencies(new[] { package });
        // public static Dependency[] BuildDependencies(FullPackage[] packages)
        // {
        //     var packages = new List<Tuple<string, string, string>>();
        //     
        // }
    }
}
