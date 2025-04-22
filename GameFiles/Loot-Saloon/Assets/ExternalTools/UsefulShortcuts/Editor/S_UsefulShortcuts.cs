using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

static class S_UsefulShortcuts
{
    // Alt + C
    [Shortcut("Clear Console", KeyCode.C, ShortcutModifiers.Alt)]
    public static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");

        method.Invoke(new object(), null);
    }
}