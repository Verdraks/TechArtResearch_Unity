using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public interface IFavoritesCacheElement
    {
        string Name { get; }
        Object Object { get;}
        Texture2D Preview { get; }
    }
}