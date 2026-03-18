using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    public class MVsCommandsPreferencesTab : IMVsPreferencesTab
    {
        public string TabName => "Commands";
        public Texture Icon => EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image;

        Vector2 _scroll;

        string[] _assemblyNames;

        public void Load()
        {
            var values = MVsPrefs<MVsCommandsValues>.Values;

            if (values.AssemblyNames != null && values.AssemblyNames.Length > 0)
            {
                _assemblyNames = values.AssemblyNames.ToArray();
                return;
            }

            _assemblyNames = Array.Empty<string>();
        }

        public void Save()
        {
            var values = MVsPrefs<MVsCommandsValues>.Values;

            values.AssemblyNames = _assemblyNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray();

            MVsPrefs<MVsCommandsValues>.Save();
        }

        public void Reset()
        {
            MVsPrefs<MVsCommandsValues>.Reset();
            Load();
        }

        public void OnGUI()
        {
            var values = MVsPrefs<MVsCommandsValues>.Values;

            GUILayout.Label("Commands Settings", EditorStyles.boldLabel);

            values.UseCommands = EditorGUILayout.Toggle("Use Commands", values.UseCommands);

            GUILayout.Space(10);

            GUI.enabled = values.UseCommands;

            GUILayout.Label("Included Assemblies (string names)", EditorStyles.boldLabel);

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Min(_assemblyNames.Length * 27, 216)));

            for (int i = 0; i < _assemblyNames.Length; i++)
                DrawAssembly(i);

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(18)))
            {
                Add(ref _assemblyNames, "");
            }

            GUILayout.Space(17);
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        void DrawAssembly(int i)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Zone Asset
            var asmdef = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(g => AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(g)))
                .FirstOrDefault(a => a != null && a.name == _assemblyNames[i]);

            var newAsmdef = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField(
                asmdef,
                typeof(AssemblyDefinitionAsset),
                false
            );

            // Si l'utilisateur choisit un asset → on remplace le string
            if (newAsmdef != asmdef && newAsmdef != null)
                _assemblyNames[i] = newAsmdef.name;

            // Zone texte
            string newName = EditorGUILayout.TextField(_assemblyNames[i]);

            if (newName != _assemblyNames[i])
                _assemblyNames[i] = newName;

            // Bouton delete
            if (GUILayout.Button("Delete", GUILayout.Width(50)))
            {
                RemoveAt(ref _assemblyNames, i);
                GUI.FocusControl(null);

                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                return;
            }

            GUILayout.EndHorizontal();
        }

        void Add(ref string[] arr, string value)
        {
            Array.Resize(ref arr, arr.Length + 1);
            arr[^1] = value;
        }

        void RemoveAt(ref string[] arr, int index)
        {
            arr = arr.Where((a, i) => i != index).ToArray();
        }
    }
}