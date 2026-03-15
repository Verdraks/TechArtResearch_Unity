
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    public interface IFavoritesElement
    {
        public bool IsValid() { return true; }
        public Object GetObject() { return null; }
        public void Focus() { }
    }
}