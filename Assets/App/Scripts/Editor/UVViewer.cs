using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UVViewer : EditorWindow
{
    private Texture2D m_UVTexture;
    private int m_TexSize = 1024;
    private Color m_Background = Color.white;
    private Color m_LineColor = Color.black;
    private bool m_ShowTriangles = true;
    private bool m_ShowVertices = true;
    private float m_PointSize = 3f;
    private Mesh m_ExplicitMesh;
    private bool m_WrapUVs = true;

    // New fields for channel, zoom and pan
    private int m_UVChannel; // 0 = uv, 1 = uv2, ... (initialized to default)
    private Vector2 m_Pan = Vector2.zero;
    private float m_Zoom = 1f;
    private Rect m_LastPreviewRect;

    [MenuItem("Window/UV Viewer")]
    public static void Open()
    {
        GetWindow<UVViewer>("UV Viewer");
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void OnDisable()
    {
        if (m_UVTexture != null)
        {
            DestroyImmediate(m_UVTexture);
            m_UVTexture = null;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("UV Viewer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Sélectionnez un Mesh (asset) ou un GameObject avec MeshFilter/SkinnedMeshRenderer. Vous pouvez aussi glisser un Mesh ici.", MessageType.Info);

        m_ExplicitMesh = (Mesh)EditorGUILayout.ObjectField("Mesh (optionnel)", m_ExplicitMesh, typeof(Mesh), false);

        Mesh mesh = m_ExplicitMesh != null ? m_ExplicitMesh : GetSelectedMeshFromSelection();

        if (mesh == null)
        {
            EditorGUILayout.HelpBox("Aucun mesh sélectionné.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Mesh:", mesh.name);

        // UV channel selector
        m_UVChannel = EditorGUILayout.Popup("UV Channel", m_UVChannel, new[] { "UV0", "UV1", "UV2", "UV3" });

        // check channel presence
        var checkList = new List<Vector2>();
        mesh.GetUVs(m_UVChannel, checkList);
        if (checkList.Count == 0)
        {
            EditorGUILayout.HelpBox($"Le mesh sélectionné n'a pas d'UVs dans le canal {m_UVChannel}.", MessageType.Warning);
        }

        m_TexSize = EditorGUILayout.IntField("Texture Size", m_TexSize);
        m_TexSize = Mathf.Clamp(m_TexSize, 16, 8192);
        m_WrapUVs = EditorGUILayout.Toggle("Wrap UVs (mod 1)", m_WrapUVs);
        m_Background = EditorGUILayout.ColorField("Background", m_Background);
        m_LineColor = EditorGUILayout.ColorField("Line Color", m_LineColor);
        m_ShowTriangles = EditorGUILayout.Toggle("Show triangles", m_ShowTriangles);
        m_ShowVertices = EditorGUILayout.Toggle("Show vertices", m_ShowVertices);
        m_PointSize = EditorGUILayout.Slider("Point Size", m_PointSize, 1f, 16f);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate UV Preview"))
        {
            GenerateAndAssign(mesh);
        }
        if (GUILayout.Button("Fit"))
        {
            FitToView();
        }
        if (GUILayout.Button("Reset"))
        {
            ResetView();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Preview area (zoomable, pannable)
        if (m_UVTexture != null)
        {
            Rect previewRect = GUILayoutUtility.GetRect(10, 10000, 10, 10000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_LastPreviewRect = previewRect;

            HandlePreviewEvents(previewRect);

            EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f));

            float dispW = m_UVTexture.width * m_Zoom;
            float dispH = m_UVTexture.height * m_Zoom;
            Vector2 center = previewRect.center + m_Pan;
            Rect texRect = new Rect(center.x - dispW / 2f, center.y - dispH / 2f, dispW, dispH);

            GUI.DrawTexture(texRect, m_UVTexture, ScaleMode.StretchToFill, false);

            Handles.BeginGUI();
            Handles.color = m_LineColor;
            Vector3[] border = new Vector3[] {
                new Vector3(previewRect.xMin, previewRect.yMin),
                new Vector3(previewRect.xMax, previewRect.yMin),
                new Vector3(previewRect.xMax, previewRect.yMax),
                new Vector3(previewRect.xMin, previewRect.yMax),
                new Vector3(previewRect.xMin, previewRect.yMin)
            };
            Handles.DrawAAPolyLine(2f, border);
            Handles.EndGUI();

            GUI.Label(new Rect(previewRect.x + 6, previewRect.y + 6, 400, 20), $"Zoom: {m_Zoom:F2}  Pan: {m_Pan.x:F0},{m_Pan.y:F0}  Channel: UV{m_UVChannel}");
        }
        else
        {
            if (GUILayout.Button("Generate (auto)"))
            {
                GenerateAndAssign(mesh);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tips:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(" - Utilisez la molette pour zoomer/dézoomer sur la zone d'aperçu.");
        EditorGUILayout.LabelField(" - Cliquez et glissez (molette ou Alt+clic gauche) pour déplacer (pan) la vue.");
        EditorGUILayout.LabelField(" - Double-cliquez dans la zone d'aperçu pour Fit.");
        EditorGUILayout.LabelField(" - Les UVs en dehors de 0..1 seront ramenés dans 0..1 si 'Wrap UVs' est coché.");
        EditorGUILayout.LabelField(" - Pour visualiser le mesh directement, sélectionnez le GameObject dans la hiérarchie.");
    }

    private void HandlePreviewEvents(Rect previewRect)
    {
        Event e = Event.current;
        Vector2 mouse = e.mousePosition;

        // zoom with scroll wheel when mouse is over preview
        if (previewRect.Contains(mouse) && e.type == EventType.ScrollWheel)
        {
            float oldZoom = m_Zoom;
            float delta = -e.delta.y * 0.1f;
            m_Zoom = Mathf.Clamp(m_Zoom * (1f + delta), 0.05f, 20f);

            // keep mouse position stable while zooming
            Vector2 localMouse = mouse - (previewRect.center + m_Pan);
            m_Pan -= localMouse * (m_Zoom / oldZoom - 1f);

            e.Use();
            Repaint();
        }

        // pan: middle mouse drag or alt+left drag
        if (previewRect.Contains(mouse) && e.type == EventType.MouseDrag && (e.button == 2 || (e.button == 0 && e.alt)))
        {
            m_Pan += e.delta;
            e.Use();
            Repaint();
        }

        // double click to fit
        if (previewRect.Contains(mouse) && e.type == EventType.MouseDown && e.clickCount == 2)
        {
            FitToView();
            e.Use();
            Repaint();
        }
    }

    private void FitToView()
    {
        if (m_UVTexture == null) return;
        Rect previewRect = m_LastPreviewRect;
        if (previewRect.width <= 0 || previewRect.height <= 0)
        {
            previewRect = new Rect(10, 80, position.width - 20, position.height - 140);
        }
        float fitX = previewRect.width / m_UVTexture.width;
        float fitY = previewRect.height / m_UVTexture.height;
        m_Zoom = Mathf.Min(fitX, fitY);
        if (m_Zoom <= 0) m_Zoom = 1f;
        m_Pan = Vector2.zero; // center
    }

    private void ResetView()
    {
        m_Zoom = 1f;
        m_Pan = Vector2.zero;
    }

    private Mesh GetSelectedMeshFromSelection()
    {
        var go = Selection.activeGameObject;
        if (go)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) return mf.sharedMesh;
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null) return smr.sharedMesh;
        }

        var obj = Selection.activeObject as Mesh;
        if (obj != null) return obj;
        return null;
    }

    private void GenerateAndAssign(Mesh mesh)
    {
        if (mesh == null) return;

        // get UVs for selected channel
        var uvsList = new List<Vector2>();
        mesh.GetUVs(m_UVChannel, uvsList);
        if (uvsList.Count == 0)
        {
            m_UVTexture = null;
            ShowNotification(new GUIContent($"Mesh has no UVs in channel {m_UVChannel}."));
            return;
        }

        if (m_UVTexture != null)
        {
            DestroyImmediate(m_UVTexture);
            m_UVTexture = null;
        }

        m_UVTexture = GenerateUVTexture(mesh, m_TexSize, m_UVChannel);
        FitToView();
    }

    private Texture2D GenerateUVTexture(Mesh mesh, int size, int channel)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = m_Background;
        tex.SetPixels(cols);

        var uvsList = new List<Vector2>();
        mesh.GetUVs(channel, uvsList);
        var tris = mesh.triangles;

        if (m_ShowTriangles && tris != null && tris.Length >= 3)
        {
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                int i0 = tris[i];
                int i1 = tris[i + 1];
                int i2 = tris[i + 2];
                if (i0 < uvsList.Count && i1 < uvsList.Count && i2 < uvsList.Count)
                {
                    DrawLineOnTexture(tex, MapUV(uvsList[i0]), MapUV(uvsList[i1]), m_LineColor);
                    DrawLineOnTexture(tex, MapUV(uvsList[i1]), MapUV(uvsList[i2]), m_LineColor);
                    DrawLineOnTexture(tex, MapUV(uvsList[i2]), MapUV(uvsList[i0]), m_LineColor);
                }
            }
        }

        if (m_ShowVertices)
        {
            for (int i = 0; i < uvsList.Count; i++)
            {
                DrawPointOnTexture(tex, MapUV(uvsList[i]), m_LineColor, Mathf.RoundToInt(m_PointSize));
            }
        }

        tex.Apply();
        return tex;
    }

    private Vector2 MapUV(Vector2 uv)
    {
        if (m_WrapUVs)
        {
            uv.x = uv.x - Mathf.Floor(uv.x);
            uv.y = uv.y - Mathf.Floor(uv.y);
        }
        return uv;
    }

    // Bresenham line algorithm
    private void DrawLineOnTexture(Texture2D tex, Vector2 uvA, Vector2 uvB, Color col)
    {
        int width = tex.width;
        int height = tex.height;
        int x0 = Mathf.Clamp(Mathf.RoundToInt(uvA.x * (width - 1)), 0, width - 1);
        int y0 = Mathf.Clamp(Mathf.RoundToInt(uvA.y * (height - 1)), 0, height - 1);
        int x1 = Mathf.Clamp(Mathf.RoundToInt(uvB.x * (width - 1)), 0, width - 1);
        int y1 = Mathf.Clamp(Mathf.RoundToInt(uvB.y * (height - 1)), 0, height - 1);

        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            tex.SetPixel(x0, y0, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void DrawPointOnTexture(Texture2D tex, Vector2 uv, Color col, int size)
    {
        int cx = Mathf.RoundToInt(uv.x * (tex.width - 1));
        int cy = Mathf.RoundToInt(uv.y * (tex.height - 1));
        int r = Mathf.Max(1, size / 2);
        for (int x = cx - r; x <= cx + r; x++)
        {
            for (int y = cy - r; y <= cy + r; y++)
            {
                if (x >= 0 && y >= 0 && x < tex.width && y < tex.height)
                {
                    tex.SetPixel(x, y, col);
                }
            }
        }
    }
}
