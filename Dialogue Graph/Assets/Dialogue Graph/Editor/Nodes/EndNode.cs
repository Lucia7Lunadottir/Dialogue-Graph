using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
namespace PG.DialogueGraphEditor
{
    // Пустая нода: ничего не показывает, срабатывают её Behaviours, и диалог закрывается
    [Serializable]
    public class EndNode : BaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddInputPort("in").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
            base.OnDefineOptions(context);
        }

        public override RuntimeBaseNode CreateRuntimeNode()
        {
            return new RuntimeEndNode();
        }

        public override void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
        }
    }
}
