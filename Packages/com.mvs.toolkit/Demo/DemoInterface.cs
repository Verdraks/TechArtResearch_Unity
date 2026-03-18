using UnityEngine;

namespace MVsToolkit.Demo
{
    internal class DemoInterface : MonoBehaviour, IDemoInterface 
    {
        [SerializeField] string debugText;

        public void DemoMethod()
        {
            UnityEngine.Debug.Log(debugText);
        }
    }
}