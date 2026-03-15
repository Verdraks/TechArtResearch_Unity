using System.Collections.Generic;
using UnityEngine;

namespace MVsToolkit.SceneBrowser.Editor
{
    public class SceneBrowserDatabase : ScriptableObject
    {
        public List<SceneBrowerData> scenes = new();
    }
}