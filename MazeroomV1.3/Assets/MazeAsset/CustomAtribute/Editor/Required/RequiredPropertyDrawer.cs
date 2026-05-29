using UnityEditor;
using UnityEngine;
namespace MazeAsset.CustomAttribute.Editor
{
    [CustomPropertyDrawer(typeof(Required))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null;

            if (isMissing)
            {
                var previousColor = GUI.color;
                GUI.color = Color.red;
                EditorGUI.PropertyField(position, property, label);
                GUI.color = previousColor;
                EditorGUI.HelpBox(position, $"{property.name} is required!", MessageType.Error);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool isMissing = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null;

            if (isMissing)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 40f;
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}