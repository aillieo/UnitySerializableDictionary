// -----------------------------------------------------------------------
// <copyright file="SerializableDictionaryEditor.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    internal class SerializableDictionaryEditor : PropertyDrawer
    {
        private static readonly float spaceHeight = EditorGUIUtility.singleLineHeight * 0.5f;
        private static readonly float dropAreaHeight = EditorGUIUtility.singleLineHeight * 2f;
        private static readonly Color lightGray = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static readonly float emptyHeight = 24f;

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

        private SerializedProperty serializedProperty;

        private static readonly Dictionary<string, WeakReference<ReorderableList>> listCache = new Dictionary<string, WeakReference<ReorderableList>>();
        private static readonly Dictionary<string, Type> valueTypeCache = new Dictionary<string, Type>();

        private Type GetValueTypeByProperty(SerializedProperty property)
        {
            var key = property.propertyPath;
            if (!valueTypeCache.TryGetValue(key, out var valueType))
            {
                valueType = GetValueType(property.serializedObject.targetObject.GetType());
                valueTypeCache[key] = valueType;
            }

            return valueType;
        }

        private ReorderableList GetReorderableListByProperty(SerializedProperty property)
        {
            var key = property.propertyPath;
            if (!listCache.TryGetValue(key, out var weakList))
            {
                var newlist = this.CreateReorderableList(property);
                weakList = new WeakReference<ReorderableList>(newlist);
                listCache[key] = weakList;
            }

            if (weakList.TryGetTarget(out var list) && list != null && list.serializedProperty != null && list.serializedProperty.serializedObject == property.serializedObject)
            {
                return list;
            }
            else
            {
                list = this.CreateReorderableList(property);
                weakList.SetTarget(list);
            }

            return list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.serializedProperty = property;

            if (!this.serializedProperty.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var reorderableList = this.GetReorderableListByProperty(property);

            SerializedProperty keys = property.FindPropertyRelative("keys");
            SerializedProperty values = property.FindPropertyRelative("values");

            var count = keys.arraySize;

            float height = 0;
            height += EditorGUIUtility.singleLineHeight;

            height += this.GetHeightForAllPairs(reorderableList);

            height += spaceHeight;

            var valueType = this.GetValueTypeByProperty(property);
            var drawDropArea = typeof(UnityEngine.Object).IsAssignableFrom(valueType);
            if (drawDropArea)
            {
                height += dropAreaHeight;
            }

            var selected = reorderableList.index;
            if (selected >= 0 && selected < count)
            {
                SerializedProperty key = keys.GetArrayElementAtIndex(selected);
                SerializedProperty value = values.GetArrayElementAtIndex(selected);
                var drawAsSingleLine = DrawAsSingleLine(key.propertyType, value.propertyType);
                if (!drawAsSingleLine)
                {
                    height += spaceHeight;
                    height += EditorGUI.GetPropertyHeight(value, GUIContent.none, value.isExpanded);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            this.serializedProperty = property;

            GUI.Box(position, GUIContent.none);

            SerializedProperty keys = property.FindPropertyRelative("keys");
            SerializedProperty values = property.FindPropertyRelative("values");

            var count = keys.arraySize;

            Rect rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            this.serializedProperty.isExpanded = GUI.Toggle(rect, this.serializedProperty.isExpanded, $"{label.text} ({count})", "BoldLabel");
            if (!this.serializedProperty.isExpanded)
            {
                return;
            }

            var reorderableList = this.GetReorderableListByProperty(property);

            rect.y += EditorGUIUtility.singleLineHeight;

            reorderableList.DoList(rect);

            rect.y += this.GetHeightForAllPairs(reorderableList);

            var valueType = this.GetValueTypeByProperty(property);
            var drawDropArea = typeof(UnityEngine.Object).IsAssignableFrom(valueType);

            if (property.FindPropertyRelative("invalidFlag").boolValue)
            {
                // rect.height = EditorGUIUtility.singleLineHeight;
                // EditorGUI.HelpBox(rect, "Duplicate keys exist", MessageType.Error);
            }

            if (drawDropArea)
            {
                rect.y += spaceHeight;
                rect.height = dropAreaHeight;
                this.DrawDropArea(rect, property);
                rect.y += dropAreaHeight;
            }

            var selected = reorderableList.index;
            if (selected >= 0 && selected < count)
            {
                SerializedProperty key = keys.GetArrayElementAtIndex(selected);
                SerializedProperty value = values.GetArrayElementAtIndex(selected);
                var drawAsSingleLine = DrawAsSingleLine(key.propertyType, value.propertyType);
                if (!drawAsSingleLine)
                {
                    rect.y += spaceHeight;
                    rect.height = EditorGUI.GetPropertyHeight(value, GUIContent.none, value.isExpanded);
                    EditorGUI.PropertyField(rect, value, GUIContent.none, value.isExpanded);
                }
            }

            EditorGUI.EndProperty();
        }

        protected void DrawDropArea(Rect position, SerializedProperty property)
        {
            Event evt = Event.current;
            Rect dropArea = position;
            Color guiColor = GUI.color;
            GUI.color = Color.gray;
            GUI.Box(dropArea, "Drop objects here to add new entries", new GUIStyle("box") { alignment = TextAnchor.MiddleCenter });
            GUI.color = guiColor;

            var valueType = this.GetValueTypeByProperty(property);

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

                        var newObjects = new HashSet<UnityEngine.Object>(DragAndDrop.objectReferences);
                        SerializedProperty values = property.FindPropertyRelative("values");
                        for (int i = 0, size = values.arraySize; i < size; ++i)
                        {
                            newObjects.Remove(values.GetArrayElementAtIndex(i).objectReferenceValue);
                        }

                        var newObjectCount = newObjects.Count;
                        if (newObjectCount > 0)
                        {
                            SerializedProperty keys = property.FindPropertyRelative("keys");
                            var oldSize = keys.arraySize;
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

        private ReorderableList CreateReorderableList(SerializedProperty property)
        {
            var reorderableList = new ReorderableList(this.serializedProperty.serializedObject, this.serializedProperty.FindPropertyRelative("keys"));
            reorderableList.drawElementCallback = this.DrawElementCallback;
            reorderableList.elementHeightCallback = this.ElementHeightCallback;
            reorderableList.onAddCallback = this.OnAddCallback;
            reorderableList.onRemoveCallback = this.OnRemoveCallback;
            reorderableList.onReorderCallbackWithDetails = this.OnReorderCallbackWithDetails;
            reorderableList.headerHeight = 1f;
            reorderableList.drawHeaderCallback = this.DrawHeaderCallback;
            return reorderableList;
        }

        private float ElementHeightCallback(int index)
        {
            SerializedProperty keys = this.serializedProperty.FindPropertyRelative("keys");
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");

            if (index >= keys.arraySize || index >= values.arraySize)
            {
                return 0;
            }

            SerializedProperty key = keys.GetArrayElementAtIndex(index);
            SerializedProperty value = values.GetArrayElementAtIndex(index);

            return this.GetHeightForPair(key, value);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty keys = this.serializedProperty.FindPropertyRelative("keys");
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");
            SerializedProperty key = keys.GetArrayElementAtIndex(index);
            SerializedProperty value = values.GetArrayElementAtIndex(index);

            if (DrawAsSingleLine(key.propertyType, value.propertyType))
            {
                Rect half = rect;
                half.width = rect.width * 0.5f;
                EditorGUI.PropertyField(half, key, GUIContent.none);
                half.x += half.width;
                EditorGUI.PropertyField(half, value, GUIContent.none);
            }
            else
            {
                var heightForKey = EditorGUI.GetPropertyHeight(key);

                rect.height = heightForKey;
                EditorGUI.PropertyField(rect, key, GUIContent.none);

                rect.y += heightForKey;
                rect.height = heightForKey;
            }
        }

        private void OnAddCallback(ReorderableList list)
        {
            SerializedProperty keys = this.serializedProperty.FindPropertyRelative("keys");
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");
            var count = keys.arraySize;
            keys.InsertArrayElementAtIndex(count);
            values.InsertArrayElementAtIndex(count);
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            SerializedProperty keys = this.serializedProperty.FindPropertyRelative("keys");
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");
            var index = list.index;
            var last = keys.arraySize - 1;

            if (index != last)
            {
                keys.MoveArrayElement(index, last);
                values.MoveArrayElement(index, last);
            }
            else
            {
                list.index = last - 1;
            }

            values.DeleteArrayElementAtIndex(last);
            keys.DeleteArrayElementAtIndex(last);
        }

        private void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");
            values.MoveArrayElement(oldIndex, newIndex);
        }

        private void DrawHeaderCallback(Rect rect)
        {
        }

        private float GetHeightForPair(SerializedProperty key, SerializedProperty value)
        {
            if (DrawAsSingleLine(key.propertyType, value.propertyType))
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(key);
            }
        }

        private float GetHeightForAllPairs(ReorderableList reorderableList)
        {
            var height = 0f;
            SerializedProperty keys = this.serializedProperty.FindPropertyRelative("keys");
            SerializedProperty values = this.serializedProperty.FindPropertyRelative("values");

            height += reorderableList.headerHeight;

            var count = keys.arraySize;

            if (count != 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    SerializedProperty key = keys.GetArrayElementAtIndex(i);
                    SerializedProperty value = values.GetArrayElementAtIndex(i);
                    height += this.GetHeightForPair(key, value);
                    if (i != 0)
                    {
                        height += EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
            else
            {
                height += emptyHeight;
            }

            height += reorderableList.footerHeight;
            return height;
        }
    }
}
