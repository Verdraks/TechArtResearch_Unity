using System;
using System.Collections.Generic;

namespace MVsToolkit.Favorites.Editor
{
    [Serializable]
    public class FavoritesData
    {
        public List<FavoritesGroup> FavoritesGroups = new ();
    }
}