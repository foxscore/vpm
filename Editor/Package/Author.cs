using System;
using SimpleJSON;
// ReSharper disable InconsistentNaming

namespace VRC.FVPM.Package
{
    [Serializable]
    public class Author
    {
        public string Name;
        public string Email;
        public string Url;
        
        public Author()
        {
            Name = "";
            Email = "";
            Url = "";
        }
        
        public Author(string name, string email, string url)
        {
            Name = name;
            Email = email;
            Url = url;
        }
        
        public override string ToString()
        {
            return Name;
        }

        public static Author FromJson(JSONNode jsonNode)
        {
            var author = new Author();
            
            if (jsonNode == null)
                return author;
            
            if (jsonNode.HasKey("name"))
                author.Name = jsonNode["name"];
            
            if (jsonNode.HasKey("email"))
                author.Email = jsonNode["email"];
            
            if (jsonNode.HasKey("url"))
                author.Url = jsonNode["url"];
            
            return author;
        }
    }
}