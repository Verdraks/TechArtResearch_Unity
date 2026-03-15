using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public static class FactoryFavoritesCacheElement
    {
        public static IFavoritesCacheElement Create(IFavoritesElement element)
        {
            Debug.Assert(element != null && element.IsValid());
            Object obj = element.GetObject();
            
            Texture2D preview = AssetPreview.GetAssetPreview(obj) 
                                ?? AssetPreview.GetMiniThumbnail(obj) 
                                ?? AssetPreview.GetMiniTypeThumbnail(obj.GetType());
            
            IFavoritesCacheElement cachedElement = new DefaultFavoritesCacheElement
            {
                Name = obj.name,
                Object = obj,
                Preview = preview
            };
            
            return cachedElement;
        }
    }
}