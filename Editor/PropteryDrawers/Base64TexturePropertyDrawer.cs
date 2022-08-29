using UnityEditor;
using UnityEngine;
using VRC.FVPM.Graphical;
using VRC.FVPM.Internal;

namespace VRC.FVPM.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Base64Texture))]
    public class Base64TexturePropertyDrawer : PropertyDrawer
    {
        const int TextureSize = 128;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var baseHeight = base.GetPropertyHeight(property, label);
            return property.isExpanded ? baseHeight + TextureSize + 2 : baseHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            
            if (!property.isExpanded)
                return;
            
            #region Texture rect
            var textureRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                TextureSize
            );
            using (new Toolbox.IndentLevelScope())
                textureRect = EditorGUI.IndentedRect(textureRect);
            textureRect.width = TextureSize;
            // If the texture would draw outside of the rect, move it to the left.
            if (textureRect.xMax > position.xMax)
                textureRect.x -= textureRect.xMax - position.xMax;
            #endregion

            #region Texture
            Texture texture = Texture2D.grayTexture;
            var objText = property.FindPropertyRelative("Texture");
            if (objText != null && objText.objectReferenceValue != null)
                texture = objText.objectReferenceValue as Texture;
            #endregion
            
            GUI.DrawTexture(textureRect, texture);
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property) => true;
    }
}