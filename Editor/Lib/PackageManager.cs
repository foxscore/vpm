using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.FVPM.Package;
using VRC.FVPM.ScriptableObjects;

namespace Editor.Lib
{
    [InitializeOnLoad]
    public static class PackageManager
    {
        private static bool _isInitialized;

        private static List<RepositoryPackageEntry> _packageEntries;
        
        static PackageManager()
        {
            Init();
        }

        public static void Init()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            RepositoryManager.Init();
            
            _packageEntries = new List<RepositoryPackageEntry>();
            foreach (var repo in RepositoryManager.GetRepositories())
            {
                foreach (var repositoryPackageEntry in repo.packages)
                {
                    var entry = GetPackageEntry(repositoryPackageEntry.name);
                    if (entry == null)
                        _packageEntries.Add(repositoryPackageEntry);
                    else
                        Debug.LogWarning(
                            $"Origin conflict: {repositoryPackageEntry.name}" +
                            $"\nSelected: {entry.packages[0].Repository}" +
                            $"\nSkipped: {repo.url}"
                        );
                }
            }
        }

        public static RepositoryPackageEntry GetPackageEntry(string name)
        {
            return _packageEntries.FirstOrDefault(x => x.name == name);
        }
        
        /// <summary>
        /// WARNING: Providing a dynamic version will be performance-wise inefficient.
        /// </summary>
        public static FullPackage GetPackage(string name, Version version, bool allowBeta = false)
        {
            var entry = GetPackageEntry(name);

            if (version.IsDynamic)
            {
                #region Major
                
                if (version.Major == -1)
                    return entry.GetMostRecentPackage(allowBeta);
                
                var packages = entry.packages
                    .Where(p => p.Version.Major == version.Major)
                    .ToArray();
                
                if (!allowBeta)
                    packages = packages.Where(p => p.IsBeta == false).ToArray();
                
                // ToDo: Store results whenever possible
                // Aka.: Get rid of "multiple possible enumerations" warning
                
                if (!packages.Any())
                    return null;

                if (packages.Count() == 1)
                    return packages[0];
                
                #endregion
                
                #region Minor

                if (version.Minor == -1)
                {
                    packages = packages.OrderByDescending(p => p.Version.Minor).ToArray();
                    packages = packages.Where(p => p.Version.Minor == packages[0].Version.Minor).ToArray();
                    packages = packages.OrderByDescending(p => p.Version.Build).ToArray();
                    packages = packages.Where(p => p.Version.Build == packages[0].Version.Build).ToArray();
                    
                    if (packages.Count() == 1)
                        return packages[0];
                    
                    return packages.OrderByDescending(p => p.Version.Beta).First();
                }
                
                packages = packages.Where(p => p.Version.Minor == version.Minor).ToArray();

                if (!packages.Any())
                    return null;
                
                if (packages.Count() == 1)
                    return packages[0];

                #endregion
                
                #region Build
                
                if (version.Build == -1)
                {
                    packages = packages.OrderByDescending(p => p.Version.Build).ToArray();
                    packages = packages.Where(p => p.Version.Build == packages[0].Version.Build).ToArray();
                    
                    if (packages.Count() == 1)
                        return packages[0];
                    
                    return packages.OrderByDescending(p => p.Version.Beta).First();
                }
                
                packages = packages.Where(p => p.Version.Build == version.Build).ToArray();
                
                if (!packages.Any())
                    return null;
                
                if (packages.Count() == 1)
                    return packages[0];
                
                #endregion
                
                #region Beta
                
                return packages
                    .OrderBy(p => p.Version.Beta)
                    .First();
                
                #endregion

                // ToDo return latest package
            }
            
            return entry?
                .packages
                .FirstOrDefault(x => x.Version == version);
        }
        
        public static void InstallPackage(FullPackage package)
        {
            // ToDo: Install package
            Debug.Log("Installing package: " + package.Name);
        }
        
        public static void InstallPackage(FvpmPackage package)
        {
            // ToDo: Find full package and call InstallPackage(FullPackage)
            Debug.Log("Installing package: " + package.name);
        }


        public static FullPackage GetInstalled(string packageName)
        {
            FullPackage cache;
            
            // Don't convert to Linq, because it will load all packages into memory
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dir in Directory.GetDirectories("Packages"))
            {
                cache = FullPackage.FromJson(File.ReadAllText(dir + "/package.json"));
                if (cache.Name == packageName)
                    return cache;
            }

            return null;
        }

        public static void UninstallPackage(FullPackage selection)
        {
            // ToDo: Uninstall package
            Debug.Log("Uninstalling package: " + selection.Name);
        }
    }
}