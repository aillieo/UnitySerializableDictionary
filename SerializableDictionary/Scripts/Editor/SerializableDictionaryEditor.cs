using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    internal class SerializableDictionaryEditor : PropertyDrawer
    {
        private readonly Dictionary<string, bool> expandState = new Dictionary<string, bool>();

        private static readonly float spaceHeight = 4;
        private static readonly float spaceWidth = 4;
        private static readonly float buttonWidth = EditorGUIUtility.singleLineHeight;
        private static readonly float folderWidth = EditorGUIUtility.singleLineHeight * 1.0f;

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
                SerializedProperty key = keys.GetArrayElementAtIndex(i);
                height += EditorGUI.GetPropertyHeight(key);
                string keyPropertyPath = key.propertyPath;

                if (expandState.TryGetValue(keyPropertyPath, out bool expand) && expand)
                {
                    height += EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(i));
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
                rect.height = heightForKey;

                string keyPropertyPath = key.propertyPath;
                if (!expandState.TryGetValue(keyPropertyPath, out bool expand))
                {
                    expand = false;
                }

                if (expand)
                {
                    float heightForValue = EditorGUI.GetPropertyHeight(value);
                    rect.height = heightForValue;
                    bool addIndent = value.hasVisibleChildren;
                    if (addIndent)
                    {
                        rect.x += folderWidth;
                        rect.width -= folderWidth;

                        EditorGUI.PropertyField(rect, value, true);

                        rect.x -= folderWidth;
                        rect.width += folderWidth;
                    }
                    else
                    {
                        EditorGUI.PropertyField(rect, value, GUIContent.none);
                    }

                    rect.y += heightForValue;
                }

                folder.height = rect.y - folder.y;

                if (GUI.Button(folder, expand ? "-" : "+"))
                {
                    expand = !expand;
                    expandState[keyPropertyPath] = expand;
                    return;
                }

                if (GUI.Button(button, "x"))
                {
                    keys.DeleteArrayElementAtIndex(i);
                    values.DeleteArrayElementAtIndex(i);
                    return;
                }

                rect.y += spaceHeight;
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            if (property.FindPropertyRelative("invalidFlag").boolValue)
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
