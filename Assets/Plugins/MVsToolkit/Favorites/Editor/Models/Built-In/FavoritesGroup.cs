using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    [Serializable]
    public class FavoritesGroup
    {
        public const string K_DefaultName = "New Folder";
        
        #region Fields

        public string Name = K_DefaultName;
        public Color Color = Color.white;
        public List<IFavoritesElement> Elements = new();

        #endregion
    }
}