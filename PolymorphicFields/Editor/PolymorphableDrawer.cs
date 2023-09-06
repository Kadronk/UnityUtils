using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kadronk.PolymorphicFields.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphableAttribute))]
    public class PolymorphableDrawer : PropertyDrawer
    {
        private GUIContent _contSelectType = new GUIContent("Select type...");
        private GUIContent _contChangeType = new GUIContent("Change type");
        private GUIContent _contSetToNull = new GUIContent("Set to null");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue != null) {
                float buttonWidth = GUI.skin.button.CalcSize(_contChangeType).x + 8.0f;
                Rect rectChangeType = position;
                rectChangeType.xMin += position.width - buttonWidth;
                rectChangeType.height = EditorGUIUtility.singleLineHeight;
                if (EditorGUI.DropdownButton(rectChangeType, _contChangeType, FocusType.Keyboard)) {
                    SelectTypeMenu();
                }

                if (attribute != null) {
                    switch (((PolymorphableAttribute)attribute).Style)
                    {
                        case LabelStyle.Append:
                            label.text += $" ({property.managedReferenceValue.GetType().GetDisplayName()})";
                            break;
                        case LabelStyle.Replace:
                            label.text = property.managedReferenceValue.GetType().GetDisplayName();
                            break;
                        case LabelStyle.Empty:
                            label = GUIContent.none;
                            break;
                    }
                }

                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.LabelField(position, label);
            Rect rectSelectClass = position;
            rectSelectClass.xMin += EditorGUIUtility.labelWidth;
            if (EditorGUI.DropdownButton(rectSelectClass, _contSelectType, FocusType.Keyboard)) {
                SelectTypeMenu();
            }

            void SelectTypeMenu() {
                List<Type> types = new List<Type>(fieldInfo.FieldType.GetSubtypes());
                if (attribute != null && ((PolymorphableAttribute)attribute).IncludeParent) {
                    ++types.Capacity;
                    types.Insert(0, fieldInfo.FieldType);
                }
                string[] typesStr = new string[types.Count];
                for (int i = 0; i < types.Count; i++) {
                    typesStr[i] = types[i].GetDisplayName();
                }
                
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < types.Count; i++) {
                    menu.AddItem(new GUIContent(typesStr[i]), property.managedReferenceValue != null && property.managedReferenceValue.GetType() == types[i], CreateInstance, types[i]);
                }
                menu.AddSeparator(string.Empty);
                menu.AddItem(_contSetToNull, false, RemoveInstance);
                menu.ShowAsContext();
                
                void CreateInstance(object type) {
                    property.managedReferenceValue = Activator.CreateInstance((Type)type);
                    property.serializedObject.ApplyModifiedProperties();
                }

                void RemoveInstance() {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}