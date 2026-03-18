using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    /// <summary>
    /// Static utility class that encapsulates all reflection-based interactions
    /// with Unity's internal ProjectBrowser window.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBrowserExtension
    {
        #region Constants - ProjectBrowserRender
        private const int k_ScrollBarPaddingOneCollumLayout = 12;
        #endregion
        
        #region Constants - Reflection Names

        private const string k_TypeNameProjectBrowser = "UnityEditor.ProjectBrowser";
        private const string k_FieldNameTreeViewRect = "m_TreeViewRect";
        private const string k_FieldNameViewMode = "m_ViewMode";
        private const string k_FieldNameAssetTree = "m_AssetTree";
        private const string k_PropertyNameDataTree = "data";
        private const string k_PropertyNameShowingVerticalScroll = "showingVerticalScrollBar";
        private const string k_MethodNameSetFolderSelection = "SetFolderSelection";
        private const string k_MethodNameSetExpanded = "SetExpanded";

        #endregion

        #region Static Fields - Reflection Cache

        private static readonly Type s_ProjectBrowserType;
        private static readonly FieldInfo s_TreeViewRectField;
        private static readonly FieldInfo s_ViewModeField;
        private static readonly FieldInfo s_AssetTreeField;
        private static readonly PropertyInfo s_DataTreeProperty;
        private static readonly PropertyInfo s_ShowingVerticalScrollProperty;
        private static readonly MethodInfo s_SetFolderSelectionMethod;
        private static readonly MethodInfo s_SetExpandedMethod;

        #endregion

        #region Static Fields - State Cache

        private static EditorWindow s_ProjectBrowserWindow;

        #endregion

        #region Initialization

        static ProjectBrowserExtension()
        {
            s_ProjectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType(k_TypeNameProjectBrowser);

            if (s_ProjectBrowserType == null)
            {
                Debug.LogWarning("[ProjectBrowserExtension] Could not find ProjectBrowser type.");
                return;
            }

            s_TreeViewRectField = s_ProjectBrowserType.GetField(
                k_FieldNameTreeViewRect,
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            s_ViewModeField = s_ProjectBrowserType.GetField(
                k_FieldNameViewMode,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            s_AssetTreeField = s_ProjectBrowserType.GetField(
                k_FieldNameAssetTree,
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            s_DataTreeProperty = s_AssetTreeField?.FieldType.GetProperty(
                k_PropertyNameDataTree,
                BindingFlags.Public | BindingFlags.Instance
            );
            
            s_ShowingVerticalScrollProperty = s_AssetTreeField?.FieldType.GetProperty(
                k_PropertyNameShowingVerticalScroll,
                BindingFlags.Public | BindingFlags.Instance
            );

            s_SetFolderSelectionMethod = s_ProjectBrowserType.GetMethod(
                k_MethodNameSetFolderSelection,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                null,
                new[] { typeof(int[]), typeof(bool) },
                null
            );

            s_SetExpandedMethod = s_DataTreeProperty?.PropertyType.GetMethod(
                k_MethodNameSetExpanded,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                null,
                new[] { typeof(int), typeof(bool) },
                null
            );
        }

        #endregion

        #region Public Properties


        public static bool ProjectBrowserOpen
        {
            get
            {
                Object[] potentialWindows = Resources.FindObjectsOfTypeAll(s_ProjectBrowserType);
                return potentialWindows is { Length: > 0 };
            }
        }
        
        public static EditorWindow ProjectBrowserWindow
        {
            get
            {
                if (!s_ProjectBrowserWindow)
                {
                    s_ProjectBrowserWindow = GetProjectBrowser();
                }
                return s_ProjectBrowserWindow;
            }
        }

        #endregion

        #region Public Methods - Window Access

        private static EditorWindow GetProjectBrowser()
        {
            return s_ProjectBrowserType != null ? EditorWindow.GetWindow(s_ProjectBrowserType) : null;
        }

        /// <summary>
        /// Return value enum View Mode of Project Browser 
        /// </summary>
        /// <returns>0 == One Collum, 1 == Two Collum</returns>
        private static int GetViewMode()
        {
            if (ProjectBrowserWindow == null || s_ViewModeField == null)
                return -1;

            return (int)s_ViewModeField.GetValue(ProjectBrowserWindow);
        }
        

        public static Rect GetViewRect()
        {
            if (ProjectBrowserWindow == null)
                return Rect.zero;

            Rect treeViewRect = (Rect)s_TreeViewRectField.GetValue(ProjectBrowserWindow);
            int viewMode = GetViewMode();
            
            if (viewMode == 0) 
            {
                // Instead of passing the FieldInfo, we need the asset tree instance retrieved from the ProjectBrowser window.
                bool showingVerticalScroll = (bool)s_ShowingVerticalScrollProperty.GetValue(s_AssetTreeField.GetValue(ProjectBrowserWindow));
                
                return new Rect(
                    treeViewRect.x + treeViewRect.width / 2,
                    treeViewRect.y,
                    treeViewRect.width / 2 - (showingVerticalScroll ?  k_ScrollBarPaddingOneCollumLayout : 0) ,
                    treeViewRect.height
                );
            }
            
            return treeViewRect;
        }

        #endregion

        #region Public Methods - Folder Navigation

        public static void ExpandFolder(int instanceID)
        {
            if (ProjectBrowserWindow == null)
                return;

            int viewMode = GetViewMode();

            if (viewMode == 1) // TwoColumns
            {
                SetFolderSelection(instanceID);
            }
            else // OneColumn
            {
                SetTreeViewExpanded(instanceID, true);
            }
        }

        /// <summary>
        /// Sets the folder selection in the ProjectBrowser (TwoColumns mode).
        /// </summary>
        private static void SetFolderSelection(int instanceID, bool revealAndFrameInFolderTree = false)
        { 
            s_SetFolderSelectionMethod?.Invoke(ProjectBrowserWindow, new object[] { new[]{instanceID}, revealAndFrameInFolderTree });
        }

        /// <summary>
        /// Sets the expanded state of an item in the tree view (OneColumn mode).
        /// </summary>
        private static void SetTreeViewExpanded(int instanceID, bool expanded)
        {
            if (ProjectBrowserWindow == null || s_AssetTreeField == null || s_DataTreeProperty == null || s_SetExpandedMethod == null)
                return;

            object assetTree = s_AssetTreeField.GetValue(ProjectBrowserWindow);
            if (assetTree == null) return;

            object dataTree = s_DataTreeProperty.GetValue(assetTree);
            if (dataTree == null) return;

            s_SetExpandedMethod.Invoke(dataTree, new object[] { instanceID, expanded });
            EditorGUIUtility.PingObject(instanceID);
        }

        #endregion
    }
}
