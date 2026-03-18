using System.Collections;
using System.Reflection;
using UnityEditor;

namespace MVsToolkit.ComponentHeaderCleaner.Editor
{
    public class ComponentHeaderCleaner
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update += InitHeader;
        }

        private static void InitHeader()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

            FieldInfo fieldInfo = typeof(EditorGUIUtility).GetField("s_EditorHeaderItemsMethods", flags);
            IList value = (IList)fieldInfo.GetValue(null);
            if (value == null) return;

            value.Clear();

            EditorApplication.update -= InitHeader;
        }
    }
}