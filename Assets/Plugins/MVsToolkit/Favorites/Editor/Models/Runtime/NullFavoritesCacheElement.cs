using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public struct NullFavoritesCacheElement : IFavoritesCacheElement
    {
        public string Name => string.Empty;
        public Object Object => null;
        public Texture2D Preview => Texture2D.blackTexture;
    }
}