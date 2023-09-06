using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kadronk.PolymorphicFields.Editor
{
    /// <summary>
    /// Should only be used in <b>Custom Editors</b>. This does not work in custom property drawers.<br/>
    /// Can be used along <see cref="PolymorphableAttribute"/> to add a type-switching button to each element. The attribute's <see cref="PolymorphableAttribute.Style"/> will be applied <i>on top</i> of this PolymorphicList label style.
    /// </summary>
    public class PolymorphicList<T>
    {
        public ReorderableList.ElementCallbackDelegate drawElementCallback => _list.drawElementCallback;
        public ReorderableList.ElementHeightCallbackDelegate elementHeightCallback => _list.elementHeightCallback;
        
        private ReorderableList _list;

        public PolymorphicList(SerializedProperty property, bool includeParentClass = false, LabelStyle style = LabelStyle.Append) {
            _list = new ReorderableList(property.serializedObject, property, true, true, true, true);
            _list.drawHeaderCallback += rect => EditorGUI.LabelField(rect, property.displayName);
            _list.drawElementCallback += (rect, index, active, focused) => {
                SerializedProperty arrayElement = property.GetArrayElementAtIndex(index);
                GUIContent label = new GUIContent(arrayElement.displayName);
                if (arrayElement.managedReferenceValue != null) {
                    switch (style) {
                        case LabelStyle.Append:
                            label.text += $" ({arrayElement.managedReferenceValue.GetType().GetDisplayName()})";
                            break;
                        case LabelStyle.Replace:
                            label.text = arrayElement.managedReferenceValue.GetType().GetDisplayName();
                            break;
                        case LabelStyle.Empty:
                            label = GUIContent.none;
                            break;
                    }
                }
                EditorGUI.PropertyField(rect, arrayElement, label, true);
            };
            _list.elementHeightCallback += index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index));
            _list.onAddDropdownCallback += (rect, list) =>
            {
                List<Type> types = new List<Type>(typeof(T).GetSubtypes());
                if (includeParentClass) {
                    ++types.Capacity;
                    types.Insert(0, typeof(T));
                }

                string[] typesStr = new string[types.Count];
                for (int i = 0; i < types.Count; i++) {
                    typesStr[i] = types[i].GetDisplayName();
                }

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < types.Count; i++) {
                    menu.AddItem(new GUIContent(typesStr[i]), false, CreateInstance, types[i]);
                }
                menu.DropDown(rect);

                void CreateInstance(object type) {
                    ++property.arraySize;
                    property.GetArrayElementAtIndex(property.arraySize-1).managedReferenceValue = Activator.CreateInstance((Type)type);
                    property.serializedObject.ApplyModifiedProperties();
                }
            };
        }

        public void DoList(Rect position) {
            _list.DoList(position);
        }

        public void DoLayoutList() {
            _list.DoLayoutList();
        }
    }
}