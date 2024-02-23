// -----------------------------------------------------------------------
// <copyright file="StringToObjectDictionary.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;

    /// <summary>
    /// Represents a serializable dictionary that maps strings to UnityEngine.Objects.
    /// </summary>
    [Serializable]
    public class StringToObjectDictionary
        : SerializableDictionary<string, UnityEngine.Object>
    {
    }
}
