using UnityEngine;
using UnityEditor;

namespace Kadronk.SpriteAnimation.Editor
{
    [CustomPropertyDrawer(typeof(SpriteFrame))]
    public class SpriteFrameDrawer : PropertyDrawer
    { 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 50.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect rectSprite = new Rect(position.x, position.y, position.height, position.height);
            EditorGUI.ObjectField(rectSprite, property.FindPropertyRelative("_sprite"), typeof(Sprite), GUIContent.none);
            Rect rectDelay = new Rect(position.x + position.height + 10.0f, position.y + position.height - EditorGUIUtility.singleLineHeight, position.height, EditorGUIUtility.singleLineHeight);
            property.FindPropertyRelative("_delay").floatValue = EditorGUI.FloatField(rectDelay, GUIContent.none, property.FindPropertyRelative("_delay").floatValue);
            
            EditorGUI.EndProperty();
        }
    }
}
