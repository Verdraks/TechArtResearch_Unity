using UnityEngine;

namespace MVsToolkit.BatchRename.Editor
{
    public interface IRenameTarget
    {
        string Name { get; }
        string Path { get; }
        Object UnityObject { get; }

        void SetName(string newName);

        bool IsValidTarget();
    }
}