using System;
using System.Runtime.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRC.FVPM.Package
{
    [Serializable]
    public class Version : ISerializable, IComparable, IComparable<Version>
    {
        private string _version = "0.0.0";
        private int _major;
        private int _minor;
        private int _build;
        private int _beta;

        public Version(string version)
        {
            VersionString = version;
        }
        
        public Version(int major, int minor, int build, int beta = 0)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Beta = beta;
            BuildVersionString();
        }

        public Version(SerializationInfo info, StreamingContext context)
        {
            _major = info.GetInt32("Major");
            _minor = info.GetInt32("Minor");
            _build = info.GetInt32("Build");
            _beta = info.GetInt32("Beta");
            BuildVersionString();
        }
        
        /// <summary>
        /// An integer of -1 indicates 'x' (any).
        /// This is used in relation with dependency calculations.
        /// </summary>
        public int Major
        {
            get { return _major; }
            set
            {
                _major = value;
                BuildVersionString();
            }
        }
        
        /// <summary>
        /// An integer of -1 indicates 'x' (any).
        /// This is used in relation with dependency calculations.
        /// </summary>
        public int Minor
        {
            get { return _minor; }
            set
            {
                _minor = value;
                BuildVersionString();
            }
        }
        
        /// <summary>
        /// An integer of -1 indicates 'x' (any).
        /// This is used in relation with dependency calculations.
        /// </summary>
        public int Build
        {
            get { return _build; }
            set
            {
                _build = value;
                BuildVersionString();
            }
        }
        
        public int Beta
        {
            get { return _beta; }
            set
            {
                _beta = value;
                BuildVersionString();
            }
        }

        public bool IsBeta => _beta > 0;
        
        private void BuildVersionString() => _version = GenerateVersionString(_major, _minor, _build, _beta);
        
        public static string GenerateVersionString(int major, int minor, int build, int beta = 0)
        {
            var versionString = major + "." + minor + "." + build;
            
            if (beta > 0)
                versionString += "-beta." + beta;

            return versionString;
        }
        
        public string VersionString
        {
            get => _version;
            set
            {
                _version = value;
                
                var betaSplit = value.Split('-');

                if (betaSplit.Length > 2)
                {
                    Reset();
                    return;
                }
                
                var versionSplit = betaSplit[0].Split('.');
                if (versionSplit.Length != 3)
                {
                    Reset();
                    return;
                }

                _major = versionSplit[0] == "x"
                    ? _major = -1
                    : _major = int.Parse(versionSplit[0]);
                
                _minor = versionSplit[1] == "x"
                    ? _minor = -1
                    : _minor = int.Parse(versionSplit[1]);
                
                _build = versionSplit[2] == "x"
                    ? _build = -1
                    : _build = int.Parse(versionSplit[2]);
                
                if (betaSplit.Length == 2)
                {
                    if (!betaSplit[1].StartsWith("beta."))
                    {
                        Reset();
                        return;
                    }
                    
                    _beta = int.Parse(betaSplit[1].Substring(5));

                    if (_beta == 0)
                        _version = $"{_major}.{_minor}.{_build}";
                }
            }
        }

        private void Reset()
        {
            _version = "0.0.0";
            _major = 0;
            _minor = 0;
            _build = 0;
            _beta = 0;
        }
        
        public bool IsValid => _version != "0.0.0";
        public bool IsDynamic => _major == -1 || _minor == -1 || _build == -1;
        
        public static implicit operator string(Version v) => v.VersionString;
        public static implicit operator Version(string v) => new Version(v);
        
        public static bool operator ==(Version v1, Version v2) => v1.VersionString == v2.VersionString;
        public static bool operator !=(Version v1, Version v2) => v1.VersionString != v2.VersionString;
        
        public static bool operator >(Version v1, Version v2) {
            if (v1.Major > v2.Major)
                return true;
            if (v1.Major < v2.Major)
                return false;
            
            if (v1.Minor > v2.Minor)
                return true;
            if (v1.Minor < v2.Minor)
                return false;
            
            if (v1.Build > v2.Build)
                return true;
            if (v1.Build < v2.Build)
                return false;
            
            if (v1.Beta > v2.Beta)
                return true;
            if (v1.Beta < v2.Beta)
                return false;
            
            return false;
        }
        public static bool operator <(Version v1, Version v2)
        {
            if (v1.Major < v2.Major)
                return true;
            if (v1.Major > v2.Major)
                return false;
            
            if (v1.Minor < v2.Minor)
                return true;
            if (v1.Minor > v2.Minor)
                return false;
            
            if (v1.Build < v2.Build)
                return true;
            if (v1.Build > v2.Build)
                return false;
            
            if (v1.Beta < v2.Beta)
                return true;
            if (v1.Beta > v2.Beta)
                return false;
            
            return false;
        }
        public static bool operator >=(Version v1, Version v2) => v1 > v2 || v1 == v2;
        public static bool operator <=(Version v1, Version v2) => v1 < v2 || v1 == v2;

        public override bool Equals(object obj)
        {
            if (obj is Version)
                return this == (Version)obj;
            return false;
        }
        
        public override int GetHashCode()
        {
            return VersionString.GetHashCode();
        }
        
        public override string ToString()
        {
            return VersionString;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Version))
                return 1;
            
            return CompareTo((Version)obj);
        }

        public int CompareTo(Version other)
        {
            return this > other ? 1 : 0;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Major", _major);
            info.AddValue("Minor", _minor);
            info.AddValue("Build", _build);
            info.AddValue("Beta", _beta);
        }
    }
}