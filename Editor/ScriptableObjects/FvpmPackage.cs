using System;
using UnityEngine;

namespace VRC.FVPM.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "FVPM Package Reference", menuName = "FVPM/Package Reference", order = 420)]
    public class FvpmPackage : ScriptableObject
    {
        public string repositoryUrl = "";
        
        public string packageName = "";
        public string packageVersion = "";
        public string[] additionalRepositories = Array.Empty<string>();
    }
}