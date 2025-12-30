using UnityEngine;
using UnityEditor;

public class BatchRenameWindow : EditorWindow
{
    private string m_BaseName = "Object";
    private string m_Prefix = "";
    private string m_Suffix = "";
    private int m_StartIndex = 1;
    private int m_Increment = 1;
    private bool m_SortByHierarchy = true;

    [MenuItem("Tools/Batch Rename %#r")] // Ctrl/Cmd + Shift + R
    private static void ShowWindow()
    {
        BatchRenameWindow window = GetWindow<BatchRenameWindow>("Batch Rename");
        window.minSize = new Vector2(300, 200);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        m_BaseName = EditorGUILayout.TextField("Base Name", m_BaseName);
        m_Prefix = EditorGUILayout.TextField("Prefix", m_Prefix);
        m_Suffix = EditorGUILayout.TextField("Suffix", m_Suffix);

        EditorGUILayout.Space();
        m_StartIndex = EditorGUILayout.IntField("Start Index", m_StartIndex);
        m_Increment = EditorGUILayout.IntField("Increment", m_Increment);

        m_SortByHierarchy = EditorGUILayout.Toggle("Sort by Hierarchy", m_SortByHierarchy);

        EditorGUILayout.Space();

        if (GUILayout.Button("Rename Selected Objects"))
        {
            RenameSelectedObjects();
        }
    }

    private void RenameSelectedObjects()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Batch Rename", "No objects selected.", "OK");
            return;
        }

        Undo.RecordObjects(selected, "Batch Rename Objects");

        if (m_SortByHierarchy)
        {
            System.Array.Sort(selected, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        }

        int index = m_StartIndex;
        foreach (var go in selected)
        {
            go.name = $"{m_Prefix}{m_BaseName}{index}{m_Suffix}";
            index += m_Increment;
            EditorUtility.SetDirty(go);
        }

        EditorUtility.DisplayDialog("Batch Rename", $"Renamed {selected.Length} objects.", "OK");
    }
}
