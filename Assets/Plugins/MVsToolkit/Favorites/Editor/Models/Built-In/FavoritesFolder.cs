using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    [Serializable]
    public struct FavoritesFolder : IFavoritesElement
    {
        private string m_Guid;
        private string m_Path;

        public FavoritesFolder(string itemGuid)
        {
            m_Guid = itemGuid;
            m_Path = AssetDatabase.GUIDToAssetPath(m_Guid);
        }

        public bool IsValid() => !string.IsNullOrEmpty(m_Guid) && 
                                  AssetDatabase.IsValidFolder(m_Path);
        
        public Object GetObject() => !IsValid() ? null : AssetDatabase.LoadAssetAtPath<Object>(m_Path);

        public void Focus()
        {
            Object folderAsset = GetObject();
            if (!folderAsset) return;
            
            ProjectBrowserExtension.ExpandFolder(folderAsset.GetInstanceID());

        }
    }
}
