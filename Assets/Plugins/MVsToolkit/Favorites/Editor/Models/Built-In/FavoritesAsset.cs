using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    [Serializable]
    public struct FavoritesAsset : IFavoritesElement
    {
        private string m_Guid;

        public FavoritesAsset(string itemGuid)
        {
            m_Guid = itemGuid;
        }

        public bool IsValid() => !string.IsNullOrEmpty(m_Guid);
        public Object GetObject() => !IsValid() ? null : AssetDatabase.LoadAssetByGUID<Object>(new GUID(m_Guid));

        public void Focus()
        {
            Object obj = GetObject();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
