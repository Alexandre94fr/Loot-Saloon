using System;
using UnityEngine;

/// <summary>
/// This script doesn't follow the group convention about prefixing any script with S_... for the reason that is not nice to have a [S_ReadOnlyInInspector]. </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyInInspectorAttribute : PropertyAttribute
{
    // Don't need to put things in here, the logic is in the S_ReadOnlyCustomDrawer
}