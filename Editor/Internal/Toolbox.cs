using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace VRC.FVPM.Internal
{
    internal static class Toolbox
    {
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        
        public static string ToRelativePath(this string path)
        {
            // Replace all \ with /
            
            path = Regex.Replace(
                path,
                @"\\",
                "/",
                RegexOptions.IgnoreCase
            );

            var currentDirectory = Regex.Replace(
                Directory.GetCurrentDirectory(),
                @"\\",
                "/",
                RegexOptions.IgnoreCase
            );
            
            path = path.Replace(currentDirectory, "");
            
            return path.StartsWith("/")
                ? path.Substring(1)
                : path;
        }
        
        public static string GetHashString(this string inputString)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
        
        public static byte[] GetHash(this string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        
        public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        public static Texture2D ToTexture2D(this string base64)
        {
            var texture = new Texture2D(1, 1);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.LoadImage(Convert.FromBase64String(base64));
            return texture;
        }
        
        public static Texture2D ToTexture2D(this Texture texture)
        {
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }

        public static string ToBase64(this Texture texture) => ToBase64(texture.ToTexture2D());
        public static string ToBase64(this Texture2D texture)
        {
            try
            {
                var bytes = texture.EncodeToPNG();
                return Convert.ToBase64String(bytes);
            }
            catch(ArgumentException e)
            {
                // Show a modal dialog to the user
                // Tell them that the image is not readable, and if the system should try to fix it
                // If the user says yes, make the image readable and try again
                // If the user says no, then don't do anythin                
                if (EditorUtility.DisplayDialog(
                        "Image is not readable",
                        "The image is not readable.\n\n" +
                        "This is usually due to the Read/Write flag not being set on the asset.\n\n" +
                        "Would you like the system to try to fix this?",
                        "Yes, fix automatically",
                        "Cancel"
                    ))
                {
                    try
                    {
                        // Apple the Read/Write flag on the asset
                        SetTextureImporterFormat(texture, true);
                    }
                    catch (Exception)
                    {
                        // Show a modal dialog to the user
                        // Tell them that it failed, and why it failed
                        EditorUtility.DisplayDialog(
                            "Failed to fix image",
                            "The system failed to fix the image.\n\n" +
                            e.Message,
                            "Cancel"
                        );
                        return null;
                    }
                    // Try again
                    try
                    {
                        return texture.ToBase64();
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog(
                            "Failed to fix image",
                            "The system failed to fix the image.\n\n" +
                            "No further attempts will be made.",
                            "Cancel"
                        );
                        return null;
                    }
                }

                return null;
            }
        }
        
        public static bool IsBase64String(this string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return false;
            try
            {
                _ = Convert.FromBase64String(
                    base64.PadRight(
                        base64.Length / 4 * 4 + (base64.Length % 4 == 0 ? 0 : 4),
                        '='
                    )
                );
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static void SetTextureImporterFormat( Texture2D texture, bool isReadable)
        {
            if ( null == texture ) return;

            var assetPath = AssetDatabase.GetAssetPath( texture );
            if (string.IsNullOrEmpty(assetPath))
                return;
            
            var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
            if (tImporter == null) return;
            
            tImporter.textureType = TextureImporterType.Sprite;
            tImporter.isReadable = isReadable;

            AssetDatabase.ImportAsset( assetPath );
            AssetDatabase.Refresh();
        }

        public static Texture2D GenerateTexture(float rgb, float a = 1) => GenerateTexture(rgb, rgb, rgb, a);
        // Generate 1x1 texture from rgb color (0-1)
        public static Texture2D GenerateTexture(float r, float g, float b, float a = 1)
        {
            // Check if each color is 0-1 and not 0-255
            if (r > 1) r /= 255;
            if (g > 1) g /= 255;
            if (b > 1) b /= 255;
            if (a > 1) a /= 255;
            
            var texture = new Texture2D(1, 1);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, new Color(r, g, b, a));
            texture.Apply();
            return texture;
        }
        
        public class IndentLevelScope : IDisposable
        {
            public IndentLevelScope()
            {
                EditorGUI.indentLevel++;
            }
            public void Dispose()
            {
                EditorGUI.indentLevel--;
            }
        }
        
        public class HorizontalScope : IDisposable
        {
            public HorizontalScope()
            {
                EditorGUILayout.BeginHorizontal();
            }
            
            public HorizontalScope(params GUILayoutOption[] options)
            {
                EditorGUILayout.BeginHorizontal(options);
            }
            
            public void Dispose()
            {
                EditorGUILayout.EndHorizontal();
            }
        }
        
        public class VerticalScope : IDisposable
        {
            public VerticalScope()
            {
                EditorGUILayout.BeginVertical();
            }
            
            public VerticalScope(params GUILayoutOption[] options)
            {
                EditorGUILayout.BeginVertical(options);
            }
            
            public void Dispose()
            {
                EditorGUILayout.EndVertical();
            }
        }
        
        public class DisabledScope : IDisposable
        {
            public DisabledScope(bool disabled = true)
            {
                EditorGUI.BeginDisabledGroup(disabled);
            }
            public void Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }
        
        public class ChangeCheckScope : IDisposable
        {
            private bool _disposed;
            
            public ChangeCheckScope()
            {
                EditorGUI.BeginChangeCheck();
            }
            public bool HasChanged()
            {
                _disposed = true;
                return EditorGUI.EndChangeCheck();
            }
            public void Dispose()
            {
                if (!_disposed)
                    EditorGUI.EndChangeCheck();
            }
        }
        
        public static Texture2D ScaleTexture(this Texture2D source,int targetWidth,int targetHeight) {
            var result=new Texture2D(targetWidth,targetHeight,source.format,false);
            for (var i = 0; i < result.height; ++i) {
                for (var j = 0; j < result.width; ++j) {
                    var newColor = source.GetPixelBilinear(j / (float)result.width, i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }
            result.Apply();
            return result;
        }
    }
}