using System;
using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    [Serializable]
    public class MVsHierarchyValues
    {
        public bool DrawFolderIcon = true;
        public bool DrawFirstComponentIcon = true;
        public bool DrawComponentsIcon = true;

        public bool DrawZebraMod = true;
        public bool DrawChildLines = true;

        public string ZebraModBlackColor = "#353535";
        public string ZebraModWhiteColor = "#BFBFBF";
        public string PrefabColor = "#8CC7FF";
        public string MissingPrefabColor = "#FF6767";

        public Color BackgroundColor(bool isOdd = false)
        {
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ?
                            ZebraModBlackColor : ZebraModWhiteColor,
                            out Color c);
            Color color = c;

            if (EditorGUIUtility.isProSkin)
                return (DrawZebraMod && isOdd) ?
                    color : new Color(0.219f, 0.219f, 0.219f);
            else
                return (DrawZebraMod && isOdd) ?
                     color : new Color(0.7843138f, 0.7843138f, 0.7843138f);
        }
    }
}