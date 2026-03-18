using Object = UnityEngine.Object;
using UnityEditor;

namespace MVsToolkit.Favorites.Editor
{
    public class FavoritesService
    {
        public FavoritesStorage Storage => m_Storage;
        private FavoritesStorage m_Storage;

        public void Init()
        {
            m_Storage = new FavoritesStorage();
            Storage.Load();
            VerifyData();
        }

        public void Shutdown()
        {
            m_Storage.Save();
        }

        private void VerifyData()
        {
            foreach (FavoritesGroup folder in m_Storage.FavoritesGroups)
            {
                foreach (IFavoritesElement element in folder.Elements.ToArray())
                {
                    if (element == null || !element.IsValid())
                    {
                        folder.Elements.Remove(element);
                    }
                }
            }
            m_Storage.Save();
        }

        public void FocusElement(IFavoritesElement element)
        {
            if (element == null || !element.IsValid()) return;
            element.Focus();
        }

        public void CreateGroup(string name = null)
        {
            if (string.IsNullOrEmpty(name)) name = FavoritesGroup.K_DefaultName;

            FavoritesGroup group = new()
            {
                Name = name,
                Color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f)
            };
            m_Storage.FavoritesGroups.Add(group);
            m_Storage.Save();
        }
        
        public void LoadGroup(FavoritesGroup group)
        {
            m_Storage.ClearCache();
            m_Storage.CacheGroup(group);
        }
        
        public void DeleteGroup(FavoritesGroup group)
        {
            m_Storage.FavoritesGroups.Remove(group);
            m_Storage.Save();
        }

        public void AddItem(FavoritesGroup group, Object obj)
        {
            
            if (group == null)
            {
                CreateGroup();
                group = m_Storage.FavoritesGroups[^1];
            }
            
            if (m_Storage.ContainElement(group, obj)) return;
            
            FactoryFavoritesElement.FavoritesElementContext ctx = new()
            {
                ObjectTarget = obj,
                Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj))
            };

            IFavoritesElement element = FactoryFavoritesElement.Create(ctx);

            group.Elements.Add(element);
            
            m_Storage.CacheGroup(group);
            
            m_Storage.Save();
        }

        public void DeleteItem(FavoritesGroup group, IFavoritesElement element)
        {
            group.Elements.Remove(element);
            m_Storage.Save();
        }
        
    }
}

