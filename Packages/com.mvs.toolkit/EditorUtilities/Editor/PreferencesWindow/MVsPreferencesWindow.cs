using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Preferences.Editor
{
    public class MVsPreferencesWindow : EditorWindow
    {
        List<IMVsPreferencesTab> _tabs = new();
        int _selectedTab = 0;
        Vector2 _scroll;

        [MenuItem("Tools/MVsToolkit/Preferences Window")]
        public static void Open()
        {
            var window = GetWindow<MVsPreferencesWindow>("MVs Preferences");
            window.Show();
        }

        void OnEnable()
        {
            LoadTabs();
        }

        void LoadTabs()
        {
            _tabs.Clear();

            var types = TypeCache.GetTypesDerivedFrom<IMVsPreferencesTab>();

            foreach (var t in types)
            {
                if (t.IsAbstract) continue;

                IMVsPreferencesTab tab = (IMVsPreferencesTab)Activator.CreateInstance(t);
                tab.Load();
                _tabs.Add(tab);
            }
        }

        void OnGUI()
        {
            if (_tabs.Count == 0)
            {
                EditorGUILayout.HelpBox("No preference tabs found.", MessageType.Info);
                return;
            }

            DrawToolbar();
            DrawSelectedTab();
        }

        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                GUIContent content = new GUIContent(tab.TabName, tab.Icon);

                if (GUILayout.Toggle(_selectedTab == i, content, EditorStyles.toolbarButton))
                    _selectedTab = i;
            }

            GUILayout.EndHorizontal();
        }

        void DrawSelectedTab()
        {
            _scroll = GUILayout.BeginScrollView(_scroll);
            _tabs[_selectedTab].OnGUI();
            GUILayout.EndScrollView();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Commands Preferences",
                    $"Are you sure you want to reset all {_tabs[_selectedTab].TabName} preferences to their default values?",
                    "Reset",
                    "Cancel"))
                {
                    _tabs[_selectedTab].Reset();
                    Debug.Log($"[MVsToolkit] {_tabs[_selectedTab].TabName} preferences reset to default values.");
                }
            }
            if (GUILayout.Button("Save", GUILayout.Height(25)))
            {
                foreach (var tab in _tabs)
                    tab.Save();

                Debug.Log("[MVsToolkit] Preferences saved.");
            }
            GUILayout.EndHorizontal();
        }
    }
}