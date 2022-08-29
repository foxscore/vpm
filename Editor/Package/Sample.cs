using System;
using SimpleJSON;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class Sample
    {
        public string name;
        public string description;
        public string path;
        
        public Sample(string name, string description, string path)
        {
            this.name = name;
            this.description = description;
            this.path = path;
        }

        public static Sample[] ParseArray(JSONNode jsonNode)
        {
            if (jsonNode == null)
                return Array.Empty<Sample>();
            
            var array = new Sample[jsonNode.Count];
            for (var i = 0; i < jsonNode.Count; i++)
                array[i] = Parse(jsonNode[i]);
            return array;
        }

        private static Sample Parse(JSONNode jsonNode)
        {
            return new Sample(
                jsonNode["name"],
                jsonNode["description"],
                jsonNode["path"]
            );
        }
    }
}