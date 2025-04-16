using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
public class ReadOnlyInInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect p_position, SerializedProperty p_serializedProperty, GUIContent p_label)
    {
        EditorGUI.BeginDisabledGroup(true);

        EditorGUI.PropertyField(p_position, p_serializedProperty, p_label, true);

        EditorGUI.EndDisabledGroup();
    }
}