using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public struct DefaultFavoritesCacheElement : IFavoritesCacheElement
    {
        public string Name { get; set; }
        public Object Object { get; set; }
        public Texture2D Preview { get; set; }
    }
}