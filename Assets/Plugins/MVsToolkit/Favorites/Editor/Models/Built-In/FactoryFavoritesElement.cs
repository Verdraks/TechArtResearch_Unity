using System;
using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Favorites.Editor
{
    public static class FactoryFavoritesElement
    {
        public static IFavoritesElement Create(FavoritesElementContext context)
        {
            IFavoritesElement element;
            
            switch (context.ObjectTarget)
            {
                case GameObject go:
                {
                    string scenePath = go.scene.path;
                    element = new FavoritesSceneObject(scenePath, string.Empty);
                }  
                    break;
                case DefaultAsset when AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(context.ObjectTarget)):
                    element = new FavoritesFolder(context.Guid);
                    break;
                default:
                    element = new FavoritesAsset(context.Guid);
                    break;
            }
            
            Debug.Assert(element != null);
            return element;
        }
        
        public struct FavoritesElementContext
        {
            public UnityEngine.Object ObjectTarget;
            public string Guid;
        }
    }
}