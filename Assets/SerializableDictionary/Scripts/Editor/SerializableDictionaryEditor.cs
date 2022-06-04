using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Color lightGray = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Type cachedValueType;
        private Type valueType
        {
            get
            {
                if (cachedValueType == null)
                {
                    cachedValueType = GetValueType(fieldInfo.FieldType);
                }

                return cachedValueType;
            }
        }

        private static readonly HashSet<SerializedPropertyType> shortTypes = new HashSet<SerializedPropertyType>()
        {
            SerializedPropertyType.Boolean,
            SerializedPropertyType.Color,
            SerializedPropertyType.Enum,
            SerializedPropertyType.Float,
            SerializedPropertyType.Gradient,
            SerializedPropertyType.Integer,
            SerializedPropertyType.String,
            SerializedPropertyType.LayerMask,
            SerializedPropertyType.ObjectReference,
        };

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
                SerializedProperty value = values.GetArrayElementAtIndex(i);

                if (DrawAsSingleLine(key.propertyType, value.propertyType))
                {
                    height += EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    height += EditorGUI.GetPropertyHeight(key);
                    string keyPropertyPath = key.propertyPath;

                    if (expandState.TryGetValue(keyPropertyPath, out bool expand) && expand)
                    {
                        height += EditorGUI.GetPropertyHeight(value);
                    }
                }

                height += spaceHeight;
            }

            height += EditorGUIUtility.singleLineHeight;

            bool drawDropArea = typeof(UnityEngine.Object).IsAssignableFrom(valueType);
            if (drawDropArea)
            {
                height += EditorGUIUtility.singleLineHeight * 2f;
            }

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
            GUI.Label(rect, $"{label.text} ({keys.arraySize})");

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

            Rect rectNoFolder = rect;
            rectNoFolder.width = rect.width - buttonWidth;

            rect.width = rectNoFolder.width - folderWidth;
            rect.x += folderWidth;

            for (int i = 0; i < count; ++i)
            {
                SerializedProperty key = keys.GetArrayElementAtIndex(i);
                SerializedProperty value = values.GetArrayElementAtIndex(i);

                if (DrawAsSingleLine(key.propertyType, value.propertyType))
                {
                    button.y = rect.y;
                    rectNoFolder.y = rect.y;
                    Rect half = rectNoFolder;
                    half.width = rectNoFolder.width * 0.5f;
                    EditorGUI.PropertyField(half, key, GUIContent.none);
                    half.x += half.width;
                    EditorGUI.PropertyField(half, value, GUIContent.none);

                    rect.y += EditorGUIUtility.singleLineHeight;
                }
                else
                {
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
                        Color backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = lightGray;
                        GUI.Box(rect, GUIContent.none);
                        GUI.backgroundColor = backgroundColor;
                        EditorGUI.PropertyField(rect, value, new GUIContent(value.type), value.hasVisibleChildren);

                        rect.y += heightForValue;
                    }

                    expand = EditorGUI.Toggle(folder, string.Empty, expand, EditorStyles.foldout);
                    expandState[keyPropertyPath] = expand;
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
            bool drawDropArea = typeof(UnityEngine.Object).IsAssignableFrom(valueType);
            if (property.FindPropertyRelative("invalidFlag").boolValue)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.HelpBox(rect, "Duplicate keys exist", MessageType.Error);
            }
            else
            {
                if (drawDropArea)
                {
                    rect.height = EditorGUIUtility.singleLineHeight * 2f;
                    DrawDropArea(rect, property);
                    rect.y += rect.height;
                    rect.height = EditorGUIUtility.singleLineHeight;
                }

                DrawAddButton(rect, property);
            }
        }

        protected void DrawAddButton(Rect position, SerializedProperty property)
        {
            Rect buttonRect = position;
            buttonRect.width = Mathf.Min(position.width, 100);
            buttonRect.x = position.center.x - buttonRect.width * 0.5f;
            if (GUI.Button(buttonRect, "+"))
            {
                SerializedProperty keys = property.FindPropertyRelative("keys");
                SerializedProperty values = property.FindPropertyRelative("values");
                int count = keys.arraySize;
                keys.InsertArrayElementAtIndex(count);
                values.InsertArrayElementAtIndex(count);
            }
        }

        protected void DrawDropArea(Rect position, SerializedProperty property)
        {
            Event evt = Event.current;
            Rect dropArea = position;
            Color guiColor = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(dropArea, "Drop objects hero to add new entries");
            GUI.color = guiColor;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dropArea.Contains(evt.mousePosition) &&
                        DragAndDrop.objectReferences.Any(o => valueType.IsInstanceOfType(o)))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            HashSet<UnityEngine.Object> newObjects = new HashSet<UnityEngine.Object>(DragAndDrop.objectReferences);
                            SerializedProperty values = property.FindPropertyRelative("values");
                            for (int i = 0, size = values.arraySize; i < size; ++i)
                            {
                                newObjects.Remove(values.GetArrayElementAtIndex(i).objectReferenceValue);
                            }

                            int newObjectCount = newObjects.Count;
                            if (newObjectCount > 0)
                            {
                                SerializedProperty keys = property.FindPropertyRelative("keys");
                                int oldSize = keys.arraySize;
                                keys.arraySize += newObjectCount;
                                values.arraySize += newObjectCount;
                                foreach (var newObj in newObjects)
                                {
                                    keys.GetArrayElementAtIndex(oldSize).stringValue = newObj.name;
                                    values.GetArrayElementAtIndex(oldSize).objectReferenceValue = newObj;
                                    ++oldSize;
                                }
                            }
                        }
                    }

                    break;
            }
        }

        private static Type GetValueType(Type propertyType)
        {
            while (propertyType != null)
            {
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                {
                    Type[] genericArgs = propertyType.GenericTypeArguments;
                    if (genericArgs != null && genericArgs.Length == 2)
                    {
                        Type valueType = genericArgs[1];
                        return valueType;
                    }
                }
                else
                {
                    propertyType = propertyType.BaseType;
                }
            }

            return null;
        }

        private static bool DrawAsSingleLine(SerializedPropertyType keyType, SerializedPropertyType valueType)
        {
            return shortTypes.Contains(keyType) && shortTypes.Contains(valueType);
        }
    }
}
