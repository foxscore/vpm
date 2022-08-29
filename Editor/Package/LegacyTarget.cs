using System;
using System.Collections.Generic;
using SimpleJSON;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class LegacyTarget
    {
        public string path;
        public string guid;
        
        public LegacyTarget(string path, string guid)
        {
            this.path = path;
            this.guid = guid;
        }

        public static LegacyTarget[] ParseArray(JSONNode jsonNode)
        {
            if (jsonNode == null)
                return Array.Empty<LegacyTarget>();
            
            var list = new List<LegacyTarget>();
            foreach (var key in jsonNode.Keys)
                list.Add(new LegacyTarget(key, jsonNode[key]));
            return list.ToArray();
        }
    }
}