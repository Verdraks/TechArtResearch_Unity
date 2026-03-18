using UnityEngine;

namespace MVsToolkit.Demo.Spreadsheet
{
    [CreateAssetMenu(fileName = "SSO_ItemData", menuName = "MVsToolkit/Demo/SSO_ItemData")]
    internal class SSO_ItemData : ScriptableObject
    {
        public string itemName;
        public string itemDescription;

        public Sprite visual;
        public GameObject prefab;
    }
}