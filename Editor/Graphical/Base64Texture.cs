using System;
using System.Runtime.Serialization;
using SimpleJSON;
using UnityEngine;
using VRC.FVPM.Internal;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBePrivate.Global

namespace VRC.FVPM.Graphical
{
    [Serializable]
    public sealed class Base64Texture : ISerializable
    {
        #region Fields
        private string _base64;
        private Texture _texture;
        #endregion
        
        #region Constructors
        public Base64Texture(string base64)
        {
            Base64 = base64;
        }
        
        public Base64Texture(Texture texture)
        {
            Texture = texture;
        }

        public Base64Texture(SerializationInfo info, StreamingContext context)
        {
            Base64 = info.GetString("Base64");
        }
        #endregion
        
        #region Properties
        public string Base64
        {
            get => _base64;
            set
            {
                if (
                    string.IsNullOrEmpty(value) ||
                    !value.IsBase64String()
                )
                {
                    _base64 = null;
                    _texture = null;
                    return;
                }
                
                _base64 = value;
                _texture = _base64.ToTexture2D();
            }
        }

        public Texture Texture
        {
            get => _texture;
            set
            {
                if (value == null)
                {
                    _texture = null;
                    _base64 = null;
                    return;
                }
                
                _texture = value;
                _base64 = _texture.ToBase64();
            }
        }
        #endregion

        #region Conversion
        public static implicit operator Texture2D(Base64Texture base64Texture) => base64Texture.Texture.ToTexture2D();
        public static implicit operator Texture(Base64Texture base64Texture) => base64Texture.Texture;
        public static implicit operator string(Base64Texture base64Texture) => base64Texture.Base64;
        
        public static implicit operator Base64Texture(Texture2D texture) => new Base64Texture(texture);
        public static implicit operator Base64Texture(Texture texture) => new Base64Texture(texture);
        public static implicit operator Base64Texture(string base64) => new Base64Texture(base64);
        public static implicit operator Base64Texture(JSONNode node) => new Base64Texture(node.Value);
        #endregion

        #region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Base64", Base64);
        }
        #endregion
    }
}