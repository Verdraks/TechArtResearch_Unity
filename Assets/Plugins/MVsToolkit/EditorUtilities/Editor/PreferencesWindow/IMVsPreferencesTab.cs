using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    public interface IMVsPreferencesTab
    {
        string TabName { get; }
        Texture Icon { get; }

        void OnGUI();
        void Load();
        void Save();

        void Reset();
    }
}