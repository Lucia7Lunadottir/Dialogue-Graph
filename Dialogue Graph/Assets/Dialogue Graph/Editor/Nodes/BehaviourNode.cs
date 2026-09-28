using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public abstract class BehaviourNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").AsVertical().WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        public abstract RuntimeNodeBehaviour CreateRuntimeBehaviour();

        public virtual void ExportToRuntime(RuntimeNodeBehaviour runtimeBehaviour, ExportContext context)
        {
        }
    }
}
