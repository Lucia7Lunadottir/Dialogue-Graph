using PG.DialogueGraph;

namespace PG.DialogueGraphEditor
{
    public interface IRuntimeExportable
    {
        RuntimeBaseNode CreateRuntimeNode(); 
        void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context);
    }
}