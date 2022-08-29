using System;
using SimpleJSON;
using VRC.FVPM.Graphical;

// ReSharper disable InconsistentNaming

namespace VRC.FVPM.Package
{
    [Serializable]
    public class FullPackage
    {
        // ToDo: Add a "auto import to assets" option
        
        public string Name = "";
        public string DisplayName = "";
        public Version Version = new Version(0, 0, 0);

        public string UnityVersion = "2019.4.31f1";
        
        public string Description = "";
        public Author Author = new Author();
        
        public Dependency[] UnityDependencies = Array.Empty<Dependency>();
        public Dependency[] VpmDependencies = Array.Empty<Dependency>();

        public string DownloadUrl = "";
        public string Repository = "";
        
        public bool Deprecated = false;
        
        public string[] Tags = Array.Empty<string>();

        public Base64Texture Icon;

        public string VRChatVersion = "";
        public bool HideInEditor = true;
        
        public Sample[] Samples = Array.Empty<Sample>();
        
        public LegacyTarget[] LegacyFolders = Array.Empty<LegacyTarget>();
        public LegacyTarget[] LegacyFiles = Array.Empty<LegacyTarget>();

        public bool IsBeta => Version.IsBeta;

        public void ApplyJson(JSONNode child)
        {
            Name = child["name"];
            DisplayName = child["displayName"];
            Version = child["version"].Value;
            
            UnityVersion = child["unity"];
            
            Description = child["description"];
            Author = Author.FromJson(child["author"]);
            
            UnityDependencies = Dependency.ParseArray(child["dependencies"]);
            VpmDependencies = Dependency.ParseArray(child["vpmDependencies"]);
            
            DownloadUrl = child["url"];
            Repository = child["repo"];
            
            Deprecated = child.HasKey("deprecated") && child["deprecated"].AsBool;

            Tags = child.HasKey("tags")
                ? child["tags"].AsArray
                : Array.Empty<string>();

            VRChatVersion = child["vrchatVersion"];
            HideInEditor = child["hideInEditor"];

            Samples = Sample.ParseArray(child["samples"]);

            LegacyFolders = LegacyTarget.ParseArray(child["legacyFolders"]);
            LegacyFolders = LegacyTarget.ParseArray(child["legacyFiles"]);
        }

        public static FullPackage FromJson(string json) => FromJson(JSON.Parse(json));
        public static FullPackage FromJson(JSONNode json)
        {
            var package = new FullPackage();
            package.ApplyJson(json);
            return package;
        }
    }
}