namespace MVsToolkit.Demo
{
    internal interface IDemoInterface 
    {
        public void DemoMethod();
    }

    [System.Serializable]
    internal struct InlineClass
    {
        public string name;
        public bool isValid;
    }
}