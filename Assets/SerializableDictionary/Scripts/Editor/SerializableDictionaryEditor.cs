using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    [CustomEditor(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

}
