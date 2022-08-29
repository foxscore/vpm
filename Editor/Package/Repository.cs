using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using VRC.FVPM.Enums;
using VRC.FVPM.Graphical;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class Repository
    {
        public string name = "";
        public string url = "";
        public string author = "";
        public string description = "";
        
        public RepositoryPackageEntry[] packages = Array.Empty<RepositoryPackageEntry>();
        
        public RepositoryFeatures features = 0;
        
        public Base64Texture icon;
        public AuthorizationMode authorizationMode = AuthorizationMode.None;

        internal void ApplyJson(JSONNode rootNode)
        {
            name = rootNode["name"];
            url = rootNode["url"];
            author = rootNode["author"];
            
            if (rootNode.HasKey("description"))
                description = rootNode["description"];
            else
                description = "";

            if (rootNode.HasKey("features"))
                foreach (var item in rootNode["features"].AsArray)
                    features |= (RepositoryFeatures) Enum.Parse(typeof(RepositoryFeatures), item.Value);
            else
                features = 0;

            if ((features & RepositoryFeatures.Icon) != 0)
                icon = rootNode["icon"];
            
            if (
                ((features & RepositoryFeatures.FvpmAuthorizationOptional) != 0) ||
                ((features & RepositoryFeatures.FvpmAuthorizationRequired) != 0)
            )
                authorizationMode = (AuthorizationMode) Enum.Parse(typeof(AuthorizationMode), rootNode["authorizationMode"]);
            
            #region Packages
            var packagesNode = rootNode["packages"];
            var pkgs = packagesNode.Keys;
            var list = new List<RepositoryPackageEntry>();
            foreach (var pkgName in pkgs)
            {
                var pkg = packagesNode[pkgName];
                var children = pkg["versions"].Children;
                var childrenList = new List<FullPackage>();
                
                foreach (var child in children)
                {
                    var fullPackage = new FullPackage();
                    fullPackage.ApplyJson(child);
                    childrenList.Add(fullPackage);
                }
                
                var entry = new RepositoryPackageEntry();
                entry.name = pkgName;
                entry.packages = childrenList.ToArray();
                list.Add(entry);
            }
            packages = list.ToArray();
            #endregion
        }
        
        // Custom array operator that returns the first package with the given name
        public RepositoryPackageEntry this[string packageName] =>
            packages.FirstOrDefault(package => package.name == packageName);

        public bool Supports(RepositoryFeatures feature) => (features & feature) == feature;
    }
}