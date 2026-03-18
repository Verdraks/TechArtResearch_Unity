using UnityEngine;

namespace MVsToolkit.Demo
{
    internal class DemoSerializeReference : MonoBehaviour
    {

        [Header("Settings")]

        [SerializeReference, Attributes.SerializeReferenceDrawer] private BaseClass[] m_DemoClassPrivateArray;
        [SerializeReference, Attributes.SerializeReferenceDrawer] public BaseClass DemoClass;
        [SerializeReference, Attributes.SerializeReferenceDrawer] private BaseClass m_DemoClassPrivate;
        
        [SerializeReference, Attributes.SerializeReferenceDrawer] public IInterfaceExample DemoInterfaceImpl;
    }


    [System.Serializable]
    public struct BaseStruct
    {
        public float SomeValue;
    }
    
    [System.Serializable]
    public abstract class BaseClass{}
    
    [System.Serializable]
    public class ChildClassA : BaseClass {}

    [System.Serializable]
    public class ChildClassB : BaseClass
    {
        public float SomeValue;
    }
    
    public interface IInterfaceExample {  }
    
    [System.Serializable]
    public class InterfaceImplA : IInterfaceExample {  }

    [System.Serializable]
    public class InterfaceImplB : IInterfaceExample
    {
        public float SomeValue;
    }
    
}