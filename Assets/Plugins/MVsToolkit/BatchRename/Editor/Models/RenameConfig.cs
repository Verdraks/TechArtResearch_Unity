using System;
using MVsToolkit.Attributes;
using UnityEngine;

namespace MVsToolkit.BatchRename.Editor
{
    [Serializable]
    public class RenameConfig
    {
        [SerializeReference] [SerializeReferenceDrawer]
        public IRenameOperation[] Operations;

        [SerializeReference] [SerializeReferenceDrawer]
        public IRenameRule[] Rules;
    }
}