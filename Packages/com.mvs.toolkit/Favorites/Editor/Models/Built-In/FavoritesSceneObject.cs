using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MVsToolkit.Favorites.Editor
{
    [Serializable]
    public struct FavoritesSceneObject : IFavoritesElement
    {
        private string m_ObjectPathInScene;
        private string m_ScenePath;

        public FavoritesSceneObject(string scenePath, string objectPathInScene)
        {
            m_ScenePath = scenePath;
            m_ObjectPathInScene = objectPathInScene;
        }
        
        public bool IsValid()
        {
            Scene scene = SceneManager.GetSceneByPath(m_ScenePath);
            if (!scene.IsValid()) return false;
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                Transform targetObj = rootObj.transform.Find(m_ObjectPathInScene);
                if (!targetObj) continue;
                return true;
            }
            return false;
        }

        public Object GetObject()
        {
            //TODO: Handle ref gameobject in unloaded scene
            Scene scene = SceneManager.GetSceneByPath(m_ScenePath);
            if (!scene.IsValid()) return null;
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var rootObj in rootObjects)
            {
                Transform targetObj = rootObj.transform.Find(m_ObjectPathInScene);
                if (targetObj == null) continue;
                return targetObj.gameObject;
            }
            return null;
        }

        public void Focus()
        {
            Object obj = GetObject();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}