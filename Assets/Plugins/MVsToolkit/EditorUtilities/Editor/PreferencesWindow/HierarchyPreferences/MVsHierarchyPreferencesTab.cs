using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    public class MVsHierarchyPreferencesTab : IMVsPreferencesTab
    {
        public string TabName => "Hierarchy";
        public Texture Icon => EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image;

        public void Load() { }

        public void Save()
        {
            MVsPrefs<MVsHierarchyValues>.Save();
        }

        public void Reset()
        {
            MVsPrefs<MVsHierarchyValues>.Reload();
        }

        public void OnGUI()
        {
            GUILayout.Label("Hierarchy Settings", EditorStyles.boldLabel);

            MVsPrefs<MVsHierarchyValues>.Values.DrawFolderIcon = EditorGUILayout.Toggle("Draw Folder Icon", MVsPrefs<MVsHierarchyValues>.Values.DrawFolderIcon);
            MVsPrefs<MVsHierarchyValues>.Values.DrawFirstComponentIcon = EditorGUILayout.Toggle("Draw First Component Icon", MVsPrefs<MVsHierarchyValues>.Values.DrawFirstComponentIcon);
            MVsPrefs<MVsHierarchyValues>.Values.DrawComponentsIcon = EditorGUILayout.Toggle("Draw Components Icon", MVsPrefs<MVsHierarchyValues>.Values.DrawComponentsIcon);

            GUILayout.Space(10);

            MVsPrefs<MVsHierarchyValues>.Values.DrawZebraMod = EditorGUILayout.Toggle("Draw Zebra Mod", MVsPrefs<MVsHierarchyValues>.Values.DrawZebraMod);
            MVsPrefs<MVsHierarchyValues>.Values.DrawChildLines = EditorGUILayout.Toggle("Draw Child Lines", MVsPrefs<MVsHierarchyValues>.Values.DrawChildLines);

            GUILayout.Space(10);

            MVsPrefs<MVsHierarchyValues>.Values.ZebraModBlackColor = DrawColor("Zebra Black", MVsPrefs<MVsHierarchyValues>.Values.ZebraModBlackColor);
            MVsPrefs<MVsHierarchyValues>.Values.ZebraModWhiteColor = DrawColor("Zebra White", MVsPrefs<MVsHierarchyValues>.Values.ZebraModWhiteColor);
            MVsPrefs<MVsHierarchyValues>.Values.PrefabColor = DrawColor("Prefab Color", MVsPrefs<MVsHierarchyValues>.Values.PrefabColor);
            MVsPrefs<MVsHierarchyValues>.Values.MissingPrefabColor = DrawColor("Missing Prefab Color", MVsPrefs<MVsHierarchyValues>.Values.MissingPrefabColor);
        }

        string DrawColor(string label, string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var c))
                c = Color.white;

            Color newColor = EditorGUILayout.ColorField(label, c);
            return "#" + ColorUtility.ToHtmlStringRGB(newColor);
        }
    }
}