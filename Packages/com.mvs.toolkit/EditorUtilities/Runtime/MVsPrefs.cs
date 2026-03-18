using System.IO;
using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    public static class MVsPrefs<T> where T : class, new()
    {
        private static T _values;

        private static string FilePath =>
            Path.Combine("ProjectSettings", $"MVsToolkit_{typeof(T).Name}.json");

        public static T Values
        {
            get
            {
                if (_values == null)
                    _values = LoadInternal();

                return _values;
            }
        }

        public static void Save()
        {
            if (_values == null)
                _values = new T();

            string json = JsonUtility.ToJson(_values, true);
            File.WriteAllText(FilePath, json);
        }

        public static void Reload()
        {
            _values = LoadInternal();
        }

        public static void Reset()
        {
            _values = new T();
            Save();
        }

        private static T LoadInternal()
        {
            if (!File.Exists(FilePath))
            {
                var def = new T();
                File.WriteAllText(FilePath, JsonUtility.ToJson(def, true));
                return def;
            }

            string json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<T>(json);
        }
    }
}