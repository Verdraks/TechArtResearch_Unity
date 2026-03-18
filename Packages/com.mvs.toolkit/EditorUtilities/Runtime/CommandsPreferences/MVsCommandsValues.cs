using System;

namespace MVsToolkit.Preferences.Editor
{
    [Serializable]
    public class MVsCommandsValues
    {
        public bool UseCommands = true;
        public string[] AssemblyNames;

        public MVsCommandsValues()
        {
            UseCommands = false;
            AssemblyNames = new string[]
            {
                "Assembly-CSharp",
                "Assembly-CSharp-firstpass",
                "MVsToolkit.DebugCanvas"
            };
        }
    }
}