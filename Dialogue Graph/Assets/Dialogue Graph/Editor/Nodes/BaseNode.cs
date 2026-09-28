using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
namespace PG.DialogueGraphEditor
{
    [Serializable]
    public abstract class BaseNode : Node, IRuntimeExportable
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("Behaviours").AsVertical().WithCapacity(PortCapacity.Multi).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        public virtual string GetJsonData()
        {
            return "";
        }

        public virtual RuntimeBaseNode CreateRuntimeNode()
        {
            return null;
        }

        public virtual void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            
        }
    }
}