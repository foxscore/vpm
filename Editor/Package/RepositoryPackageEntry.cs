using System;
using System.Linq;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class RepositoryPackageEntry
    {
        public string name = "";
        public FullPackage[] packages = Array.Empty<FullPackage>();
        
        public FullPackage GetMostRecentPackage(bool allowBeta)
        {
            if (packages.Length == 0)
                return null;
            
            var mostRecent = packages[0];
            foreach (var p in packages)
            {
                if (p.IsBeta && !allowBeta)
                    continue;
                
                if (p.Version > mostRecent.Version)
                    mostRecent = p;
            }
            
            return mostRecent;
        }
        
        public FullPackage[] GetPackages(bool allowBeta)
        {
            return packages.Where(p => p.IsBeta == allowBeta).ToArray();
        }
    }
}