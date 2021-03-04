using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>),true)]
    internal class SerializableDictionaryEditor : PropertyDrawer
    {
        private static readonly float spaceHeight = 4;
        private static readonly float spaceWidth = 4;
        private static readonly float buttonWidth = EditorGUIUtility.singleLineHeight;
        private static readonly float folderWidth = EditorGUIUtility.singleLineHeight * 0.5f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty keys = property.FindPropertyRelative("keys");
            SerializedProperty values = property.FindPropertyRelative("values");
            int count = keys.arraySize;

            float height = 0;
            height += EditorGUIUtility.singleLineHeight;
            height += spaceHeight;
            for (int i = 0; i < count; ++i)
            {
                height += EditorGUI.GetPropertyHeight(keys.GetArrayElementAtIndex(i));
                SerializedProperty value = values.GetArrayElementAtIndex(i);
                if (value.isExpanded)
                {
                    height += EditorGUI.GetPropertyHeight(value, true);
                    height -= EditorGUIUtility.singleLineHeight;
                }
                height += spaceHeight;
            }

            height += EditorGUIUtility.singleLineHeight;
            height += spaceHeight;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Box(position, GUIContent.none);

            SerializedProperty keys = property.FindPropertyRelative("keys");
            SerializedProperty values = property.FindPropertyRelative("values");
            int count = keys.arraySize;

            Rect rect = position;

            rect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(rect, label);

            rect.width -= spaceWidth * 2f;
            rect.x += spaceWidth;

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.y += spaceHeight;

            Rect button = rect;
            button.x = rect.x + rect.width - buttonWidth;
            button.height = EditorGUIUtility.singleLineHeight;
            button.width = buttonWidth;

            Rect folder = rect;
            folder.width = folderWidth;

            rect.width = rect.width - buttonWidth - folderWidth;
            rect.x += folderWidth;

            for (int i = 0; i < count; ++i)
            {
                SerializedProperty key = keys.GetArrayElementAtIndex(i);
                SerializedProperty value = values.GetArrayElementAtIndex(i);

                float heightForKey = EditorGUI.GetPropertyHeight(key);
                rect.height = heightForKey;
                EditorGUI.PropertyField(rect, key, GUIContent.none);

                button.y = rect.y;
                folder.y = rect.y;

                rect.y += heightForKey;

                if (value.isExpanded)
                {
                    SerializedProperty valueCopy = value.Copy();
                    int depth = valueCopy.depth;

                    if (valueCopy.NextVisible(valueCopy.isExpanded))
                    {
                        do
                        {
                            if (valueCopy.depth <= depth)
                            {
                                break;
                            }

                            float h = EditorGUI.GetPropertyHeight(valueCopy, false);
                            rect.height = h;
                            EditorGUI.PropertyField(rect, valueCopy);
                            rect.y += h;
                            rect.y += EditorGUIUtility.standardVerticalSpacing;
                        }
                        while (valueCopy.NextVisible(valueCopy.isExpanded));
                    }
                }

                folder.height = rect.y - folder.y;

                if (GUI.Button(folder, ""))
                {
                    value.isExpanded = !value.isExpanded;
                    return;
                }

                if (GUI.Button(button, "-"))
                {
                    keys.DeleteArrayElementAtIndex(i);
                    values.DeleteArrayElementAtIndex(i);
                    return;
                }

                rect.y += spaceHeight;
            }

            if(property.FindPropertyRelative("invalidFlag").boolValue)
            {
                EditorGUI.HelpBox(rect, "Duplicate keys exist", MessageType.Error);
            }
            else
            {
                Rect buttonRect = rect;
                buttonRect.width = Mathf.Min(rect.width, 100);
                buttonRect.x = rect.center.x - buttonRect.width * 0.5f;
                if (GUI.Button(buttonRect, "+"))
                {
                    keys.InsertArrayElementAtIndex(count);
                    values.InsertArrayElementAtIndex(count);
                }
            }
        }
    }
}
