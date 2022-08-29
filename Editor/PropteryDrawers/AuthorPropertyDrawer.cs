using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.FVPM.Internal;
using VRC.FVPM.Package;

namespace VRC.FVPM.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Author))]
    public class AuthorPropertyDrawer : PropertyDrawer
    {
        private const float SpaceHeight = 2f;
        private float _lineHeight;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _lineHeight = base.GetPropertyHeight(property, label);
            return property.isExpanded
                ? _lineHeight * 4 + SpaceHeight * 3f
                : _lineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Label
            var labelRect = property.isExpanded
                ? new Rect(position.x, position.y, position.width, _lineHeight)
                : position;

            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            // property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(labelRect, property.isExpanded, label);
            
            if (!property.isExpanded)
                return;

            // Properties

            var nameRect = new Rect(position.x, position.y + _lineHeight + SpaceHeight, position.width, _lineHeight);
            var emailRect = new Rect(position.x, position.y + _lineHeight * 2 + SpaceHeight * 2, position.width, _lineHeight);
            var urlRect = new Rect(position.x, position.y + _lineHeight * 3 + SpaceHeight * 3, position.width, _lineHeight);

            // using (new Toolbox.IndentLevelScope())
            // {
            //     nameRect = EditorGUI.IndentedRect(nameRect);
            //     emailRect = EditorGUI.IndentedRect(emailRect);
            //     websiteRect = EditorGUI.IndentedRect(websiteRect);
            // }

            using (new Toolbox.IndentLevelScope())
            {
                EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("Name"), new GUIContent("Name"));
                EditorGUI.PropertyField(emailRect, property.FindPropertyRelative("Email"), new GUIContent("Email"));
                EditorGUI.PropertyField(urlRect, property.FindPropertyRelative("Url"), new GUIContent("Url"));
            }
        }
    }
}