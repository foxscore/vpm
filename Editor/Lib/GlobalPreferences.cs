using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEditor;
using UnityEngine.Windows;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Editor.Lib
{
    [InitializeOnLoad]
    public static class GlobalPreferences
    {
        private const string DefaultRepoFile = "[\"https://vpm.directus.app/assets/936917f9-c2d4-4470-a5bc-9bb38176fa01.json\"]";
        
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FVPM"
        );
        private static readonly string RepositoriesPath = Path.Combine(AppDataPath, "Repositories.json");

        private static List<string> _repositories;

        private static bool _didInit;

        static GlobalPreferences()
        {
            Init();
        }
        
        public static Action OnRepositoriesChanged;

        public static void Init()
        {
            if (_didInit) return;
            _didInit = true;
            
            ExistenceCheck();
            Reload();
            
            // Watch for changes to the repositories file
            var fsw = new FileSystemWatcher();
            fsw.Path = AppDataPath;
            fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fsw.Filter = "*.json";
            fsw.Changed += (sender, e) =>
            {
                // Restore if missing
                if (e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed)
                    ExistenceCheck();
                    
                Reload();
                OnRepositoriesChanged?.Invoke();
            };
            fsw.EnableRaisingEvents = true;
            fsw.BeginInit();
        }

        static void ExistenceCheck()
        {
            
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            if (!File.Exists(RepositoriesPath))
                File.WriteAllText(RepositoriesPath, DefaultRepoFile);
        }

        static void Reload()
        {
            _repositories = new List<string>();
            var repositoriesString = File.ReadAllText(RepositoriesPath);
            var repositoriesJson = JSON.Parse(repositoriesString);
            _repositories = ((string[])repositoriesJson).ToList();
        }

        public static string[] GetRepositories() => _repositories.ToArray();
        
        public static void SaveRepositories()
        {
            var repositoriesJson = JSON.Parse("[]");
            foreach (var repository in _repositories)
                repositoriesJson.AsArray.Add(repository);
            File.WriteAllText(RepositoriesPath, repositoriesJson.ToString());
        }
        
        public static void RemoveRepository(string repository)
        {
            _repositories.Remove(repository);
            SaveRepositories();
        }
        
        public static void AddRepository(string repository)
        {
            _repositories.Add(repository);
            SaveRepositories();
        }
        
        public static bool HasRepository(string repository) => _repositories.Contains(repository);
    }
}