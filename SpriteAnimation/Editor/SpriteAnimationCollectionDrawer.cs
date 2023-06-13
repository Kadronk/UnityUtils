using System;
using UnityEditor;
using UnityEngine;

namespace Kadronk.SpriteAnimation.Editor
{
    [CustomEditor(typeof(SpriteAnimationCollection))]
    public class SpriteAnimationCollectionDrawer : UnityEditor.Editor
    {
        // Serialized Properties
        private SerializedProperty _propIds;
        private SerializedProperty _propAssets;

        private void OnEnable()
        {
            _propIds = serializedObject.FindProperty("_IDs");
            _propAssets = serializedObject.FindProperty("_assets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();

            while (_propAssets.arraySize < _propIds.arraySize)
            {
                _propAssets.InsertArrayElementAtIndex(_propAssets.arraySize);
            }

            for (int i = 0; i < _propIds.arraySize; i++)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Rect rectElement = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.currentViewWidth, lineHeight, lineHeight);

                EditorGUI.PropertyField(new Rect(rectElement.x, rectElement.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), _propIds.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rectElement.x + EditorGUIUtility.labelWidth + 5.0f, rectElement.y, EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 105.0f, EditorGUIUtility.singleLineHeight), _propAssets.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUI.Button(new Rect(rectElement.x + EditorGUIUtility.labelWidth + 5.0f + EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 100.0f, rectElement.y, 60.0f, rectElement.height), "-"))
                {
                    _propIds.DeleteArrayElementAtIndex(i);
                    _propAssets.DeleteArrayElementAtIndex(i);
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(100.0f)))
            {
                _propIds.InsertArrayElementAtIndex(_propIds.arraySize);
                _propAssets.InsertArrayElementAtIndex(_propAssets.arraySize);
            }
        }
    }
}