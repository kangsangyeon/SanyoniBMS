#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace SanyoniLib.UnityEditorHelper
{

    public static class UnityHelper
    {
        // Ctrl + Alt + Shift + C
        [Shortcut("Clear Console", KeyCode.C, ShortcutModifiers.Action | ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }

}

#endif