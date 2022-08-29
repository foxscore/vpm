using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SimpleJSON;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class Dependency : ISerializable
    {
        public string packageName;
        public Version version;
        
        public Dependency(string packageName, Version version)
        {
            this.packageName = packageName;
            this.version = version;
        }

        public Dependency(SerializationInfo info, StreamingContext context)
        {
            packageName = info.GetString("packageName");
            version = (Version)info.GetValue("version", typeof(Version));
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", packageName);
            info.AddValue("Version", version);
        }

        public static Dependency[] ParseArray(JSONNode node)
        {
            if (node == null)
                return Array.Empty<Dependency>();
            
            var list = new List<Dependency>();
            foreach (var key in node.Keys)
                list.Add(new Dependency(key, new Version(node[key].Value)));
            return list.ToArray();
        }
    }
}