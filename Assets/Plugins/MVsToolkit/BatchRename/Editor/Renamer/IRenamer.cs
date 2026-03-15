using System.Collections.Generic;

namespace MVsToolkit.BatchRename.Editor
{
    public interface IRenamer
    {
        IEnumerable<RenameResult> Preview(IEnumerable<IRenameTarget> targets, RenameConfig config);
        void Apply(IEnumerable<RenameResult> results, RenameConfig config);
    }
}