using System;
using AillieoUtils;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [Serializable]
    public class StringToString : SerializableDictionary<string, string>
    {
    }

    [Serializable]
    public class StringToObject : SerializableDictionary<string, UnityEngine.Object>
    {
    }

    [Serializable]
    public class Vector2ToAnimationCurve : SerializableDictionary<Vector2, AnimationCurve>
    {
    }

    [Serializable]
    public class Vector2ToObject : SerializableDictionary<Vector2, UnityEngine.Object>
    {
    }

    [Serializable]
    public class Vector4ToAnimationCurve : SerializableDictionary<Vector4, AnimationCurve>
    {
    }

    [Serializable]
    public class Vector4ToObject : SerializableDictionary<Vector4, UnityEngine.Object>
    {
    }

    [Serializable]
    public class Vector4ToVector4 : SerializableDictionary<Vector4, Vector4>
    {
    }

    public StringToString stringToString;
    public StringToObject stringToObject;
    public Vector2ToAnimationCurve vector2ToAnimationCurve;
    public Vector2ToObject vector2ToObject;
    public Vector4ToAnimationCurve vector4ToAnimationCurve;
    public Vector4ToObject vector4ToObject;
    public Vector4ToVector4 vector4ToVector4;
}
